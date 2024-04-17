namespace CsharpALG.Graph;

public class StronglyConnectedComponent
{
    public StronglyConnectedComponent(List<List<int>> graph)
    {
        this.graph = graph;
        int n = graph.Count;

        // parent of a node in dfs forest
        pi = new int[n];
        Array.Fill(pi, -1);

        // init transpose of the graph
        graphT = new List<List<int>>(n);
        for (int i = 0; i < n; ++i) graphT.Add(new List<int>());
        for (int i = 0; i < n; ++i)
        {
            // edge (i, j) -> (j, i)
            foreach (int j in graph[i])
            {
                graphT[j].Add(i);
            }
        }

        numComps = -1;
    }

    public int GetNumComponents()
    {
        if (numComps == -1)
        {
            List<int> order = getOrderFromGraphT();
            numComps = getComponentFromGraph(order);
        }
        return numComps;
    }

    public List<List<int>> ListComponents()
    {
        int m = GetNumComponents();
        List<List<int>> comps = new List<List<int>>(m);

        int n = graph.Count;
        int[] rootIds = new int[n];
        Array.Fill(rootIds, -1);

        Stack<int> stk = new Stack<int>();
        for (int u=0; u < n; ++u)
        {
            if (rootIds[u] == -1)
            {
                // insert u into stack until its root is known
                int v = u;
                while (rootIds[v] == -1 && pi[v] != -1)
                {
                    stk.Push(v);
                    v = pi[v];
                }

                if (rootIds[v] == -1) rootIds[v] = v;

                // fill rootids in stack
                int rid = rootIds[v];
                while (stk.Count > 0)
                {
                    v = stk.Pop();
                    rootIds[v] = rid;
                }
            }
        }

        // partition components according to there roots
        var dict = new Dictionary<int, List<int> >();
        for (int u=0; u < n; ++u)
        {
            if (!dict.ContainsKey(rootIds[u]))
                dict.Add(rootIds[u], new List<int>());

            dict[rootIds[u]].Add(u);
        }

        foreach(var kv in dict)
        {
            comps.Add(kv.Value);
        }

        return comps;
    }

    private List<int> getOrderFromGraphT()
    {
        List<int> order = new List<int>();

        int n = graphT.Count;
        char[] color = new char[n];
        Array.Fill(color, 'w');

        for (int i = 0; i < n; ++i)
        {
            if (color[i] == 'w')
                dfs(graphT, i, color, order, null);
        }

        order.Reverse();
        return order;
    }

    private int getComponentFromGraph(List<int> order)
    {
        numComps = 0;
        int n = graph.Count;
        char[] color = new char[n];
        Array.Fill(color, 'w');

        foreach (int u in order)
        {
            if (color[u] == 'w')
            {
                dfs(graph, u, color, null, pi);
                numComps += 1;
            }
        }

        return numComps;
    }

    static private void dfs(List<List<int>> graph, int u, char[] color, List<int>? order, int[]? pi)
    {
        color[u] = 'g';
        foreach (int v in graph[u])
        {
            if (color[v] == 'w')
            {
                if (pi is not null) pi[v] = u;
                dfs(graph, v, color, order, pi);
            }
        }
        // order is sorted by finish time in increasing order
        if (order is not null) order.Add(u);
        color[u] = 'b';
    }

    // graph does not contains self-loop && parallel edges
    // when node i does not have connected node, graph[i]
    // shall set to an empty list (not null)
    private List<List<int>> graph;
    private List<List<int>> graphT;
    private int[] pi; // dfs tree formed by dfs
    private int numComps; // number of components
}