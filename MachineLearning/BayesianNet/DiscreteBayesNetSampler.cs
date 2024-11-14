using System.Diagnostics;

namespace CsharpALG.MachineLearning;

public abstract class DiscreteBayesNetSampler
{
    public DiscreteBayesNetSampler(DiscreteBayesNet bn)
    {
        this.bn = bn;
    }

    /// <summary>
    /// given observations stored in evidence, returns n (possibly duplicated) samples
    /// from the bayesian net. The samples are weighted.
    /// </summary>
    /// <param name="evidence">observed values of some bayesian nodes.</param>
    /// <param name="n"></param>
    /// <returns></returns>
    public (List<string>, Dictionary<List<string>, double>) Sample(Dictionary<string, string> evidence, int n)
    {
        Dictionary<List<string>, double> samples = new(new ListStringEqualityComparer());

        // get field name of the sampled values
        List<string> fieldNames = bn.TravelSequence
            .Select(x => x.Name)
            .Where(x => !evidence.ContainsKey(x)).ToList();

        if (fieldNames.Count == 0)
        {
            throw new Exception("Sampling failed since all nodes of the bayesian net are observed");
        }

        initializeState(evidence);
        for (int i = 0; i < n; ++i)
        {
            (var data, var weight) = sampleOne(evidence);
            samples[data] = samples.GetValueOrDefault(data) + weight;
        }

        return (fieldNames, samples);
    }

    protected abstract void initializeState(Dictionary<string, string> evidence);

    protected abstract (List<string>, double) sampleOne(Dictionary<string, string> evidence);

    protected DiscreteBayesNet bn;
}

public class ParticleFilteringBNSampler : DiscreteBayesNetSampler
{
    public ParticleFilteringBNSampler(DiscreteBayesNet bn) : base(bn)
    {

    }

    protected override (List<string>, double) sampleOne(Dictionary<string, string> evidence)
    {
        double weight = 1.0;
        List<string> datapoint = new();
        Dictionary<string, string> nodeValues = new(evidence);
        foreach (var node in bn.TravelSequence)
        {
            var parentValues = node.Parents.Select(x => nodeValues[x.Name]).ToList();

            if (evidence.ContainsKey(node.Name))
            {
                weight *= node.P(evidence[node.Name], parentValues);
            }
            else
            {
                var value = node.Sample(parentValues);
                datapoint.Add(value);
                nodeValues[node.Name] = value;
            }
        }

        return (datapoint, weight);
    }

    protected override void initializeState(Dictionary<string, string> evidence)
    {
        return;
    }
}

public class GibsBNSampler : DiscreteBayesNetSampler
{
    public GibsBNSampler(DiscreteBayesNet bn) : base(bn) { }

    protected override (List<string>, double) sampleOne(Dictionary<string, string> evidence)
    {
        // sampling all nodes
        List<string> point = new();
        foreach (var node in bn.TravelSequence)
        {
            if (!evidence.ContainsKey(node.Name))
            {
                var prob = calcMarkovBlancket(node, state!);
                state![node.Name] = prob.Sample();
                point.Add(state![node.Name]);
            }
        }

        return (point, 1);
    }

    protected override void initializeState(Dictionary<string, string> evidence)
    {
        state = completeState(evidence);
    }

    private DiscreteProbDist calcMarkovBlancket(
        DiscreteBayesNode node,
        Dictionary<string, string> nodeValues
    )
    {
        DiscreteProbDist prob = new(node.Name);

        // get value of parents
        var parentValues = node.ParentNodeNames.Select(x => nodeValues[x]).ToList();

        // get child node and values
        var children = node.Children;
        var childrenValue = children.Select(x => nodeValues[x.Name]).ToList();

        // get value's for children's parents
        (var spouses, var nodeIndices) = getSpouse(node, nodeValues);

        foreach (var x in node.Domain)
        {
            double w = node.P(x, parentValues);
            var numChildren = children.Count;
            for (int i = 0; i < numChildren; ++i)
            {
                var spouseValue = spouses[i];
                spouseValue[nodeIndices[i]] = x;
                var cvalue = childrenValue[i];
                var child = children[i];
                w *= child.P(cvalue, spouseValue);
            }

            prob[x] = w;
        }

        prob.Normalize();
        return prob;
    }

    // init a state using evidence
    // a state is a consistant assignment of node with the evidence
    Dictionary<string, string> completeState(Dictionary<string, string> evidence)
    {
        Dictionary<string, string> state = new(evidence);
        foreach (var node in bn.TravelSequence)
        {
            if (!evidence.ContainsKey(node.Name))
            {
                state[node.Name] = randomChoice(node.Domain);
            }
        }
        return state;
    }

    private static string randomChoice(string[] values)
    {
        int n = values.Length;
        var rand = new Random();
        int idx = rand.Next(n);
        return values[idx];
    }

    private static (List<List<string>>, List<int>) getSpouse(
        DiscreteBayesNode node, Dictionary<string, string> nodeValues)
    {
        List<List<string>> spouseValues = new();
        List<int> nodeIdx = new(); // idx of node in spouseValues
        foreach (var child in node.Children)
        {
            List<string> spouse = new();
            int numParent = child.Parents.Count;
            for (int i = 0; i < numParent; ++i)
            {
                string name = child.ParentNodeNames[i];
                spouse.Add(nodeValues[name]);

                if (name == node.Name)
                {
                    nodeIdx.Add(i);
                }
            }
            spouseValues.Add(spouse);
        }

        // Debug.Assert(spouseValues.Count > 0);
        Debug.Assert(spouseValues.Count == nodeIdx.Count);

        return (spouseValues, nodeIdx);
    }

    private Dictionary<string, string>? state;
}