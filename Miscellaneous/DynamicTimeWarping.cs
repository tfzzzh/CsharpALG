using System.Diagnostics;

namespace CsharpALG.Miscellaneous;

/// <summary>
/// dynamic time warping find the best alignment between two time series under certain restriction
///
///constrains:
/// 1) Every index in the first (or second) sequence must match to one or more indices from the other sequence
/// 2) the first index of sequence 1 must match to the first of sequence 2
/// 3) the last index of sequence 1 must match to the last of sequence 2
/// 4) the matching from sequence 1 to seqence 2 must be monotonic increasing
/// references: https://en.wikipedia.org/wiki/Dynamic_time_warping
/// </summary>
public class DynamicTimeWarping
{
    public static double Match(
        double[] seq1,
        double[] seq2,
        out List<(int, int)> matches )
    {
        int m = seq1.Length, n = seq2.Length;

        var costs = new double[m+1, n+1];
        var decisions = new int[m+1, n+1]; // 0: match i to j then delete both
                                 // 1: match i to j then delete i
                                 // 2: match i to j then delete j

        for (int i=0; i <=m; ++i)
            for (int j=0; j <= n; ++j)
            {
                costs[i, j] = double.PositiveInfinity;
                decisions[i, j] = -1;
            }
        costs[0, 0] = 0.0;

        for (int i=1; i <= m; ++i)
            for (int j=1; j <= n; ++j)
            {
                double costCurr = Math.Abs(seq1[i-1] - seq2[j-1]);
                double prevCost = double.PositiveInfinity;

                if (costs[i-1, j-1] < prevCost)
                {
                    prevCost = costs[i-1, j-1];
                    decisions[i, j] = 0;
                }

                if (costs[i-1, j] < prevCost)
                {
                    prevCost = costs[i-1, j];
                    decisions[i, j] = 1;
                }

                if (costs[i, j-1] < prevCost)
                {
                    prevCost = costs[i, j-1];
                    decisions[i, j] = 2;
                }

                Debug.Assert(decisions[i, j] >= 0);
                costs[i, j] = costCurr + prevCost;
            }

        // reconstruct matches
        matches = reconstruct(decisions);

        return costs[m, n];
    }

    private static List<(int, int)> reconstruct(int[,] decisions)
    {
        var matches = new List<(int, int)>();
        int m = decisions.GetLength(0) - 1;
        int n = decisions.GetLength(1) - 1;

        if (m == 0 || n == 0) return matches;

        int i = m, j = n;
        while (i > 0 && j > 0)
        {
            matches.Add((i-1, j-1));
            if (decisions[i, j] == 0)
            {
                i -= 1; j -= 1;
            }
            else if (decisions[i, j] == 1)
            {
                i -= 1;
            }
            else
            {
                Debug.Assert(decisions[i, j] == 2, $"decisions[{i},{j}] = {decisions[i,j]}");
                j -= 1;
            }
        }

        Debug.Assert(i == 0 && j == 0);
        matches.Reverse();

        return matches;
    }
}

public static class DynamicTimeWarpingExample
{
    public static void Run()
    {
        Console.WriteLine($"Run DynamicTimeWarpingExample");
        // one is empty
        double[] seq1 = [];
        double[] seq2 = [1.0, 2.0, 3.0];
        run(seq1, seq2);

        // equal sequences
        seq1 = [1.0, 5.0, 3, 8, 2.6, 4.2, 3.7, 9.5, 8.8];
        seq2 = [1.0, 5.0, 3, 8, 2.6, 4.2, 3.7, 9.5, 8.8];
        run(seq1, seq2);

        // when seq2 contains median of seq1
        seq1 = [2.0992827650689465, 0.909263296567894, 1.3701112813172824,
        -3.386393131045394, -5.327709079378103, 2.628893800954665,
         -2.12886629965533, -1.7489157663545256, 1.9176656473913356,
          -4.294894373870964, -3.802884601889078, 1.8804840754958039,
           -7.089759103706598, 0.7188956172747512, -3.669509016895343,
            -0.45903640530036316];

        seq2 = [1.1396872889425882, -1.9388910330049276, -0.961200263196637, -2.064272711097853];

        run(seq1, seq2);
    }

    static void run(double[] seq1, double[] seq2)
    {
        double cost;
        List<(int, int)> matches;

        cost = DynamicTimeWarping.Match(seq1, seq2, out matches);

        Console.WriteLine($"distance between seq1 and seq2 is {cost}");
        Console.WriteLine("matches: ");
        Console.Write('\t');
        foreach(var tuple in matches)
        {
            Console.Write($"({tuple.Item1}, {tuple.Item2}) ");
        }
        Console.WriteLine();
    }
}