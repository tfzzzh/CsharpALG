using System.Diagnostics;

namespace CsharpALG.MaxFlow;

/// <summary>
/// Edmonds-Karp algorithm slove the maxflow problem in O(|V| |E|^2)
/// It iteratively augument the flow via bfs searching on the residual graph
/// When applying to maximum-bipartite-matching problem, its complexity can
/// further reduce to O(|V||E|)
/// </summary>
public class EdmondsKarp
{
    /// <summary>
    /// capacity shall be a nonegative square matrix with zero diagnal
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    public EdmondsKarp(int[,] capacity, int src, int dst)
    {
        this.capacity = (int[,]) capacity.Clone();
        this.capacity_original = capacity;
        this.src = src;
        this.dst = dst;
        n = capacity.GetLength(0);

        // extract adj matrix
        adj = new List<List<int>>(n);
        for (int i=0; i < n; ++i) adj.Add(new List<int>());
        for (int u = 0; u < n; ++u)
        {
            for (int v=u+1; v < n; ++v)
            {
                if (capacity[u,v] > 0 || capacity[v,u] > 0)
                {
                    adj[u].Add(v);
                    adj[v].Add(u);
                }
            }
        }

        pi = new int[n];
        color = new char[n];
    }

    public int ComputeMaxFlow()
    {
        int bridge = search();
        int flow = 0;
        while (bridge != -1)
        {
            // enumerate edge from path from dst to src
            int v = dst, u = pi[v];
            while (u != -1)
            {
                capacity[u, v] -= bridge;
                capacity[v, u] += bridge;
                v = u;
                u = pi[v];
            }
            flow += bridge;

            bridge = search();
        }

        return flow;
    }

    public void DisplayMaxFlow()
    {
        // construct flow matrix from residual flow
        Console.WriteLine("the result max flow is:");
        int[,] flow = new int[n, n];
        for (int u=0; u < n; ++u)
        {
            for (int v=0; v < n; ++v)
            {
                if (capacity[u, v] < capacity_original[u, v])
                {
                    flow[u, v] = capacity_original[u, v] - capacity[u, v];
                    Debug.Assert(capacity[v, u] - capacity_original[v, u] == flow[u, v]);
                }
                Console.Write($"{flow[u, v]}\t");
            }
            Console.WriteLine();
        }

        // compute flow value
        int flowValue = 0;
        for (int v=0; v < n; ++v)
        {
            flowValue += flow[src, v] - flow[v, src];
        }

        Console.WriteLine($"the flow value is: {flowValue}");

        // compute minimal cut value from the flow
        Debug.Assert(color[dst] == 'w');
        List<int> srcCut = new List<int>();
        List<int> dstCut = new List<int>();
        for (int u=0; u < n; ++u)
        {
            if (color[u] == 'b')
            {
                srcCut.Add(u);
            }
            else
            {
                dstCut.Add(u);
            }
        }

        int minCut = 0;
        foreach(int u in srcCut)
            foreach(int v in dstCut)
                minCut += capacity_original[u, v];

        Debug.Assert(flowValue == minCut);

        Console.WriteLine($"the cut value is: {minCut}");
    }

    // search for an augment path from src to dst
    // return the minimal capacity in the path
    // if no path found return -1
    int search()
    {
        // fill pi and color
        Array.Fill(pi, -1);
        Array.Fill(color, 'w');

        // use bfs to search from src to dst
        // tuple means (node, bridge to current node)
        var queue = new Queue<(int, int)>();
        queue.Enqueue((src, int.MaxValue));
        color[src] = 'b'; // when color[u] == 'b' u is reachable from src

        while (queue.Count > 0)
        {
            (int u, int bridge) = queue.Dequeue();
            if (u == dst)
            {
                return bridge;
            }

            foreach(int v in adj[u])
            {
                if (color[v] == 'w' && capacity[u, v] > 0)
                {
                    queue.Enqueue((v, Math.Min(bridge, capacity[u, v])));
                    color[v] = 'b';
                    pi[v] = u;
                }
            }
        }

        // if not found return -1
        return -1;
    }

    private List<List<int> > adj; // adj matrix
    private int[] pi; // store path from src to dst
    private char[] color; // color[u] == 'b' => one can find u when search from src
    private int[,] capacity; // residual capacity
    private int[,] capacity_original;
    private int n; // number of nodes
    private int src;
    private int dst;
}