using System.Collections;
using System.Diagnostics;

namespace CsharpALG.MachineLearning;


public class NNSearchByKDTree
{
    public NNSearchByKDTree(double[,] X)
    {
        // require X not empty and not null
        this.X = X;
        (numEntries, numDims) = (X.GetLength(0), X.GetLength(1));
        var sortIndices = GetSortIndices(X);
        root = buildTree(sortIndices);
        visited = new bool[numEntries];
    }

    /// <summary>
    /// given a query find its knearest neighbors
    /// </summary>
    /// <param name="query"></param>
    /// <param name="k"></param>
    /// <returns>a list of k (index, distance) tuples sorted by distance where index points to the data point X[index]</returns>
    /// <exception cref="InvalidDataException"></exception>
    public List<(int, double)> kNearestNeighbor(double[] query, int k)
    {
        // int n = X.GetLength(0), d = X.GetLength(1);
        if (query.Length != numDims)
            throw new InvalidDataException($"query, dataset dimension are not equal: " +
                $"query vector has dim {query.Length} while the dataset has dim {numDims}");

        // if (k >= n) return Enumerable.Range(0, n).ToList();
        Array.Fill(visited, false);

        var searchQ = new PriorityQueue<KDTreeNode, double>();
        var neighborQ = new PriorityQueue<int, double>(
            Comparer<double>.Create((a, b) => -a.CompareTo(b))
        );

        searchQ.Enqueue(root, double.PositiveInfinity);
        while (searchQ.Count > 0)
        {
            KDTreeNode node; double score;
            searchQ.TryDequeue(out node!, out score);

            // check if the node shall be expand under current distance
            if (!shouldExpand(score, neighborQ, k)) continue;

            // use median of the node to update current least distances
            updateNeighborQ(node, k, query, neighborQ);

            // insert child with score into the search queue
            var leftNode = node.Left;
            if (leftNode is not null)
            {
                double leftScore = query[node.Axis] <= node.Plane ? 0 : query[node.Axis] - node.Plane;
                searchQ.Enqueue(leftNode, leftScore);
            }

            var rightNode = node.Right;
            if (rightNode is not null)
            {
                double rightScore = query[node.Axis] > node.Plane ? 0 : node.Plane - query[node.Axis];
                searchQ.Enqueue(rightNode, rightScore);
            }
        }

        // insert k neighbors stored in neighborQ into a list
        List<(int, double)> result = new List<(int, double)>();
        while (neighborQ.Count > 0)
        {
            int idx; double dist;
            neighborQ.TryDequeue(out idx, out dist);
            result.Add((idx, dist));
        }
        result.Reverse();
        return result;
    }

    public double SearchRatio
    {
        get => ((double) visited.Select(x => x ? 1 : 0).Sum()) / numEntries;
    }

    public static int[,] GetSortIndices(double[,] X)
    {
        int n = X.GetLength(0), d = X.GetLength(1);
        int[,] sortIndices = new int[d, n];

        double[] col = new double[n];
        for (int j = 0; j < d; ++j)
        {
            int[] sortIndex = Enumerable.Range(0, n).ToArray();
            for (int i = 0; i < n; ++i)
                col[i] = X[i, j];

            Array.Sort(col, sortIndex);

            for (int i = 0; i < n; ++i)
                sortIndices[j, i] = sortIndex[i];
        }

        return sortIndices;
    }

    private bool shouldExpand(double score, PriorityQueue<int, double> neighbor, int k)
    {
        if (neighbor.Count < k) return true;

        double dist;
        neighbor.TryPeek(out _, out dist);

        return score < dist;
    }

    private void updateNeighborQ(KDTreeNode node, int k, double[] query, PriorityQueue<int, double> neighborQ)
    {
        int dataIdx = node.SortIndices[node.Axis, node.MedianIdx];
        if (visited[dataIdx]) return;

        double medianDist = node.GetDistance(query);
        if (neighborQ.Count < k)
        {
            neighborQ.Enqueue(dataIdx, medianDist);
        }
        else
        {
            double topDist;
            neighborQ.TryPeek(out _, out topDist);

            if (medianDist < topDist)
            {
                neighborQ.Dequeue();
                neighborQ.Enqueue(dataIdx, medianDist);
            }
        }

        visited[dataIdx] = true;
    }

    private KDTreeNode buildTree(int[,] sortIndices)
    {
        KDTreeNode root = new KDTreeNode(sortIndices, X);

        if (root.IsLeaf)
            return root;

        (var leftIndices, var rightIndices) = root.Partition();

        root.Left = buildTree(leftIndices);
        root.Right = buildTree(rightIndices);

        Debug.Assert(root.Left is not null && root.Right is not null);

        return root;
    }

    private KDTreeNode root;
    private double[,] X; // feature matrix
    private int numEntries;
    private int numDims;
    private bool[] visited; // mask for visited index, without this array, duplicates may emerge
}

class KDTreeNode
{
    public KDTreeNode(int[,] sortIndices, double[,] dataSet, double eps = 1e-10)
    {
        SortIndices = sortIndices;
        X = dataSet;

        double variance;
        (Axis, variance) = getPartitionDim();
        IsLeaf = variance <= eps;

        int size = sortIndices.GetLength(1);
        MedianIdx = (size-1) / 2;
        Plane = X[sortIndices[Axis, MedianIdx], Axis];
    }

    // get query score for i
    public (int[,], int[,]) Partition()
    {
        int dim = SortIndices.GetLength(0), size = SortIndices.GetLength(1);

        int numLeft = 0;
        for (int i = 0; i < size; ++i)
        {
            int idx = SortIndices[Axis, i];
            if (X[idx, Axis] > Plane) break;
            numLeft += 1;
        }

        // if (!(numLeft < size && MedianIdx < numLeft))
        // {
        //     Console.WriteLine($"plane = {Plane}");
        //     for (int i = 0; i < size; ++i)
        //     {
        //         int idx = SortIndices[Axis, i];
        //         Console.Write($"{X[idx, Axis]} ");
        //     }
        //     Console.WriteLine();
        // }

        Debug.Assert(numLeft < size && MedianIdx < numLeft, $"{numLeft}, {size}, {MedianIdx}, {numLeft}");

        int[,] sortIndicesLeft = new int[dim, numLeft];
        int[,] sortIndicesRight = new int[dim, size - numLeft];
        for (int d = 0; d < dim; ++d)
        {
            int j1 = 0, j2 = 0;
            for (int i = 0; i < size; ++i)
            {
                int idx = SortIndices[d, i];
                if (X[idx, Axis] <= Plane)
                {
                    sortIndicesLeft[d, j1] = idx;
                    j1 += 1;
                }
                else
                {
                    sortIndicesRight[d, j2] = idx;
                    j2 += 1;
                }
            }
        }

        return (sortIndicesLeft, sortIndicesRight);
    }

    // get distance from the query point to the median point
    public double GetDistance(double[] query)
    {
        Debug.Assert(query.Length == X.GetLength(1));
        double dist = 0.0;
        for (int i = 0; i < query.Length; ++i)
        {
            double residual = query[i] - X[SortIndices[Axis, MedianIdx], i];
            dist += residual * residual;
        }

        return Math.Sqrt(dist);
    }

    (int, double) getPartitionDim()
    {
        int dim = SortIndices.GetLength(0), size = SortIndices.GetLength(1);
        if (size == 1) return (0, 0.0);

        double mu, variance, varianceMax = 0.0;
        int partitionDim = 0;
        // compute variance on each dimension
        for (int d = 0; d < dim; ++d)
        {
            mu = computeMean(size, d);
            variance = computeVar(size, mu, d);

            if (variance > varianceMax)
            {
                partitionDim = d;
                varianceMax = variance;
            }
        }

        return (partitionDim, varianceMax);
    }

    private double computeVar(int size, double mu, int d)
    {
        double variance = 0.0;
        for (int i = 0; i < size; ++i)
        {
            double residual = X[SortIndices[d, i], d] - mu;
            variance += residual * residual;
        }

        variance /= size;
        return variance;
    }

    private double computeMean(int size, int d)
    {
        double mu = 0.0;
        for (int i = 0; i < size; ++i)
        {
            mu += X[SortIndices[d, i], d];
        }
        mu /= size;
        return mu;
    }

    public int MedianIdx; // reference to index of the dataset
    public int Axis; // partition axis
    public int[,] SortIndices; // Data[SortIndices[d,:], d] is ordered in asc
    public double[,] X; // reference to dataset
    public double Plane; // current node is partitioned by x_d == plane
    public bool IsLeaf; // whether current node is a leaf
    public KDTreeNode? Left;
    public KDTreeNode? Right;
}

class ExampleKDTreeNode
{
    static public void Run()
    {
        double[,] X = new double[,]
        {
            {2.6977974150815007,5.52757432793072,2.528902664401259,-1.9180709328020376},
            {2.383968160132672,7.066695154344373,4.591425459992741,1.661286291409347},
            {-0.7117159731138343,5.137194143241598,1.3746617335744107,-2.096714723490262},
            {-1.8117674368109666,0.4771151102884761,6.190768355947774,-1.6933128400447952},
            {-0.7583702205976479,3.648306902549088,1.8626331337547324,2.268241770250516},
            {2.974865243949881,1.6312942600375482,2.5434400897276594,2.9697780875860946},
            {-3.468750885160241,-4.530514432172298,4.758077060939723,1.7941142636455523},
            {4.253089975279541,8.105255406959312,5.3962335719295265,0.3706737280703818},
            {0.26294812045003746,1.9506902302245555,2.574167680247511,1.1163987898548102},
            {5.907422486683899,7.4837780856082805,4.053424288260366,4.300945345974823},
            {3.811294486665174,-0.2479282360138293,2.5578238147710355,-0.26264118967309225},
            {2.1638657546127136,-0.017615789915403024,2.430222727282574,0.11913363079356931},
            {9.844934196088545,5.384562849021814,-1.5449387143736635,3.811379852447144},
            {1.413055168168761,-0.5205591280080788,1.4864949431656749,-2.365215146283247},
            {-0.3915080743962194,-0.4123098536323462,0.5154350354444934,2.0082322456412824},
            {1.921191788074386,5.088867714604307,1.9010485638070582,3.0515111642287356}
        };
        int[,] sortIdx = NNSearchByKDTree.GetSortIndices(X);

        run(X, sortIdx);
    }


    static void run(double[,] X, int[,] sortIndices)
    {
        var node = new KDTreeNode(sortIndices, X);
        if (!node.IsLeaf)
        {
            Console.WriteLine($"node choose dim {node.Axis} to partition");
            (var left, var right) = node.Partition();
            Console.WriteLine($"left subnode size: {left.GetLength(1)}, right subnode size {right.GetLength(1)}");

            for (int d = 0; d < left.GetLength(0); ++d)
            {
                for (int i = 0; i < left.GetLength(1); ++i)
                {
                    Debug.Assert(X[left[d, i], node.Axis] <= node.Plane);
                }

                for (int i = 0; i < left.GetLength(1) - 1; ++i)
                {
                    Debug.Assert(X[left[d, i], d] <= X[left[d, i], d]);
                }
            }

            for (int d = 0; d < right.GetLength(0); ++d)
            {
                for (int i = 0; i < right.GetLength(1); ++i)
                {
                    Debug.Assert(X[right[d, i], node.Axis] > node.Plane);
                }

                for (int i = 0; i < right.GetLength(1) - 1; ++i)
                {
                    Debug.Assert(X[right[d, i], d] <= X[right[d, i], d]);
                }
            }
        }
    }
}

static class ExampleKNNSearch
{
    public static void Run()
    {
        // case1: test query self
        double[,] X = new double[,]{
{-1.3319049328667045,-6.545349256800515,5.957348625841179},
{-5.779981497797785,7.110340943630679,0.39929321789678407},
{-0.9744446334409318,6.679842230672162,2.566520528465367},
{3.3366883635853615,1.9371908708530443,-2.2371927643438987},
{3.7676393559221726,0.5309353849563967,-1.3188672461377284},
{0.18187869436367032,0.1288526558327714,2.1235257669230765},
{2.418697399818424,-2.0820039840570157,-3.293873591161274},
{1.0533224817661189,0.9608075774512854,4.749921417256608},
{3.5846949384774986,4.2841590539958885,4.929165752790357},
{4.92668129553239,3.129559378027831,1.6494188685062237},
{-3.7900446088327326,-4.676262572701909,-0.4517596152214254},
{1.9163789164579843,6.0025690764886175,-3.227018654966469},
{-4.11269018870151,3.3447930807723854,-0.5919698284464809},
{-5.054784923336168,-3.086373023355507,-0.6765492633841754},
{6.247573164733646,-3.835908258171589,3.1228599673455157},
{-0.8621136259850859,3.8970170952199714,2.014588663549468},
{5.503597973111614,5.114951643248581,0.6165954021194304},
{-3.2188627089951067,3.445035917928847,4.8931312618732115},
{-0.9355779449208659,1.801295506318962,-1.1070224738086774},
{1.3127949292207761,8.088523277532925,0.7732001148978247},
{0.5501865800370158,-2.5403454365785123,6.837679471653862},
{4.535834225129351,3.2959357588201437,3.6242503497405663},
{1.320188542552899,3.8836245506519083,7.353664804627492},
{-0.5878812484787317,-0.0005392738088580984,2.6717459919242597},
{3.13454484840441,1.7193588915669724,-0.3523785365945975},
{2.3135590172190614,-1.7650517229314415,-1.7937247380455252},
{0.38178344874507397,-0.382296667270019,5.305230721542344},
{4.992138545623377,0.6990760113196088,5.1450408135083645},
{4.614335304066767,6.443300547004073,2.591181338132933},
{-0.4538379902488838,2.2239346776905986,5.510636338488051},
{4.647630914987713,-1.4534617908635843,6.497852212010183},
{3.6075764651280218,4.122434347855412,-0.24217234600753113},
{4.581268662040144,4.9869936724059425,0.8971278915233694},
{-0.17635173983942298,4.3393072435444715,3.699912527558066},
{5.049234663978433,3.3539300710955686,1.5231373003877122},
{-0.8181072248000998,3.706054654156275,-6.018740669583892},
{4.357637244534546,1.7251931398273437,5.123293102595232},
{1.5602174675514666,-0.13087499506253408,1.96734108213204},
{1.5472590029591582,1.1564934270109177,3.026268803658359},
{-1.0503111625501482,-1.8865136992083995,-1.1986322664486058},
        };

    double[][] query = [[3.92329504, 6.78937581, 2.39721282],[4.7167429 , -4.38917328,  3.94383469]];
    // for (int j=0; j < X.GetLength(1); ++j) query[j] = X[20, j];
    run(X, query, 3); // find nn to self
}

    private static void run(double[,] X, double[][] queries, int k)
    {
        var searcher = new NNSearchByKDTree(X);
        foreach(var query in queries)
        {
            var result = searcher.kNearestNeighbor(query, k);

            foreach((var idx, var dist) in result)
                Console.WriteLine($"nearest neighbor = {idx}, distance = {dist}");

            Console.WriteLine($"search {searcher.SearchRatio * 100}% entries");
        }
    }
}