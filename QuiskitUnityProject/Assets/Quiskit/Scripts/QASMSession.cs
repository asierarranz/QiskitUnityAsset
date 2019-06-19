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

    [Header("Debug")]
    public bool verbose = false;
    public bool useShots = false;
    public bool useMemory = false;

    public delegate void OnExecuted(QASMExecutionResult result);
    public delegate void OnConfigurationAvailable(BackendConfiguration configuration);

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


    [ContextMenu("GetBackendConfig")]
    public void GetBackendConfig() {
        GetBackendConfig((conf) => {
            Debug.Log(conf.backend_name);
        });
    }
    public void GetBackendConfig(OnConfigurationAvailable onExecuted) {

        // API request
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        // Request
        UnityWebRequest www = UnityWebRequest.Get(server + "/api/backend/configuration");
        www.SendWebRequest().completed += (_) => {
            if (verbose) Debug.Log("text: " + www.downloadHandler.text);

            if (www.responseCode == 200) {
                BackendConfiguration config = BackendConfiguration.CreateFromJSON(getResultJSON(www.downloadHandler.text));
                onExecuted(config);
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

    public void ExecuteCode(string qasmCode, OnExecuted onExecuted) {

        // API request
        List<IMultipartFormSection> formData = new List<IMultipartFormSection> {
            // QASM parameter
            new MultipartFormDataSection("qasm", qasmCode)
        };
        // Api token parameter
        if (apiTokenString != "") {
            formData.Add(new MultipartFormDataSection("api_token", apiTokenString));
        }
        if (useShots) {
            formData.Add(new MultipartFormDataSection("shots", "64"));
        }
        if (useMemory) {
            formData.Add(new MultipartFormDataSection("memory", "True"));
        }

        // Request
        UnityWebRequest www = UnityWebRequest.Post(server + "/api/run/qasm", formData);
        www.SendWebRequest().completed += (_) => {
            if (verbose) Debug.Log("text: " + www.downloadHandler.text);

            if (www.responseCode == 200) {
                onExecuted(readJSON(www.downloadHandler.text));
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

    // Response: { "result":{ "0":539,"1":485} }
    string getResultJSON(string jsonText) {
        if (string.IsNullOrEmpty(jsonText)) return null;

        char[] charsToTrim = { '{', ' ', '\n'};
        jsonText = jsonText.Trim(charsToTrim);
        jsonText = jsonText.Substring(jsonText.IndexOf('{'));
        jsonText = jsonText.Substring(0, jsonText.Length - 1);

        return jsonText;
    }

    // Response: { "result":{ "0":539,"1":485} }
    QASMExecutionResult readJSON(string jsonText) {
        if (string.IsNullOrEmpty(jsonText)) return null;
        char[] charsToTrim = { '{', ' ', '\n', '}' };
        jsonText = jsonText.Trim(charsToTrim);
        jsonText = jsonText.Substring(jsonText.IndexOf('{') + 1);
        string[] rawResultCollection = jsonText.Split(',');
        QASMExecutionResult executionResult = new QASMExecutionResult();

        foreach (string rawResult in rawResultCollection) {
            string[] keyValue = rawResult.Split(':');
            keyValue[0] = keyValue[0].Trim('"');
            executionResult.Add(System.Convert.ToInt32(keyValue[0], 2), System.Convert.ToInt32(keyValue[1]));
        }
        
        return executionResult;
    }
}
