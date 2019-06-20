using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class BackendConfiguration {

    public string backend_name;
    public string backend_version;
    public List<string> basis_gates;
    public bool conditional;
    public List<int> coupling_map;
    public string description;
    public List<GateConfig> gates;
    public string library_dir;
    public bool local;
    public int max_shots;
    public bool memory;
    public int n_qubits;
    public bool open_pulse;
    public bool simulator;
    public string url;

    /* {
     "backend_name":"qasm_simulator",
     "backend_version":"0.2.1",
     "basis_gates":["u1","u2","u3","cx","cz","id","x","y","z","h","s","sdg","t","tdg","ccx","swap","multiplexer","snapshot","unitary","reset","initialize","kraus"],
     "conditional":true,
     "coupling_map":null,
     "description":"A C++ simulator with realistic noise for qobj files",
     "gates":[{"name":"TODO","parameters":[],"qasm_def":"TODO"}],
     "library_dir":"D:\\Aplicaciones\\Anaconda\\envs\\qiskit_env\\lib\\site-packages\\qiskit\\providers\\aer\\backends",
     "local":true,
     "max_shots":100000,
     "memory":true,
     "n_qubits":29,
     "open_pulse":false,
     "simulator":true,
     "url":"https://github.com/Qiskit/qiskit-aer"}}
     */
    public static BackendConfiguration CreateFromJSON(string jsonString) {
        return JsonUtility.FromJson<BackendConfiguration>(jsonString);
    }
}
