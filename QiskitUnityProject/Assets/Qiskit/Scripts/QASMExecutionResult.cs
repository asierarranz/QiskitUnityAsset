using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class QASMExecutionResult {

    private Dictionary<int, int> _result = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> result => _result;

    private List<int> _rawResult = new List<int>();
    public IReadOnlyList<int> rawResult => _rawResult;

    public int maxKey { get; private set; } = 0;
    public int maxValue { get; private set; } = 0;

    public void SimulateRawResult() {
        _rawResult = new List<int>();

        foreach (KeyValuePair<int, int> pair in _result) {
            for (int i = 0; i < pair.Value; i++) {
                _rawResult.Add(pair.Key);
            }
        }

        Shuffle(_rawResult);
    }

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

    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle(List<int> list) {
        int count = list.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i) {
            int r = UnityEngine.Random.Range(i, count);
            int tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}
