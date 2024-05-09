using System.Diagnostics;

namespace CsharpALG.MaxFlow;

/// <summary>
/// For a flow graph with |V| node, this algorithm solves the
/// maxflow problem in O(|V|^3)
/// </summary>
class RelabelToFront
{
    public RelabelToFront(int[,] capacity, int src, int dst)
    {
        residual = (int[,]) capacity.Clone();

        int n = capacity.GetLength(0);
        excess = new int[n];
        height = new int[n];

        this.src = src;
        this.dst = dst;
        this.n = n;

        adj = new List<List<int>>(n);
        for (int u=0; u < n; ++u) adj.Add(new List<int>());
        for (int u=0; u < n; ++u)
        {
            for (int v = u + 1; v < n; ++v)
            {
                if (capacity[u, v] > 0 || capacity[v, u] > 0)
                {
                    adj[u].Add(v);
                    adj[v].Add(u);
                }
            }
        }
        indices = new int[n];
        list = new LinkedList<int>();
        initPreflow();
    }

    public int ComputeMaxFlow()
    {
        LinkedListNode<int>? itr = list.First;

        while (itr is not null)
        {
            int u = itr.Value;
            int hold = height[u];

            discharge(u);

            // relabel happend
            if (hold < height[u])
            {
                list.Remove(itr);
                list.AddFirst(itr);
            }

            itr = itr.Next;
        }

        Debug.Assert(excess[src] + excess[dst] == 0);
        for (int u=0; u < n; ++u)
        {
            if (u != src && u != dst)
                Debug.Assert(excess[u] == 0);
        }
        return excess[dst];
    }

    // push all excess flow in u to its neighbor
    private void discharge(int u)
    {
        while (excess[u] > 0)
        {
            // for (int i=0; i < adj[u].Count && excess[u] > 0; ++i)
            // {
            //     int v = adj[u][i];
            //     if (
            //         height[u] == height[v] + 1 &&
            //         residual[u, v] > 0
            //     )
            //     {
            //         push(u, v);
            //     }
            // }

            // if (excess[u] > 0)
            //     relabel(u);

            int idx = indices[u];

            // case 1: last possible leaving edge have been explored
            if (idx == adj[u].Count)
            {
                relabel(u);
                indices[u] = 0;
            }
            else
            {
                int v = adj[u][idx];
                // case 2: current leaving edge can push
                if (height[u] == height[v] + 1 && residual[u, v] > 0)
                {
                    push(u, v);
                }
                // case 3: current leaving edge can not push
                else
                {
                    indices[u] += 1;
                }
            }
        }
    }

    // push flow from u to v, this action is
    // valid if excess[u] > 0 && height[u] = height[v] + 1
    // && there is an edge from u to v
    private void push(int u, int v)
    {
        int flow = Math.Min(excess[u], residual[u, v]);
        residual[u, v] -= flow;
        residual[v, u] += flow;
        excess[u] -= flow;
        excess[v] += flow;
    }

    // this action is valid if excess[u] > 0
    private void relabel(int u)
    {
        int hold = height[u];
        int hnew = int.MaxValue;
        foreach(int v in adj[u])
        {
            if (residual[u, v] > 0)
            {
                hnew = Math.Min(hnew, height[v] + 1);
            }
        }
        height[u] = hnew;
        Debug.Assert(hnew!=int.MaxValue && hold < hnew);
    }


    // overflow node from src to its neighbors
    private void initPreflow()
    {
        foreach(int u in adj[src])
        {
            if (residual[src, u] > 0)
            {
                int flow = residual[src, u];
                residual[src, u] -= flow;
                residual[u, src] += flow;

                excess[src] -= flow;
                excess[u] += flow;
            }
        }

        // insert all node other than s, t to list
        for (int u=0; u < n; ++u)
        {
            if (u == src || u == dst) continue;
            if (excess[u] > 0)
            {
                list.AddFirst(u);
            }
            else
            {
                list.AddLast(u);
            }
        }

        height[src] = n;
        height[dst] = 0;
    }

    // data segment
    private int[,] residual; // residual capacity
    private int[] excess; // excess flow over each node
    // if there is a tunnel from u to v in the residual
    // graph then height[u] <= height[v] + 1
    private int[] height; // height function
    private int[] indices; // next possible leaving edge to explore
    private int src; // source
    private int dst; // destination
    private int n; // number of nodes
    private List<List<int>> adj; // adjacent matrix
    private LinkedList<int> list; // store node to recharge
}