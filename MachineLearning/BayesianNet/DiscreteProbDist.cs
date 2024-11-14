using System.Diagnostics;
using System.Text;

namespace CsharpALG.MachineLearning;

/// <summary>
/// A discrete probability distribution.
/// usage:
/// example1:
///     var p = new DiscreteProbDist("Flip")
///     P["H"] = 0.125;
///     P["T"] = 0.375;
///
/// example2:
///     var p = new DiscreteProbDist("Flip", new Dictionary<string, int>({{"a", 123}, {"b", 456}}))
/// </summary>
public class DiscreteProbDist
{
    public DiscreteProbDist(string name)
    {
        this.name = name;
        variableValues = new();
        varValue2Index = new();
        probs = new();
        cdf = [0];
        n = 0;
    }

    public DiscreteProbDist(string name, Dictionary<string, int> freqs)
    {
        this.name = name;
        variableValues = new(freqs.Count);
        varValue2Index = new();
        probs = new(freqs.Count);

        foreach (var kv in freqs)
        {
            variableValues.Add(kv.Key);
            varValue2Index.Add(kv.Key, n);
            probs.Add(kv.Value);
            n += 1;
        }

        Normalize();
        recomputeCDF();
    }

    public DiscreteProbDist(string name, string[] domain, double[] probs)
    {
        Debug.Assert(domain.Length == probs.Length);

        this.name = name;
        variableValues = new(domain.Length);
        varValue2Index = new();
        this.probs = new List<double>(probs);

        foreach (string varValue in domain)
        {
            variableValues.Add(varValue);
            varValue2Index.Add(varValue, n);
            n += 1;
        }

        Normalize();
        recomputeCDF();
    }

    // indexer to get or set the prob (the prob may not normalized)
    public double this[string varName]
    {
        get
        {
            int idx = varValue2Index[varName];
            return probs[idx];
        }
        set
        {
            // case 1: when varName is already in the dict
            if (varValue2Index.ContainsKey(varName))
            {
                int idx = varValue2Index[varName];
                probs[idx] = value;
            }
            // case 2: when varName is brandNew
            else
            {
                varValue2Index[varName] = n;
                n += 1;
                variableValues.Add(varName);
                probs.Add(value);
            }
            // flush cdf
            cdf = null;
        }
    }

    // sample one element from the domain, note that the index is returned
    public int SampleIndex()
    {
        if (n == 0)
            throw new InvalidDataException($"sampling from an empty probility {name}");

        recomputeCDF();
        double coin = new Random().NextDouble();
        int i = 1;
        while (i <= n && cdf![i] <= coin)
            i += 1;

        Debug.Assert(i <= n);
        return i - 1;
    }

    public string Sample()
    {
        return Domain[SampleIndex()];
    }

    public void Normalize()
    {
        if (n == 0) return;

        double tot = probs.Sum();
        if (tot < 1e-9)
            throw new InvalidDataException($"normalizer of prob {name} is zero");

        for (int i = 0; i < n; ++i)
        {
            probs[i] /= tot;
        }

        cdf = null;
    }

    public override string ToString(){
        StringBuilder reprbuilder = new();
        reprbuilder.Append("Prob(");
        for (int i=0; i < n; ++i) {
            reprbuilder.Append($"{Domain[i]}:{Probs[i]}, ");
        }
        reprbuilder.Append(")");
        return reprbuilder.ToString();
    }

    public string Name { get => name; }
    public string[] Domain { get => variableValues.ToArray(); }
    public double[] Probs { get => probs.ToArray(); }
    public int NumEntries {get => n;}

    private void recomputeCDF()
    {
        if (cdf is not null) return;
        if (n == 0)
        {
            cdf = [0];
            return;
        }

        Normalize();
        cdf = [0];
        for (int i = 1; i <= n; ++i)
        {
            cdf.Add(cdf[i - 1] + probs[i - 1]);
        }
        Debug.Assert(Math.Abs(cdf.Last() - 1.0) < 1e-9);
        Debug.Assert(cdf.Count == n + 1);
    }

    private string name;
    private List<string> variableValues;
    private Dictionary<string, int> varValue2Index;
    private List<double> probs;
    private List<double>? cdf; // cumulative distribution for [0, x); set to null when prob changes
    private int n;
}

// Code for test DiscreteProbDist
// test for set & get
// test for initialize
// test for sampling
class TestDiscreteProbDist
{
    static public void TestSetAndGet()
    {
        var prob = new DiscreteProbDist("x");
        prob["A"] = 1.0;
        prob["B"] = 2.0;
        prob["C"] = 0.0;
        prob["D"] = 4.0;
        prob.Normalize();

        Debug.Assert(Math.Abs(prob["A"] - 1.0 / 7.0) < 1e-9);
        Debug.Assert(Math.Abs(prob["B"] - 2.0 / 7.0) < 1e-9);
        Debug.Assert(Math.Abs(prob["C"] - 0.0 / 7.0) < 1e-9);
        Debug.Assert(Math.Abs(prob["D"] - 4.0 / 7.0) < 1e-9);

        for (int i=0; i < 4; ++i) {
            Console.Write($"{prob.Domain[i]}: {prob.Probs[i]}");
        }
        Console.WriteLine();
    }

    static public void TestInitByFreqs()
    {
        var freqs = new Dictionary<string, int>(){
            {"Alice", 3}, {"Bob", 4}, {"Jack", 0},
            {"Volk", 7}, {"Xmeku", 6}, {"VikeLK", 9}
        };

        var probs = new DiscreteProbDist("x", freqs);
        string[] correctDomain = ["Alice", "Bob", "Jack", "Volk", "Xmeku", "VikeLK"];
        double[] correctProb = [3.0/29, 4.0/29, 0.0, 7.0/29, 6.0/29, 9.0/29];
        Debug.Assert(probs.NumEntries == correctDomain.Length);
        for (int i=0; i < probs.NumEntries; ++i) {
            Debug.Assert(probs.Domain[i] == correctDomain[i]);
            Debug.Assert(Math.Abs(probs.Probs[i] - correctProb[i]) < 1e-9);
            Debug.Assert(Math.Abs(probs[probs.Domain[i]] - correctProb[i]) < 1e-9);
        }
    }

    static public void TestSampling()
    {
        var freqs = new Dictionary<string, int>(){
            {"Alice", 3}, {"Bob", 4}, {"Jack", 0},
            {"Volk", 0}, {"Xmeku", 6}, {"VikeLK", 0}, {"Z", 2}
        };
        var probs = new DiscreteProbDist("", freqs);

        int n = 1000;
        int[] samplefreqs = new int[freqs.Count];
        for (int i=0; i < n; ++i) {
            samplefreqs[probs.SampleIndex()] += 1;
        }

        for (int i=0; i < probs.NumEntries; ++i) {
            Debug.Assert(
                Math.Abs(probs.Probs[i] - samplefreqs[i] / ((double) n)) < 1e-2
            );
        }
    }
}