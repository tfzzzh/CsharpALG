using System.Diagnostics;

namespace CsharpALG.Graph;

/// <summary>
/// The Hungarian algorithm solves the min cost match problem of
/// a bipartite graph in O(|V|^3).
/// More formally, Let G = (V_A, V_B) be a bipartite graph with
/// node set V_A, V_B. For simplicity we assume V_A and V_B has the
/// same size. Let W[i, j] being the weight of the graph. The min
/// match problem solve the following optimization problem
///
///     min_{X} \sum_{i, j} X[i,j] W[i, j]
///         s.t. X[i, j] >= 0
///              \sum_{i} X[i, j] = 1
///              \sum_{j} X[i, j] = 1
///
/// The Dual optimization problem of the above minimization is
///
///     max_{u, v} \sum_{i} u[i] + \sum_{j} v[j]
///         s.t. u[i] + v[j] <= w[i, j]
///
/// The Hungarian can be viewed as a primal-dual optimization method
/// for these primal and dual form linear optimization problem.
/// </summary>
class Hungarian
{
    public Hungarian(int[, ] w)
    {
        m = w.GetLength(0);
        n = w.GetLength(1);

        if (m > n)
            throw new InvalidDataException("w shall be a rectangle matrix with num_row <= num_col");

        // we use 0-th row and 0-th col as sentinal
        // add sential rows and cols to the weight matrix
        weight = new int[m+1, n+1];
        for (int i=1; i <= m; ++i)
            for (int j=1; j <= n; ++j)
            {
                weight[i, j] = w[i-1, j-1];
            }

        // allocate memeory for potential and match
        u = new int[m+1];
        v = new int[n+1];
        match = new int[n+1]; // init to 0: which means null

        // init potentials
        init_potential();
    }

    public int GetMinCost()
    {
        // find match for i-th col
        int[] ways = new int[n+1]; //
        int[] minv = new int[n+1]; // min margin to col-j
        bool[] visited = new bool[n+1]; // is col-j expanded
        for (int i=1; i <= m; ++i)
        {
            // init minv to INF, visited to false
            Array.Fill(minv, int.MaxValue);
            Array.Fill(visited, false);

            // we start by match 0-th column to i-th row.
            // then find a better columns to take the match of 0-th column
            match[0] = i;
            int j = 0;

            // when match[j] != 0 we shall find a new column to take the match
            // when match[j] == 0, we find a free column, we break the loop and
            // expand the match
            while (match[j] != 0)
            {
                visited[j] = true;

                // find next column to expand. The expand column shall have minimal
                // margin to the src
                int src = match[j];
                int delta = int.MaxValue;
                int jexpand = 0;
                for (int k=1; k <= n; ++k)
                {
                    if (!visited[k])
                    {
                        int margin = weight[src, k] - u[src] - v[k];
                        Debug.Assert(margin >= 0);
                        if (margin < minv[k])
                        {
                            minv[k] = margin;
                            // k may matched with src
                            ways[k] = j;
                        }

                        if (delta > minv[k])
                        {
                            delta = minv[k];
                            jexpand = k;
                        }
                    }
                }

                // add edge to the graph via modifying potentials
                for (int k=0; k <= n; ++k)
                {
                    if (visited[k])
                    {
                        u[match[k]] += delta;
                        v[k] -= delta;
                    }
                    else
                    {
                        minv[k] -= delta;
                    }
                }

                j = jexpand;
            }

            // modify match
            while (j != 0)
            {
                int taken = ways[j];
                match[j] = match[taken];
                j = taken;
            }
        }

        // return mincost whitch equals to the sum of potentials
        // exclude 0-th row and 0-th col
        result = 0;
        for (int i=1; i <= m; ++i) result += u[i];
        for (int j=1; j <= n; ++j) result += v[j];
        return result;
    }

    public bool CheckResult(bool verbose=true)
    {
        // check if every row is matched once exclude 0
        var num_matches = new int[m+1];
        for (int j=1; j <= n; ++j)
        {
            if (match[j] > 0)
            {
                num_matches[match[j]] += 1;
                if (verbose)
                    Console.WriteLine($"match {match[j]} to {j} weight: {weight[match[j], j]}");
            }
        }

        for (int i=1; i <= m; ++i)
            if (num_matches[i] != 1) return false;

        int primal = 0;
        for (int j=1; j <= n; ++j)
        {
            if (match[j] > 0)
            {
                primal += weight[match[j], j];
            }
        }

        return result == primal;
    }

    // u[i] + v[j] <= weight[i, j] for i \in [1, m], j \in [1, n]
    // => if we init u[i] to 0, v[j] <= min{weight[i,j]}
    void init_potential()
    {
        for (int j=1; j <= n; ++j)
        {
            v[j] = int.MaxValue;
            for (int i=1; i <= m; ++i)
            {
                v[j] = Math.Min(v[j], weight[i, j]);
            }
        }
    }

    private int[,] weight; // weight of the bipartie graph
    private int m, n; // number of rows and columns
    // potentials
    private int[] u;
    private int[] v;
    // matches of a column
    private int[] match;
    int result;
}