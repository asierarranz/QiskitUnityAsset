using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class QASMExecutable {

    public string code { get; private set; }

    public int shots { get; private set; }
    public bool useShots => shots > 0;


    public QASMExecutable(string code, int shots = 0) {
        this.code = code;
        this.shots = shots;
    }


    //  User-defined conversion from string to QASMExecutable
    public static implicit operator QASMExecutable(string code) {
        return new QASMExecutable(code);
    }
}
