using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class GateConfig {
    public string name;
    public List<string> parameters;
    public string qasm_def;
}
