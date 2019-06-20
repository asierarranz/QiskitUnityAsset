using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class QASMExecutionResult {

    private Dictionary<int, int> _result = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> result => _result;

    public List<int> _rawResult = new List<int>();
    public IReadOnlyList<int> rawResult => _rawResult;

    public int maxKey { get; private set; } = 0;
    public int maxValue { get; private set; } = 0;

    public void Add(int key, int value) {
        _result.Add(key, value);
        if (maxValue < value) {
            maxValue = value;
            maxKey = key;
        }
    }

    public void Add(int key) {
        _rawResult.Add(key);

        int value = 1;

        if (_result.ContainsKey(key)) {
            value = ++_result[key];
        } else {
            _result.Add(key, value);
        }

        if (maxValue < value) {
            maxValue = value;
            maxKey = key;
        }
    }
}
