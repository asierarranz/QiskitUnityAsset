using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QASMSession : MonoBehaviour {

    [Header("IBMQ Configuration")]
    public string apiTokenString = "";
    // Server To Request
    public string server = "http://localhost:8001";
    // Server config
    [SerializeField]
    private int _maxQBitAvailable = 5;
    public int maxQubitAvailable => _maxQBitAvailable;

    private BackendConfiguration _backendConfig;
    private UnityWebRequestAsyncOperation _backendConfigRequest;

    [Header("Debug")]
    public bool verbose = false;


    public delegate void OnExecuted(QASMExecutionResult result);
    public delegate void OnConfigurationAvailable(BackendConfiguration configuration);

    private delegate void OnJsonResult(string json);

    public static QASMSession _instance;
    public static QASMSession instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<QASMSession>();
                if (_instance == null) {
                    Debug.LogWarning("No QASMSession found. Creating new GameObject to use it.");
                    GameObject go = new GameObject("QASMSession");
                    _instance = go.AddComponent<QASMSession>();
                }
            }
            return _instance;
        }
    }

    private void Awake() {
        _instance = this;
    }

    public static void Execute(string qasmCode, OnExecuted onExecuted) => instance?.ExecuteCode(qasmCode, onExecuted);
    public static void ExecuteRawResult(string qasmCode, OnExecuted onExecuted) => instance?.ExecuteCodeRawResult(qasmCode, onExecuted);

    /// <summary>
    /// The method makes an asynchronous request of the backend configuration data.
    /// When the request is answered, the <see cref="OnConfigurationAvailable"/> 
    /// callback will be called.
    /// The <see cref="BackendConfiguration"/> is cached in order to improve subsequent calls.
    /// To clear cached configuration use the <see cref="ClearBackendConfig"/> method.
    /// </summary>
    /// <param name="onConfigurationAvailable">The callback called when the configuration is available</param>
    public void RequestBackendConfig(OnConfigurationAvailable onConfigurationAvailable) {

        if (_backendConfig != null) {
            onConfigurationAvailable(_backendConfig);
            return;
        }

        // Request
        UnityWebRequest www;
        // Check if other config request is running
        if (_backendConfigRequest != null) {
            www = _backendConfigRequest.webRequest;
        } else {
            www = UnityWebRequest.Get(server + "/api/backend/configuration");
            _backendConfigRequest = www.SendWebRequest();
        }
            
        _backendConfigRequest.completed += (_) => {
            _backendConfigRequest = null;
#if UNITY_EDITOR
            if (verbose) Debug.Log("text: " + www.downloadHandler.text);
#endif
            if (www.responseCode == 200) {
                _backendConfig = BackendConfiguration.CreateFromJSON(GetResultJSON(www.downloadHandler.text));
                onConfigurationAvailable(_backendConfig);

            } else {
                string responseCodeMessage = $"Response Code: {www.responseCode}";
                if (www.responseCode == 500) {
                    responseCodeMessage += " - Internal server error.";
                    if (!string.IsNullOrEmpty(apiTokenString)) {
                        responseCodeMessage += "\nIf you are using simulator, consider not to use apiTokenString.";
                    }
                }

                Debug.LogError(responseCodeMessage);
                throw new System.Exception(responseCodeMessage);
            }

        };
    }

    /// <summary>
    /// Execute the given <code>qasmExe.code</code> launching <code>qasmExe.shots</code> executions.
    /// When the result is ready, the <code>onExecuted</code> callback will be called.
    /// The <see cref="OnExecuted"/> will recieve <see cref="QASMExecutionResult"/> 
    /// with the per shot results as rawResult.
    /// If the backends does not supports memory feature, the raw result will be simulated with
    /// accumulated results.
    /// </summary>
    /// <param name="qasmExe">Executable configuration</param>
    /// <param name="onExecuted">The callback called when execution ends</param>
    public void ExecuteCodeRawResult(QASMExecutable qasmExe, OnExecuted onExecuted) {
        RequestBackendConfig((_) => {
            GenericExecution(qasmExe, useMemory: _backendConfig.supportsMemory, (jsonResult) => {
                if (_backendConfig.supportsMemory) {
                    onExecuted(ReadRawDataJSON(jsonResult));
                } else {
                    QASMExecutionResult result = ReadCountJSON(jsonResult);
                    result.SimulateRawResult();
                    onExecuted(result);
                }
            });
        });
    }

    /// <summary>
    /// Execute the given <code>qasmExe.code</code> launching <code>qasmExe.shots</code> executions.
    /// When the result is ready, the <code>onExecuted</code> callback will be called.
    /// The <see cref="OnExecuted"/> will recieve <see cref="QASMExecutionResult"/> 
    /// with the accumulated result.
    /// </summary>
    /// <param name="qasmExe">Executable configuration</param>
    /// <param name="onExecuted">The callback called when execution ends</param>
    public void ExecuteCode(QASMExecutable qasmExe, OnExecuted onExecuted) {
        // Request is not needed yet, see "ExecuteCodeRawResult" implementation in case of future changes
        GenericExecution(qasmExe, useMemory: false, (jsonResult) => {
            onExecuted(ReadCountJSON(jsonResult));
        });
    }

    private void GenericExecution(QASMExecutable qasmExe, bool useMemory, OnJsonResult onJsonResult) {
        // API request
        List<IMultipartFormSection> formData = new List<IMultipartFormSection> {
            // QASM parameter
            new MultipartFormDataSection("qasm", qasmExe.code),
            new MultipartFormDataSection("memory", useMemory ? "True" : "False")
        };
        // Api token parameter
        if (apiTokenString != "") {
            formData.Add(new MultipartFormDataSection("api_token", apiTokenString));
        }
        // Number of shots
        if (qasmExe.useShots) {
            formData.Add(new MultipartFormDataSection("shots", $"{qasmExe.shots}"));
        }
        
        // Request
        UnityWebRequest www = UnityWebRequest.Post(server + "/api/run/qasm", formData);
        www.SendWebRequest().completed += (_) => {
#if UNITY_EDITOR
            if (verbose) Debug.Log("text: " + www.downloadHandler.text);
#endif
            if (www.responseCode == 200) { // Is OK
                onJsonResult(GetResultJSON(www.downloadHandler.text));

            } else { // ON ERROR
                string responseCodeMessage = $"Response Code: {www.responseCode}";
                if (www.responseCode == 500) {
                    responseCodeMessage += " - Internal server error.";
                    if (!string.IsNullOrEmpty(apiTokenString)) {
                        responseCodeMessage += "\nIf you are using simulator, consider not to use apiTokenString.";
                    }
                }

                Debug.LogError(responseCodeMessage);
                throw new System.Exception(responseCodeMessage);
            }
        };
    }
    
    /// <summary>
    /// Clean result json.
    /// </summary>
    /// <param name="jsonText"></param>
    /// <returns></returns>
    private static string GetResultJSON(string jsonText) {
        if (string.IsNullOrEmpty(jsonText)) return null;

        char[] initialCharsToTrim = { '{', ' ', '\n'};
        jsonText = jsonText.Trim(initialCharsToTrim);
        jsonText = jsonText.Substring(jsonText.IndexOf(':') + 1);
        jsonText = jsonText.Substring(0, jsonText.Length - 1);

        // clean start and end
        char[] finalCharsToTrim = { ' ', '\n' };
        jsonText = jsonText.Trim(finalCharsToTrim);

        return jsonText;
    }
    
    /// <summary>
    /// Extracts from a clean json (see <see cref="GetResultJSON(string)"/>)
    /// the accumulated qasm results.
    /// </summary>
    /// <param name="jsonText"></param>
    /// <returns></returns>
    private static QASMExecutionResult ReadCountJSON(string jsonText) {
        if (string.IsNullOrEmpty(jsonText)) return null;
        
        jsonText = jsonText.TrimStart('{');
        jsonText = jsonText.TrimEnd('}');

        string[] rawResultCollection = jsonText.Split(',');
        QASMExecutionResult executionResult = new QASMExecutionResult();

        foreach (string rawResult in rawResultCollection) {
            string[] keyValue = rawResult.Split(':');
            keyValue[0] = keyValue[0].Trim('"');
            executionResult.Add(System.Convert.ToInt32(keyValue[0], 2), System.Convert.ToInt32(keyValue[1]));
        }
        
        return executionResult;
    }

    /// <summary>
    /// Extracts from a clean json (see <see cref="GetResultJSON(string)"/>)
    /// the pershot qasm results.
    /// </summary>
    /// <param name="jsonText"></param>
    /// <returns></returns>
    private static QASMExecutionResult ReadRawDataJSON(string jsonText) {
        if (string.IsNullOrEmpty(jsonText)) return null;

        jsonText = jsonText.TrimStart('[');
        jsonText = jsonText.TrimEnd(']');
        jsonText = jsonText.Replace("\"", "");

        string[] rawResultCollection = jsonText.Split(',');
        QASMExecutionResult executionResult = new QASMExecutionResult();

        foreach (string rawResult in rawResultCollection) {
            executionResult.Add(System.Convert.ToInt32(rawResult, 2));
        }

        return executionResult;
    }

    /// <summary>
    /// Clear cached backend configurations and currently running requests.
    /// </summary>
    [ContextMenu("Clear BackendConfig")]
    public void ClearBackendConfig() {
        _backendConfig = null;
        _backendConfigRequest?.webRequest?.Dispose();
        _backendConfigRequest = null;
    }

#if UNITY_EDITOR
    [ContextMenu("Get BackendConfig")]
    private void GetBackendConfig() {
        RequestBackendConfig((conf) => {
            Debug.Log(conf.backendName);
        });
    }
#endif
}
