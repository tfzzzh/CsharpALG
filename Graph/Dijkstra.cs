using System.Diagnostics;

namespace CsharpALG.Graph;

/// <summary>
/// Dijkstra solve the single source shortest path problem in O(|E| log |V|).
/// The graph edges must be non-negative from this algorithm
/// </summary>
public class Dijkstra
{
    public Dijkstra(List<List<int[]>> graph, int src)
    {
        this.graph = graph;
        this.src = src;
        pi = new int[graph.Count];
        distance = new int[graph.Count];

        // fill pi and dist
        Array.Fill(pi, -1);
        Array.Fill(distance, int.MaxValue);
    }

    /// <summary>
    /// compute all distance from the source. If node u is
    /// unreachable from the source, the distance to it is set to INT_MAX
    /// </summary>
    public void computeDistances()
    {
        // insert sort to the priority_queue
        distance[src] = 0;
        var que = new PriorityQueue<int, int>(); // (dist, u)
        que.Enqueue(src, 0);

        // get a node from priority_queue and relax distance
        while (que.Count > 0)
        {
            // get node and its distance to expand
            int u, dist;
            que.TryDequeue(out u, out dist);

            if (dist > distance[u]) continue;

            // dist <= distance[u]
            foreach(var node in graph[u])
            {
                int v = node[0];
                int w = node[1];

                int relax = dist + w;
                if (relax < distance[v])
                {
                    distance[v] = relax;
                    pi[v] = u;
                    que.Enqueue(v, relax);
                }
            }
        }
    }

    // display functions
    public void PrintDistance()
    {
        Console.Write("distances:");
        foreach (int d in distance)
        {
            if (d < int.MaxValue)
                Console.Write($"{d} ");
            else
                Console.Write("NULL ");
        }
        Console.WriteLine();
    }

    public void PrintRoute(int u)
    {
        if (distance[u] == int.MaxValue)
        {
            Console.WriteLine($"node {src} and {u} is disconnected");
            return;
        }

        Console.Write($"shortest path ({src}->{u}):");
        var stack = new Stack<int>();
        while (u != -1)
        {
            stack.Push(u);
            u = pi[u];
        }

        Debug.Assert(stack.Peek() == src);

        while (stack.Count > 0)
        {
            Console.Write($"{stack.Pop()} ");
        }

        Console.WriteLine();
    }

    private List<List<int[]> > graph;
    private int src;
    private int[] pi; // parent of a node u in forest formed by shorted path
    private int[] distance;
}