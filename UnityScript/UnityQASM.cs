using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnityQASM : MonoBehaviour {
    public bool launchQASM;
    public int collapsed;
    void Start() { }

    void Update() {
        if (launchQASM) {
            launchQASM = false;
            StartCoroutine(SendRequest());
        }
    }

    [ContextMenu("Execute")]
    private void Launch() {
        StartCoroutine(SendRequest());
    }


    IEnumerator SendRequest() {

        // Must be set Qiskit IBMQ Api token for using real devices
        string apiTokenString = "";

        // Example with the Hadamard gate
        string qasmString = "include \"qelib1.inc\"; qreg q[1]; creg c[1]; h q[0]; measure q[0] -> c[0];";
        Debug.Log("Input QASM String: " + qasmString);

        // API request
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        // QASM parameter
        formData.Add(new MultipartFormDataSection("qasm", qasmString));
        // Api token parameter
        if (apiTokenString != "") {
            formData.Add(new MultipartFormDataSection("api_token", apiTokenString));
        }

        // Request
        UnityWebRequest www = UnityWebRequest.Post("http://51.15.128.250:8001/api/run/qasm", formData);
        yield return www.SendWebRequest();
        Debug.Log("Response: " + www.downloadHandler.text);
        collapsed = readJSON(www.downloadHandler.text);

    }

    // Response: { "result":{ "0":539,"1":485} }
    int readJSON(string jsonText) {
        if (string.IsNullOrEmpty(jsonText)) return -1;

        string hits = jsonText.Split('}')[0].Split('{')[2].Split(':')[2];

        return System.Convert.ToInt32(hits) > 512 ? 1 : 0;
    }
}
