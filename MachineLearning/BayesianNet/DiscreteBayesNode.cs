using System.Diagnostics;

namespace CsharpALG.MachineLearning;

/// <summary>
/// DiscreteBayesNode is a representation for conditional probility P(x | parents). It
/// is a part of Bayesian Network.
/// </summary>
public class DiscreteBayesNode
{
    // assumptions: conditional_probs not empty && all contained prob share the same domain
    public DiscreteBayesNode(string name, List<List<string>> parentValues, List<DiscreteProbDist> probs)
    {
        Name = name;
        Parents = new();
        Children = new();
        Conditional_probs = new(new ListStringEqualityComparer());
        foreach((var parentValue, var prob) in parentValues.Zip(probs))
        {
            Conditional_probs[parentValue] = prob;
        }
        Debug.Assert(Conditional_probs.Count > 0, $"cond prob for node {Name} is empty");
    }

    // compute P(x | parents)
    public double P(string x, List<string> parent_values)
    {
        var prob = Conditional_probs[parent_values];
        return prob[x];
    }

    // sampling a value from the node
    public string Sample(List<string> parent_values)
    {
        var prob = Conditional_probs[parent_values];
        return prob.Sample();
    }

    // retain the domain of current node.
    public string[] Domain {get => Conditional_probs.Values.First().Domain;}

    // get name of parents node
    public string[] ParentNodeNames {get => Parents.Select(x => x.Name).ToArray();}

    public string Name; // uinque name of the node
    public List<DiscreteBayesNode> Parents;
    public List<DiscreteBayesNode> Children;
    // List<string> stores values of parents with the order specified by parents
    public Dictionary<List<string>, DiscreteProbDist> Conditional_probs;
}

public class ListStringEqualityComparer: IEqualityComparer<List<string> >
{
    public bool Equals(List<string>? x, List<string>? y)
    {
        if (x is null) return y is null;
        if (y is null) return x is null;

        return x.SequenceEqual(y);
    }

    public int GetHashCode(List<string> x)
    {
        unchecked
        {
            int hash = 0;
            foreach(string str in x) {
                hash = hash * 31 + str.GetHashCode();
            }
            return hash;
        }
    }
}