using System.Diagnostics;

namespace CsharpALG.Graph;

/// <summary>
/// FloydWarshall algorithm find all pair of shortest path of a graph in O(|v|^3).
/// FloydWarshall allows negative edge, but assumes that the graph does not have negative cycle
/// </summary>
public class FloydWarshall
{
    public FloydWarshall(int[,] graph)
    {
        this.graph = graph;
        n = graph.GetLength(0);
        pi = new int[n, n];

        for (int i=0; i < n; ++i)
            for (int j=0; j < n; ++j)
                pi[i, j] = -1;
    }

    public void computeDistances()
    {
        // check if diagnal is no negative
        // set diagnal to 0 when it is > 0
        for (int i=0; i < n; ++i)
        {
            if (graph[i,i] < 0) throw new InvalidDataException($"graph[{i},{i}] is negative");
            graph[i, i] = 0;
        }

        // init distance to shortest path without intermediate point
        distance = new int[n, n];
        var distanceNext = new int[n, n];
        for (int i=0; i < n; ++i)
            for (int j=0; j < n; ++j)
                distance[i, j] = graph[i, j];

        // update distance when on can travel to k
        for (int k=0; k < n; ++k)
        {
            for (int i=0; i < n; ++i)
                for (int j=0; j < n; ++j)
                {
                    int dist = distance[i, j];
                    if (
                        distance[i, k] < int.MaxValue &&
                        distance[k, j] < int.MaxValue &&
                        distance[i, k] + distance[k, j] < dist
                    )
                    {
                        dist = distance[i, k] + distance[k, j];
                        pi[i, j] = k;
                    }

                    distanceNext[i, j] = dist;
                }

            (distance, distanceNext) = (distanceNext, distance);
        }

        // check if negative graph exist via relax on shorted path
        for (int i=0; i < n; ++i)
            for (int j=0; j < n; ++j)
            {
                int dist = int.MaxValue;

                // relax dist using distance[i, k] + graph[k, j]
                for (int k=0; k < n; ++k)
                {
                    if (distance[i, k] < int.MaxValue && graph[k, j] < int.MaxValue)
                    {
                        dist = Math.Min(dist, distance[i, k] + graph[k, j]);
                    }
                }

                if (dist < distance[i, j]) throw new InvalidDataException("found negative loop in graph");
                if (dist > distance[i, j]) throw new InvalidProgramException("algorithm not converge");
            }
    }

    public void PrintDistance()
    {
        if (distance is null) throw new NullReferenceException("computeDistances is not called");
        Console.Write("distances:\n");

        for (int i=0; i < n; ++i)
        {
            for (int j=0; j < n; ++j)
            {
                int d = distance[i, j];
                if (d < int.MaxValue)
                    Console.Write($"{d} ");
                else
                    Console.Write("- ");
            }
            Console.WriteLine();
        }
    }

    public void PrintRoute(int u, int v)
    {
        if (distance is null) throw new NullReferenceException("computeDistances is not called");
        if (distance[u, v] == int.MaxValue)
        {
            Console.WriteLine($"this no path exist from {u} to {v}");
            return;
        }

        if (u == v)
        {
            Console.Write($"shortest path ({u}->{u}):{u}");
            return;
        }

        Console.Write($"shortest path ({u}->{v}):");
        Console.Write($"{u} ");
        _printRoute(u, v);
        Console.Write($"{v} ");
        Console.WriteLine();
    }

    private void _printRoute(int u, int v)
    {
        if (u == v)
        {
            return;
        }

        int k = pi[u, v];
        if (k == -1) return;

        _printRoute(u, k);
        Console.Write($"{k} ");
        _printRoute(k, v);
    }

    // graph is stored as n * n matrix, with graph[i, j]
    // being the edge weight of node i and node j
    // when there exist no edge between (i, j) graph[i,j] == INFTY
    private int[,] graph;
    private int[,]? distance;
    private int n;
    // pi[i, j] stores intermediate point of j in the shorted path
    private int[,] pi;
}