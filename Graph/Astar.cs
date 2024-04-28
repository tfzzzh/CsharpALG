namespace CsharpALG.Graph;

/// <summary>
/// Astar find the shortest path from a given source node to the given destination.
/// Astar need to specify a hueristic function h(n) which is a lower bound from node n to the destination
/// It use h(n) to construct a score function and expand the node which has the smallest score function greedily
/// Unlike Dijkstra, Astar may expand a node multiple times
/// </summary>
public class Astar
{
    public Astar(List<List<int[]> > graph)
    {
        this.graph = graph;
        n = graph.Count;
        distance = new int[n];
        pi = new int[n];
    }

    public int computeDistance(int source, int destination, int[] heuristic)
    {
        // init distance[] array
        Array.Fill(distance, int.MaxValue);
        Array.Fill(pi, -1);

        // insert source into queue
        distance[source] = 0;
        var queue = new PriorityQueue<int, int>();
        queue.Enqueue(source, distance[source] + heuristic[source]);

        // take node with least score and perform update
        while (queue.Count > 0)
        {
            int u, score;
            queue.TryDequeue(out u, out score);
            int dist = score - heuristic[u];
            if (u == destination) break;

            if (dist > distance[u]) continue;

            // update distances of neighbors
            foreach(var node in graph[u])
            {
                int v = node[0], w = node[1];
                int relax = dist + w;

                if (relax < distance[v])
                {
                    pi[v] = u;
                    distance[v] = relax;
                    queue.Enqueue(v, relax + heuristic[v]);
                }
            }
        }

        // return distance
        return distance[destination];
    }

    public void PrintRoute(int source, int destination)
    {
        if (distance[destination] == int.MaxValue)
        {
            Console.WriteLine($"{source} to {destination} is not connected");
            return;
        }

        var stack = new Stack<int>();
        int u = destination;
        while (u != -1)
        {
            stack.Push(u);
            u = pi[u];
        }

        Console.Write($"Path:({source} -> {destination}):");
        while (stack.Count > 0)
        {
            u = stack.Pop();
            Console.Write($"{u} ");
        }
        Console.WriteLine();
    }

    private List<List<int[]> > graph;
    private int n; // number of node
    private int[] distance; // known distance from the source
    private int[] pi; // parent of current node in optimal path
}