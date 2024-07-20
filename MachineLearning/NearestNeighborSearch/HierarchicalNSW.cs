using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using CsharpALG.Numerical;
namespace CSharpALG.MachineLearning;

public class HierarchicalNSW
{
    public HierarchicalNSW(double[,] X, int maxM = 16, int efConstruction = 200, int seed = 123)
    {
        // data set config
        this.X = X;
        (size, dim) = (X.GetLength(0), X.GetLength(1));

        // neighborhood size config
        this.maxM = maxM;
        maxM0 = 2 * maxM;
        mult = 1.0 / Math.Log(1.0 * maxM);
        this.efConstruction = Math.Max(efConstruction, maxM);

        // init datastructure for navigate graphs
        nodeIds = new List<int[]>();
        graphs = new List<List<List<int>>>();
        unusedIds = new List<int>();
        numLayers = 0;
        enterPoint = -1;

        // init sampler
        uniform = new Random(seed);

        // build graph
        for (int i=0; i < size; ++i) addPoint(i);
    }

    // layer ~ -ln(uniform[0, 1]) * mult
    public int SampleLayer()
    {
        return (int)(-Math.Log(uniform.NextDouble()) * mult);
    }

    /// <summary>
    /// given a query find its knearest neighbors
    /// </summary>
    /// <param name="query"></param>
    /// <param name="k"></param>
    /// <param name="ef">size of search queue</param>
    /// <returns>a list of k (index, distance) tuples sorted by distance where index points to the data point X[index]</returns>
    /// <exception cref="InvalidDataException"></exception>
    public List<(int, double)> kNearestNeighbor(ReadOnlySpan<double> query, int k, int ef)
    {
        if (query.Length != dim)
            throw new InvalidDataException($"input data has length: {query.Length}, while the dimension of dataset is {dim}");

        // use enter point to greedly navigate to level 0
        int ep = enterPoint;
        ReadOnlySpan2D<double> Xptr = X;
        for (int l = numLayers - 1; l >= 0; --l)
        {
            double dist = distance(Xptr.GetRowSpan(ep), query);
            bool changed = true;
            while (changed)
            {
                changed = false;
                var neighbor = getNeighbor(l, ep);
                foreach(int u in neighbor)
                {
                    double distU = distance(Xptr.GetRowSpan(u), query);
                    if (distU < dist)
                    {
                        dist = distU;
                        ep = u;
                        changed = true;
                    }
                }
            }
        }

        // get neighbor from level 0
        var neighborQ = searchLayer(query, ep, 0, Math.Max(k, ef));

        // insert result into a list
        List<(int, double)> result = new List<(int, double)>();

        while (neighborQ.Count > 0 && result.Count < k)
        {
            (var di, var idx) = neighborQ.Dequeue();
            result.Add((idx, di));
        }

        return result;
    }

    private void addPoint(int index)
    {
        // special case when index == 0
        int level = SampleLayer();

        // from navigate the graph to the nearest point in level+1 as the enter point
        int ep = enterPoint;
        for (int l = numLayers - 1; l >= level + 1; --l)
        {
            // expand neighbors of enter point, until local optimal found
            bool changed = true;
            double dist = getDistance(ep, index);

            while (changed)
            {
                changed = false;
                foreach (int x in getNeighbor(l, ep))
                {
                    double distX = getDistance(x, index);
                    if (distX < dist)
                    {
                        dist = distX;
                        ep = x;
                        changed = true;
                    }
                }
            }
        }

        // insert the node to layer level down to 0
        for (int l = Math.Min(numLayers - 1, level); l >= 0; --l)
        {
            // get efConstruction nearest neighbors of X[index] from layer l
            var neighborQ = searchLayer(index, ep, l, efConstruction);

            // update navigate point
            // use current nearest point as enter point of next layer
            // Debug.Assert(neighborQ.Count >= 1);
            // ep = neighborQ.Peek().Item2;

            // add these node to X[index]'s neighbor
            // foreach node in neighbor add index to its neighbor, when the node's
            // neighbor contains more node than maxM, use heuristic to drop points
            // mutuallyAddEdges(index, neighborQ, l)
            int numNeighbor = l > 0 ? maxM : maxM0;
            ep = mutuallyAddEdges(index, neighborQ, l, numNeighbor);
        }

        // when level > numLayers, insert a layer with the only point X[index]
        if (level >= numLayers)
        {
            appendNewLayer(index);
        }
    }

    private int mutuallyAddEdges(int index, PriorityQueue<(double, int), double> neighborQ, int level, int neighborSize)
    {
        // insert point index to current layer
        addPointToGraph(level, index);
        int nodeid = nodeIds[level][index];

        // get M neighbor from neighborQ
        (var _, var neighbor) = adjustNeighbor(neighborQ, maxM);

        // set neighbors of nodeid using the queue
        // graphs[level][nodeid] = new List<int>(neighborQ.Count);
        // var neighbor = new List<int>(neighborQ.Count);
        // foreach ((var element, var _) in neighborQ.UnorderedItems)
        //     neighbor.Add(element.Item2);
        graphs[level][nodeid] = neighbor;
        int next_ep = neighbor[0];

        // add index to each node in neighbor
        foreach (int u in neighbor)
        {
            var neighborU = getNeighbor(level, u);
            if (neighborU.Count < neighborSize)
            {
                neighborU.Add(index);
            }
            else
            {
                // choose best M neighbors from neighborU \cap {index}
                var neighborCand = new PriorityQueue<(double, int), double>();
                double dist = getDistance(u, index);
                neighborCand.Enqueue((dist, index), dist); // check all queue
                foreach(int v in neighborU)
                {
                    dist = getDistance(u, v);
                    neighborCand.Enqueue((dist, v), dist);
                }

                // var neighborAdj = adjustNeighbor(neighborCand, neighborSize)
                // set neighbor for u
                (var dists, neighborU) = adjustNeighbor(neighborCand, neighborSize);
                setNeighbor(level, u, neighborU);
            }
        }

        return next_ep;
    }

    // adjust neighbor and return there distances to the center and indices
    // heuristic: make u the dominant nearest point for it neighbors
    private (List<double>, List<int>) adjustNeighbor(PriorityQueue<(double, int), double> neighbor, int neighborSize)
    {
        var resultDists = new List<double>(neighborSize);
        var resultIndices = new List<int>(neighborSize);

        while (neighbor.Count > 0 && resultIndices.Count < neighborSize)
        {
            (var dist, var idx) = neighbor.Dequeue();

            bool isDominant = true;
            for (int i=0; i < resultDists.Count && isDominant; ++i)
            {
                if (dist > getDistance(resultIndices[i], idx)) isDominant = false;
            }

            if (isDominant)
            {
                resultDists.Add(dist);
                resultIndices.Add(idx);
            }
        }

        return (resultDists, resultIndices);
    }

    // given enter point ep, return at most numNeighbor nearest neighbors of level-th navigate graph
    // the results are stored in a small top heap. The heap's key is the distance to query
    private PriorityQueue<(double, int), double> searchLayer(ReadOnlySpan<double> query, int ep, int level, int numNeighbor)
    {
        bool[] isVisited = new bool[unusedIds[level]];

        // init condidate queue and current best queue
        var candidate = new PriorityQueue<(double, int), double>();
        var currentBest = new PriorityQueue<(double, int), double>(
            Comparer<double>.Create((a, b) => -(a.CompareTo(b)))
        );
        int window = numNeighbor; // window of current best

        // insert enter point into heaps
        double dist = getDistance(ep, query);
        candidate.Enqueue((dist, ep), dist);
        currentBest.Enqueue((dist, ep), dist);
        isVisited[nodeIds[level][ep]] = true;

        // expand search node according to their distance to X[index]
        while (candidate.Count > 0)
        {
            // explore best node in candidate set
            (double distCurr, int curr) = candidate.Dequeue();

            // when there exist numNeighbor point and
            // the worst distance is nicer than current best break the loop
            if (currentBest.Count >= window && currentBest.Peek().Item1 < distCurr)
                break;

            // explore unvisited neighbor of node curr
            foreach (int v in getNeighbor(level, curr))
            {
                if (isVisited[nodeIds[level][v]]) continue;
                dist = getDistance(v, query);
                if (currentBest.Count < window || currentBest.Peek().Item1 > dist)
                {
                    candidate.Enqueue((dist, v), dist);
                    if (currentBest.Count >= window)
                        currentBest.Dequeue();
                    currentBest.Enqueue((dist, v), dist);
                }
                isVisited[nodeIds[level][v]] = true;
            }

            Debug.Assert(currentBest.Count <= window);
        }

        // insert currentBest to a small head heap
        var result = new PriorityQueue<(double, int), double>(currentBest.UnorderedItems);
        return result;
    }

    private PriorityQueue<(double, int), double> searchLayer(int index, int ep, int level, int numNeighbor)
    {
        ReadOnlySpan2D<double> Xptr = X;
        var query = Xptr.GetRowSpan(index);
        return searchLayer(query, ep, level, numNeighbor);
    }

    private void appendNewLayer(int index)
    {
        // allocate space for a new layer
        nodeIds.Add(new int[size]);
        Array.Fill(nodeIds.Last(), -1);
        unusedIds.Add(0);
        graphs.Add(new List<List<int>>());
        numLayers += 1;

        // insert point index to last layer
        addPointToGraph(numLayers - 1, index);

        enterPoint = index;
    }

    private void addPointToGraph(int level, int index)
    {
        // allocate an id for the point
        int id = unusedIds[level];
        unusedIds[level] += 1;
        Debug.Assert(nodeIds[level][index] == -1, "index already in graph");
        nodeIds[level][index] = id;

        // now append id to the graph
        var graph = graphs[level];
        graph.Add(new List<int>());

        Debug.Assert(graph.Count == unusedIds[level]);
    }

    private List<int> getNeighbor(int level, int index)
    {
        int nodeId = nodeIds[level][index];
        Debug.Assert(nodeId >= 0);

        return graphs[level][nodeId];
    }

    private void setNeighbor(int level, int index, List<int> neighbor)
    {
        int maxSize = level > 0 ? maxM : maxM0;
        Debug.Assert(neighbor.Count <= maxSize);

        int nodeId = nodeIds[level][index];
        Debug.Assert(nodeId >= 0);

        graphs[level][nodeId] = neighbor;
    }

    public static double distance(ReadOnlySpan<double> va, ReadOnlySpan<double> vb)
    {
        double dist = 0.0;
        for (int i = 0; i < va.Length; ++i)
        {
            var residual = va[i] - vb[i];
            dist += residual * residual;
        }
        return Math.Sqrt(dist);
    }

    private double getDistance(int x, int y)
    {
        ReadOnlySpan2D<double> Xptr = X;
        return distance(Xptr.GetRowSpan(x), Xptr.GetRowSpan(y));
    }

    private double getDistance(int x, ReadOnlySpan<double> y)
    {
        ReadOnlySpan2D<double> Xptr = X;
        return distance(Xptr.GetRowSpan(x), y);
    }

    private List<int[]> nodeIds; // nodeIds[l][j] is the node index of data point X[j] when it is in j-th layer
    private List<int> unusedIds; // unused id of layer l, or number of points in layer l
    private List<List<List<int>>> graphs; // navigate graphs, graphs[l] is the navigate graph at layer l
    private int enterPoint; // index of private point
    private int numLayers; // layers of the graphs
    private int maxM; // size of neighbor hood
    private int maxM0; // max size of finnest graph
    private int efConstruction; // queue size used to search from a layer
    private double mult; // multiplier for layer sampling
    public double[,] X; // reference to feature matrix
    private int size; // size of dataset
    private int dim; // dimension of dataset
    private Random uniform; // uniform sampler
}

public static class HierarchicalNSWTest
{
    // check layer sampler
    public static void SampleLayerTest()
    {
        double[,] X = { { 1.0, 2.0 } };
        var hns = new HierarchicalNSW(X);
        int n = 5000000;
        int[] layers = new int[n];

        for (int i = 0; i < n; ++i)
            layers[i] = hns.SampleLayer();

        var groups = layers.GroupBy(x => x).Select(x => new
        {
            Layer = x.Key,
            Count = x.Count()
        });

        foreach (var group in groups)
        {
            Console.WriteLine($"layer = {group.Layer}, count = {group.Count}");
        }
    }
}

public static class HierarchicalNExample
{
    public static void Run()
    {
        int n = 10000, d = 128;
        double[,] X = (double[,]) LinearAlg.RandomUniform(0.0, 1.0, [n, d]);
        // Console.WriteLine("Run example of HNSW algorithm");
        // runSelfQuery(X);

        Console.WriteLine("compare HNSW with brute force algorithm");
        double[][] quries = new double[10][];
        for (int i=0; i < 10; ++i) {
            quries[i] = new double[d];
            for (int j=0; j < d; ++j) {
                quries[i][j] = X[i, j];
            }
        }

        compareWithBruteForce(X, quries, 5, 50);
    }

    public static void runSelfQuery(double[,] X)
    {
        Console.WriteLine("Run self query of HNSW algorithm");
        var nn = new HierarchicalNSW(X);
        Console.WriteLine("build tree complete");
        ReadOnlySpan2D<double> Xptr = X;

        int numCorrect = 0;
        for(int i=0; i < X.GetLength(0); ++i) {
            var neighbor = nn.kNearestNeighbor(Xptr.GetRowSpan(i), 1, 50);
            numCorrect += (neighbor[0].Item1 == i) ? 1 : 0;
        }

        double recall = numCorrect;
        recall /= X.GetLength(0);
        Console.WriteLine($"recall: {recall * 100} % data");
    }

    public static void compareWithBruteForce(double[,] X, double[][] queries, int k, int ef)
    {
        // Console.WriteLine("compare result of HNSW algorithm with brute force");
        var nn = new HierarchicalNSW(X);
        Console.WriteLine("build tree complete");

        var nnBrute = new NNBruteForceSearch(X, HierarchicalNSW.distance);

        foreach(var query in queries)
        {
            var result1 = nn.kNearestNeighbor(query, k, ef);
            var result2 = nnBrute.kNearestNeighbor(query, k);

            Debug.Assert(result1.Count == result2.Count);

            Console.WriteLine($"hnsw.index, hnsw.dist\tbrute.index, brute.dist");
            for(int i=0; i < result1.Count; ++i)
            {
                Console.WriteLine($"{result1[i].Item1}, {result1[i].Item2}\t{result2[i].Item1}, {result2[i].Item2}");
            }
        }
    }
}