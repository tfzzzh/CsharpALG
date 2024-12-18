using System.Data;
using System.Diagnostics;

namespace CsharpALG.MachineLearning;

public class DiscreteBayesNet
{
    public DiscreteBayesNet()
    {
        name2Node = new();
        topological_seq = new();
    }

    // build graph from specification
    public void BuildFrom(List<BayesNodeSpec> specs)
    {
        name2Node.Clear();
        topological_seq.Clear();

        if (specs.Count == 0)
        {
            return;
        }

        try
        {
            // build node from the specs when duplicate names found raise exception
            buildNodes(specs);

            // insert parents to correct position
            linkNodes(specs);

            // traverse to build topological_seq, raise exception when the graph contains a loop
            calcTravelSequence();

            // check integrity of the conditional probability
            checkNumberOfCondProb();
        }
        catch
        {
            name2Node.Clear();
            topological_seq.Clear();
            throw;
        }
    }

    private void checkNumberOfCondProb()
    {
        foreach (var node in name2Node.Values)
        {
            int realNumEntries = node.Conditional_probs.Count;
            int idealNumEntries = node.Parents.Count > 0 ? node.Parents.Select(x => x.Domain.Length)
                .Aggregate((a, b) => a * b) : 1;

            if (realNumEntries != idealNumEntries)
            {
                throw new Exception(
                    $"should specifying {idealNumEntries} condition prob for node {node.Name}" +
                    $" but {realNumEntries} are give"
                );
            }
        }
    }

    private void linkNodes(List<BayesNodeSpec> specs)
    {
        foreach (var spec in specs)
        {
            var node = name2Node[spec.Name];
            foreach (string parName in spec.Parents)
            {
                var parNode = name2Node[parName];
                node.Parents.Add(parNode);
                parNode.Children.Add(node);
            }
        }
    }

    private void buildNodes(List<BayesNodeSpec> specs)
    {
        foreach (var spec in specs)
        {
            var node = new DiscreteBayesNode(spec.Name, spec.ParentValues, spec.Probs);

            if (name2Node.ContainsKey(spec.Name))
            {
                throw new DuplicateNameException($"node name {spec.Name} in specs is duplicated");
            }

            name2Node[spec.Name] = node;
        }
    }


    // Get a node from the net using string as indexer
    public DiscreteBayesNode GetNode(string nodeName)
    {
        if (!name2Node.ContainsKey(nodeName))
        {
            throw new KeyNotFoundException($"node {nodeName} not found in the bayesian network");
        }
        return name2Node[nodeName];
    }

    // when failed, the graph contains a loop
    private void calcTravelSequence()
    {
        topological_seq.Clear();
        Dictionary<DiscreteBayesNode, char> color = new();
        foreach (var node in name2Node.Values)
        {
            if (!color.ContainsKey(node))
            {
                bool success = _calcTravelSequence(node, color);

                if (!success)
                {
                    throw new Exception("BayesianNet cannot have loop in specs");
                }
            }
        }
        topological_seq.Reverse();
    }

    bool _calcTravelSequence(DiscreteBayesNode node, Dictionary<DiscreteBayesNode, char> color)
    {
        color[node] = 'g';

        foreach (var child in node.Children)
        {
            bool success = true;
            if (!color.ContainsKey(child))
            {
                success = _calcTravelSequence(child, color);
            }
            else if (color[child] == 'g')
            {
                success = false;
            }

            if (!success) return false;
        }

        color[node] = 'b';
        topological_seq.Add(node);
        return true;
    }

    // return probablity of the evidence when margin out the unobserved values
    public double Marginalize(Dictionary<string, string> evidence)
    {
        return sumProduct(0, evidence);
    }

    // assumption: nodeName not in evidence && evidence is valid
    // compute
    public DiscreteProbDist InferPosterior(string nodeName, Dictionary<string, string> evidence)
    {
        var node = GetNode(nodeName);
        DiscreteProbDist prob = new DiscreteProbDist(nodeName);

        foreach (var val in node.Domain)
        {
            evidence.Add(nodeName, val);
            prob[val] = sumProduct(0, evidence);
            evidence.Remove(nodeName);
        }

        prob.Normalize();
        return prob;
    }

    double sumProduct(int idx, Dictionary<string, string> evidence)
    {
        if (idx >= topological_seq.Count) return 1.0;

        var node = topological_seq[idx];
        // get value of parents for node from evidence
        List<string> parentValue = new();
        foreach (var parName in node.ParentNodeNames)
        {
            parentValue.Add(evidence[parName]);
        }

        var result = 0.0;
        // when value of the node specified returns product factor
        if (evidence.ContainsKey(node.Name))
        {
            result = node.P(evidence[node.Name], parentValue) * sumProduct(idx + 1, evidence);
        }
        // when value of the node not known returns sum factor
        else
        {
            foreach (var val in node.Domain)
            {
                evidence.Add(node.Name, val);
                result += node.P(val, parentValue) * sumProduct(idx + 1, evidence);
                evidence.Remove(node.Name);
            }
        }

        return result;
    }

    // returns the traverse sequences
    public List<DiscreteBayesNode> TravelSequence { get => topological_seq; }

    // special case: the graph is empty
    // query name to node
    private Dictionary<string, DiscreteBayesNode> name2Node;
    // topologic sequence to visit the node from the ancestor to the leaf
    private List<DiscreteBayesNode> topological_seq;
}

// specification of a node
public record BayesNodeSpec
{
    public required string Name { get; init; }
    // public required Dictionary<List<string>, DiscreteProbDist> Conditional_probs { get; init; }
    public required List<List<string>> ParentValues;
    public required List<DiscreteProbDist> Probs;
    // Assumption, Parents are unique strings
    public required List<string> Parents { get; init; }
}
