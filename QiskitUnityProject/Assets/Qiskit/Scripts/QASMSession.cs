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
    

    public void RequestBackendConfig(OnConfigurationAvailable onExecuted) {

        if (_backendConfig != null) {
            onExecuted(_backendConfig);
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
                _backendConfig = BackendConfiguration.CreateFromJSON(getResultJSON(www.downloadHandler.text));
                onExecuted(_backendConfig);

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
    
    public void ExecuteCodeRawResult(QASMExecutable qasmExe, OnExecuted onExecuted) {
        RequestBackendConfig((_) => {
            GenericExecution(qasmExe, useMemory: _backendConfig.supportsMemory, (jsonResult) => {
                if (_backendConfig.supportsMemory) {
                    onExecuted(readRawDataJSON(jsonResult));
                } else {
                    onExecuted(readCountJSON(jsonResult));
                }
            });
        });
    }

    public void ExecuteCode(QASMExecutable qasmExe, OnExecuted onExecuted) {
        // Request is not needed yet, see "ExecuteCodeRawResult" implementation in case of future changes
        GenericExecution(qasmExe, useMemory: false, (jsonResult) => {
            onExecuted(readCountJSON(jsonResult));
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
                onJsonResult(getResultJSON(www.downloadHandler.text));

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

    // Response: { "result":{ "0":539,"1":485} }
    private static string getResultJSON(string jsonText) {
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

    // Response: { "result":{ "0":539,"1":485} }
    private static QASMExecutionResult readCountJSON(string jsonText) {
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
    
    private static QASMExecutionResult readRawDataJSON(string jsonText) {
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

#if UNITY_EDITOR
    [ContextMenu("Get BackendConfig")]
    public void GetBackendConfig() {
        RequestBackendConfig((conf) => {
            Debug.Log(conf.backendName);
        });
    }

    [ContextMenu("Clear BackendConfig")]
    public void ClearBackendConfig() {
        _backendConfig = null;
        _backendConfigRequest = null;
    }
#endif
}
