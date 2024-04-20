namespace CsharpALG.Graph;

/// <summary>
/// BellmanFord is a single source shortest path algorithm. It allows negative
/// edges for the input graph
/// </summary>
public class BellmanFord
{
    public BellmanFord(List<List<int[]>> graph, int src)
    {
        this.graph = graph;
        n = graph.Count;
        this.src = src;

        pi = new int[n];
        distance = new int[n];

        Array.Fill(pi, -1);
        Array.Fill(distance, int.MaxValue);
    }

    /// <summary>
    /// compute distance from the source to each node. when there exist an
    /// negative loop in the graph, the algorithm failed and return false
    /// </summary>
    /// <returns></returns>
    public bool computeDistances()
    {
        // number of interations, used to check negative loop
        int[] iters = new int[n];
        bool[] inQueue = new bool[n];
        var que = new Queue<int>();

        // insert source into queue
        distance[src] = 0;
        iters[src] = 0;
        pi[src] = -1;
        inQueue[src] = true;
        que.Enqueue(src);

        // relax distance in almost n-1 iterations
        while (que.Count > 0)
        {
            int u = que.Dequeue();
            inQueue[u] = false;

            if (iters[u] >= n)
            {
                hasNegativeLoop = true;
                return false;
            }

            // relax distance for u's neighbor
            foreach (var node in graph[u])
            {
                int v = node[0], w = node[1];
                int relax = distance[u] + w;
                if (relax < distance[v])
                {
                    distance[v] = relax;
                    pi[v] = u;
                    iters[v] = iters[u] + 1;
                    if (!inQueue[v])
                    {
                        inQueue[v] = true;
                        que.Enqueue(v);
                    }
                }
            }
        }

        hasNegativeLoop = false;
        return true;
    }

    public void PrintDistance()
    {
        if (hasNegativeLoop)
        {
            Console.WriteLine("Negative Loop found int the graph.");
        }
        else
        {
            Console.Write("distances:");
            foreach (int d in distance)
                Console.Write($"{d} ");
            Console.WriteLine();
        }
    }

    public void PrintRoute(int u)
    {
        if (hasNegativeLoop)
            throw new InvalidOperationException("print route for graph with negative loop is not supported");

        if (distance[u] == int.MaxValue)
        {
            Console.WriteLine($"node {src} and {u} is disconnected");
            return;
        }

        Console.Write($"shortest path ({src}->{u}):");
        var stack = new Stack<int>();
        while (u != src)
        {
            stack.Push(u);
            u = pi[u];
        }

        Console.Write($"{src} ");
        while (stack.Count > 0)
        {
            Console.Write($"{stack.Pop()} ");
        }

        Console.WriteLine();
    }

    // graph[i] contains all end point of edges
    // emitted from i. for a tuple in graph[i]
    // tuple[0] is the end point, tuple[1] is the weight
    private List<List<int[]>> graph;
    private int n;
    private int src;
    private bool hasNegativeLoop;
    private int[] pi; // next node in the shortest route from u to the source
    private int[] distance; // distance from source to all node
}