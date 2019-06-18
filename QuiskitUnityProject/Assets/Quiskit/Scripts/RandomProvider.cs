using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomProvider : MonoBehaviour {

    private static readonly string _qasmSingleBooleanCode = "include \"qelib1.inc\"; qreg q[1]; creg c[1]; h q[0]; measure q[0] -> c[0];";

    [Header("Optional")]
    public QASMSession specificSession = null;

    public delegate void OnRandomBoolGenerated(bool generatedBool);
    
    public void GenerateBool(OnRandomBoolGenerated onRandomBoolGenerated) {
        QASMSession executionSession = specificSession ?? QASMSession.instance;
        executionSession.ExecuteCode(_qasmSingleBooleanCode, (response) => {
            if (response.ContainsKey(1) && response[1] > 512) {
                onRandomBoolGenerated(true);
            } else {
                onRandomBoolGenerated(false);
            }
        });
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Bool")]
    private void TryGenerateBool() {
        GenerateBool((b) => Debug.Log($"Generated bool: {b}"));
    }
#endif

}
