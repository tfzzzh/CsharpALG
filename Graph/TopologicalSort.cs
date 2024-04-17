namespace CsharpALG.Graph;

public class TopologicalSort
{

    /// <summary>
    /// build a graph from the given edges. Assume that edges
    /// contains tuple of the form (u, v). Assume not self loop
    /// and parallel edge are contained in edges
    /// </summary>
    /// <param name="n"></param>
    /// <param name="edges"></param>
    public TopologicalSort(int n, int[,] edges)
    {
        graph = new List<List<int>>(n);
        for (int i = 0; i < n; ++i)
        {
            graph.Add(new List<int>());
        }

        int m = edges.GetLength(0);
        for (int i = 0; i < m; ++i)
        {
            int u = edges[i, 0], v = edges[i, 1];
            graph[u].Add(v);
        }
    }

    /// <summary>
    /// topological sort via deepth first search
    /// </summary>
    /// <param name="order">when the graph does not contains a loop, order is the
    ///         topological order of the graph</param>
    /// <returns>return true if the graph is acyclic, else return false</returns>
    public bool SortViaDFS(out List<int> order)
    {
        int n = graph.Count;
        char[] color = new char[n];
        for (int i = 0; i < n; ++i) color[i] = 'w';

        order = new List<int>();
        for (int i = 0; i < n; ++i)
        {
            if (color[i] == 'w')
            {
                var success = dfs(i, color, order);
                if (!success)
                    return false;
            }
        }

        order.Reverse();
        return true;
    }

    public bool SortViaBFS(out List<int> order)
    {
        int n = graph.Count;

        // init degree
        int[] degree = new int[n];
        for (int i=0; i < n; ++i)
        {
            foreach(int j in graph[i])
            {
                degree[j] += 1;
            }
        }

        // insert zero-degree node into tree
        var que = new Queue<int>();
        for (int u=0; u < n; ++u)
        {
            if (degree[u] == 0) {
                que.Enqueue(u);
            }
        }

        // bfs
        order = new List<int>();
        while (que.Count > 0)
        {
            int u = que.Dequeue();
            order.Add(u);

            foreach(int v in graph[u])
            {
                degree[v] -= 1;
                if (degree[v] == 0)
                    que.Enqueue(v);
            }
        }

        // return true when all node visit
        return order.Count == n;
    }

    bool dfs(int u, char[] color, List<int> order)
    {
        bool success = true;
        color[u] = 'g';
        foreach (int v in graph[u])
        {
            if (color[v] == 'g')
            {
                success = false;
            }
            else if (color[v] == 'w')
            {
                success = dfs(v, color, order);
            }

            if (!success) break;
        }
        color[u] = 'b';
        order.Add(u);
        return success;
    }

    // data segment
    private List<List<int>> graph;
}