using System.Diagnostics;

namespace CsharpALG.MachineLearning;


// Test build a graph without loop
// Test build a graph with loop (should raise exception)
class ExampleBuildBayesianNet
{
    static public void Run()
    {
        List<BayesNodeSpec> spec = DiscreteBayesNetSpecTestData.specs[0];

        var graph = new DiscreteBayesNet();
        graph.BuildFrom(spec);
        Console.WriteLine("visit sequences:");
        foreach (var node in graph.TravelSequence)
        {
            Console.Write($"{node.Name} ");
        }
        Console.WriteLine();

        // marginal shall be 1
        double prob = graph.Marginalize(new Dictionary<string, string>());
        Debug.Assert(Math.Abs(prob - 1.0) < 1e-9);
        Console.WriteLine($"marginal prob: {prob}");
    }
}

static class ExampleExactInference
{
    static public void Run()
    {
        List<BayesNodeSpec> spec = DiscreteBayesNetSpecTestData.specs[1];

        var graph = new DiscreteBayesNet();
        graph.BuildFrom(spec);

        // 'False: 0.716, True: 0.284'
        var prob = graph.InferPosterior("Burglary", new(){{"JohnCalls", "T"}, {"MaryCalls", "T"}});
        Debug.Assert(Math.Abs(prob["F"] - 0.716) <= 1e-3 && Math.Abs(prob["T"] - 0.284) < 1e-3);
        Console.WriteLine($"{prob["F"]}, {prob["T"]}");

        // 'False: 0.995, True: 0.00513'
        prob = graph.InferPosterior("Burglary", new(){{"JohnCalls", "T"}, {"MaryCalls", "F"}});
        Debug.Assert(Math.Abs(prob["F"] - 0.995) <= 1e-3 && Math.Abs(prob["T"] - 0.00513) < 1e-4);

        // False: 0.993, True: 0.00688'
        prob = graph.InferPosterior("Burglary", new(){{"JohnCalls", "F"}, {"MaryCalls", "T"}});
        Debug.Assert(Math.Abs(prob["F"] - 0.993) <= 1e-3 && Math.Abs(prob["T"] - 0.00688) < 1e-4);

        prob = graph.InferPosterior("Burglary", new(){{"JohnCalls", "T"}});
        Debug.Assert(Math.Abs(prob["F"] - 0.984) <= 1e-3 && Math.Abs(prob["T"] - 0.0163) < 1e-4);

        prob = graph.InferPosterior("Burglary", new(){{"MaryCalls", "T"}});
        Debug.Assert(Math.Abs(prob["F"] - 0.944) <= 1e-3 && Math.Abs(prob["T"] - 0.0561) < 1e-4);
    }
}

static class ExampleSamplerInference {
    public static void Run()
    {
        List<BayesNodeSpec> spec = DiscreteBayesNetSpecTestData.specs[0];
        var graph = new DiscreteBayesNet();
        graph.BuildFrom(spec);

        var evidence = new Dictionary<string, string>(){
            {"H", "T"}, {"J", "F"}, {"A", "T"}, {"D", "F"}
        };

        int n = 10000;

        runMargin(graph, evidence, n, new GibsBNSampler(graph));
        runMargin(graph, evidence, n, new ParticleFilteringBNSampler(graph));

        // gibbs_ask('Cloudy', dict(Rain=True), sprinkler, 1000)
        spec = DiscreteBayesNetSpecTestData.specs[1];
        graph.BuildFrom(spec);
        evidence = new Dictionary<string, string>(){
            {"JohnCalls", "T"}, {"MaryCalls", "T"}
        };
        runMargin(graph, evidence, n, new GibsBNSampler(graph));
        runMargin(graph, evidence, n, new ParticleFilteringBNSampler(graph));
    }

    private static void runMargin(DiscreteBayesNet graph, Dictionary<string, string> evidence, int n, DiscreteBayesNetSampler sampler) {
        (var nodeNames, var sampleSet) = sampler.Sample(evidence, n);

        // compute margin
        for (int i=0; i < nodeNames.Count; ++i) {
            Console.WriteLine($"Infer posterior for {nodeNames[i]}");
            var prob = computeMargin(graph.GetNode(nodeNames[i]), i, sampleSet);
            Console.WriteLine($"Estimate:   {prob}");
            var prob2 = graph.InferPosterior(nodeNames[i], evidence);
            Console.WriteLine($"GroundTruth:{prob2}");
            Console.WriteLine();
        }
    }

    static DiscreteProbDist computeMargin(DiscreteBayesNode node, int idx, Dictionary<List<string>, double> points) {
        DiscreteProbDist prob = new(node.Name);
        foreach(var x in node.Domain) {
            prob[x] = 0.0;
        }

        foreach(var kv in points) {
            var point = kv.Key;
            var weight = kv.Value;
            prob[point[idx]] += weight;
        }

        prob.Normalize();
        return prob;
    }
}

static class DiscreteBayesNetSpecTestData
{
    public static List<List<BayesNodeSpec> > specs = new(){
        new List<BayesNodeSpec>(){
            new (){
                Name = "A", ParentValues = [[]], Probs = [new("", ["F", "T"], [1.0,6.0])], Parents = []
            },
            new (){
                Name = "B", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.4, 0.2]),
                    new("", ["F", "T"], [0.5, 0.2]),
                ], Parents = ["A"]
            },
            new (){
                Name = "C", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.13, 0.35]),
                    new("", ["F", "T"], [0.45, 0.13]),
                ], Parents = ["A"]
            },
            new (){
                Name = "D", ParentValues = [[]], Probs = [new("", ["F", "T"], [1.0, 2.0])], Parents = []
            },
            new (){
                Name = "E", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.6, 0.3]),
                    new("", ["F", "T"], [0.2, 0.6]),
                ], Parents = ["D"]
            },
            new (){
                Name = "F", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.1, 0.9]),
                    new("", ["F", "T"], [0.8, 0.2]),
                ], Parents = ["E"]
            },
            new (){
                Name = "G", ParentValues = [[]], Probs = [new("", ["F", "T"], [1.0, 2.0])], Parents = []
            },
            new (){
                Name = "H", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.4, 0.7]),
                    new("", ["F", "T"], [0.5, 0.2]),
                ], Parents = ["G"]
            },
            new (){
                Name = "I", ParentValues = [["T", "T"], ["T", "F"], ["F", "T"], ["F", "F"]], Probs = [
                    new("", ["F", "T"], [0.2, 0.8]),
                    new("", ["F", "T"], [0.1, 0.2]),
                    new("", ["F", "T"], [0.7, 0.1]),
                    new("", ["F", "T"], [0.9, 0.4])
                ], Parents = ["H", "G"]
            },
            new (){
                Name = "J", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.3, 0.6]),
                    new("", ["F", "T"], [0.7, 0.4]),
                ], Parents = ["I"]
            },
            new (){
                Name = "K", ParentValues = [["T", "T"], ["T", "F"], ["F", "T"], ["F", "F"]], Probs = [
                    new("", ["F", "T"], [0.5, 0.1]),
                    new("", ["F", "T"], [0.4, 0.7]),
                    new("", ["F", "T"], [0.2, 0.2]),
                    new("", ["F", "T"], [0.3, 0.9])
                ], Parents = ["I", "J"]
            },
        },

        new List<BayesNodeSpec>(){
            new (){
                Name = "Burglary", ParentValues = [[]], Probs = [new("", ["F", "T"], [1.0-0.001, 0.001])], Parents = []
            },
            new (){
                Name = "Earthquake", ParentValues = [[]], Probs = [new("", ["F", "T"], [1.0-0.002, 0.002])], Parents = []
            },
            new (){
                Name = "Alarm",
                ParentValues = [["T","T"], ["T","F"], ["F", "T"], ["F", "F"]],
                Probs = [
                    new("", ["F", "T"], [0.05, 0.95]),
                    new("", ["F", "T"], [0.06, 0.94]),
                    new("", ["F", "T"], [1.0-0.29, 0.29]),
                    new("", ["F", "T"], [1.0-0.001, 0.001])
                ], Parents = ["Burglary", "Earthquake"]
            },
            new (){
                Name = "JohnCalls", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.1, 0.9]),
                    new("", ["F", "T"], [0.95, 0.05]),
                ], Parents = ["Alarm"]
            },
            new (){
                Name = "MaryCalls", ParentValues = [["T"], ["F"]], Probs = [
                    new("", ["F", "T"], [0.30, 0.70]),
                    new("", ["F", "T"], [0.99, 0.01]),
                ], Parents = ["Alarm"]
            }
        }
    };
}