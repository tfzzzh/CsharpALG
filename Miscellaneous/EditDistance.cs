namespace CsharpALG.Miscellaneous;

/// <summary>
/// Edit distance is the minimum number of operations required to make two strings be equal.
/// The operations includes insert a new char from a string, remove a char, or substitute a char
///
/// reference: https://en.wikipedia.org/wiki/Levenshtein_distance
/// </summary>
public static class EditDistance
{
    public static int Get(string sa, string sb)
    {
        int m = sa.Length, n = sb.Length;

        int[] distances = new int[n+1];
        int[] distsNext = new int[n+1];

        // distance[0, j] = j
        for (int j=1; j <= n; ++j)
            distances[j] = j;


        for (int i=1; i <= m; ++i)
        {
            distsNext[0] = i;
            for (int j=1; j <= n; ++j)
            {
                int substituteCost = sa[i-1] == sb[j-1] ? 0 : 1;
                distsNext[j] = Math.Min(
                    distances[j] + 1,  // remove sa[i] from sa or insert it into sb
                    Math.Min(
                        distsNext[j-1] + 1, // remove sb[j] from sb or insert it into sa
                        distances[j-1] + substituteCost // substute sa[i] with sb[j] or vis versa
                    )
                );
            }

            (distances, distsNext) = (distsNext, distances);
        }

        return distances[n];
    }
}


public static class EditDistanceExample
{
    public static void Run()
    {
        Console.WriteLine($"Run EditDistanceExample");

        // case: one is empty
        string sa = "aaa";
        string sb = "";
        Console.WriteLine($"distance between sa and sb is {EditDistance.Get(sa, sb)}");

        // case: equal
        sa = "aaa";
        sb = "aaa";
        Console.WriteLine($"distance between sa and sb is {EditDistance.Get(sa, sb)}");

        // case
        sa = "sitting";
        sb = "kitten";
        Console.WriteLine($"distance between sa and sb is {EditDistance.Get(sa, sb)}");

        // complex case
        sa = "aaabbbmnxaaacccc";
        sb = "aaabbbcccaaammnnnxxxeeezzzkkkeeebbbccc";
        Console.WriteLine($"distance between sa and sb is {EditDistance.Get(sa, sb)}");
    }
}
