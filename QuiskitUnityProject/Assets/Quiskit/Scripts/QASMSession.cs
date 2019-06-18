using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QASMSession : MonoBehaviour {

    [Header("IBMQ Configuration")]
    public string apiTokenString = "";
    // Server To Request
    public string server = "http://localhost:8001";

    public delegate void OnExecuted(Dictionary<int, int> result);

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

    void ExecuteCode(string qasmCode, OnExecuted onExecuted) {

        // API request
        List<IMultipartFormSection> formData = new List<IMultipartFormSection> {
            // QASM parameter
            new MultipartFormDataSection("qasm", qasmCode)
        };
        // Api token parameter
        if (apiTokenString != "") {
            formData.Add(new MultipartFormDataSection("api_token", apiTokenString));
        }

        // Request
        UnityWebRequest www = UnityWebRequest.Post(/*"http://51.15.128.250:8001/api/run/qasm"*/ server + "/api/run/qasm", formData);
        www.SendWebRequest().completed += (_) => {
            Debug.Log("text: " + www.downloadHandler.text);

            if (www.responseCode == 200) {
                onExecuted(readJSON(www.downloadHandler.text));
            } else {
                string responseCodeMessage = "";
                if (www.responseCode == 500) {
                    responseCodeMessage = "Internal server error.";
                    if (!string.IsNullOrEmpty(apiTokenString)) {
                        responseCodeMessage += "If you are using simulator, consider not to use apiTokenString.";
                    }
                } else {
                    responseCodeMessage = $"Response Code: {www.responseCode}";
                }

                Debug.LogError(responseCodeMessage);
                onExecuted(null);
            }
        };

    }
    
    // Response: { "result":{ "0":539,"1":485} }
    Dictionary<int, int> readJSON(string jsonText) {
        if (string.IsNullOrEmpty(jsonText)) return null;
        char[] charsToTrim = { '{', ' ', '\n', '}' };
        jsonText = jsonText.Trim(charsToTrim);
        jsonText = jsonText.Substring(jsonText.IndexOf('{') + 1);
        string[] rawResultCollection = jsonText.Split(',');
        Dictionary<int, int> table = new Dictionary<int, int>();

        foreach (string rawResult in rawResultCollection) {
            string[] keyValue = rawResult.Split(':');
            keyValue[0] = keyValue[0].Trim('"');
            table.Add(System.Convert.ToInt32(keyValue[0], 2), System.Convert.ToInt32(keyValue[1]));
        }
        
        return table;
    }
}
