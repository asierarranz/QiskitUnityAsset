using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GateConfig {

#pragma warning disable CS0649 // "attribute never assigned" warning 
    // (Assigned at "CreateFromJson" method on BackendConfiguration)

    [SerializeField]
    private string name;
    public string configName => name;

    [SerializeField]
    private List<string> parameters;
    public IReadOnlyList<string> parameterList => parameters;

    [SerializeField]
    private string qasm_def;
    public string qasmDef => qasm_def;

#pragma warning restore CS0649 // END "attribute never assigned" warning
}
