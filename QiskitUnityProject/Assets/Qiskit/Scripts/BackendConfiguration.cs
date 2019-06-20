using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class BackendConfiguration {

#pragma warning disable CS0649 // "attribute never assigned" warning
    // (Assigned at "CreateFromJson" method)

    [SerializeField]
    private string backend_name;
    public string backendName => backend_name;

    [SerializeField]
    private string backend_version;
    public string backendVersion => backend_version;

    [SerializeField]
    private List<string> basis_gates;
    public IReadOnlyList<string> basisGates => basis_gates;

    [SerializeField]
    private bool conditional;
    public bool supportsConditional => conditional;

    [SerializeField]
    private List<List<int>> coupling_map;
    public IReadOnlyList<List<int>> couplingMap => coupling_map;

    [SerializeField]
    private string description;
    public string backendDescription => description;

    [SerializeField]
    private List<GateConfig> gates;
    public IReadOnlyList<GateConfig> gatesConfig => gates;

    [SerializeField]
    private string library_dir;
    public string libraryDir => library_dir;

    [SerializeField]
    private bool local;
    public bool isLocal => local;

    [SerializeField]
    private int max_shots;
    public int maxShots => max_shots;

    [SerializeField]
    private bool memory;
    public bool supportsMemory => memory;

    [SerializeField]
    private int n_qubits;
    public int qubitsCount => n_qubits;

    [SerializeField]
    private bool open_pulse;
    public bool supportsOpenPulse => open_pulse;

    [SerializeField]
    private bool simulator;
    public bool isSimulator => simulator;

    [SerializeField]
    private string url;
    public string repositoryUrl => url;

#pragma warning restore CS0649 // END "attribute never assigned" warning


    /* 
    {
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
         "url":"https://github.com/Qiskit/qiskit-aer"
     }
     */
    public static BackendConfiguration CreateFromJSON(string jsonString) {
        return JsonUtility.FromJson<BackendConfiguration>(jsonString);
    }
}
