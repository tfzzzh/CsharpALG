namespace CsharpALG.StringProcess;


/// <summary>
/// KMP algorithm mathes string src to string pattern in O(m+n) where m,n are lengthes of the two strings
/// </summary>
public class KnuthMorrisPratt
{
    public static List<int> Match(string src, string pattern)
    {
        if (pattern.Length == 0)
            throw new InvalidDataException("pattern's length cannot be zero");

        if (src.Length == 0) return new List<int>();

        var pi = computePi(pattern);
        var positions = getMatch(src, pattern, pi);

        return positions;
    }

    static int[] computePi(string pattern)
    {
        int n = pattern.Length;

        int[] pi = new int[n + 1];
        pi[0] = -1;
        pi[1] = 0;

        // k pattern[:k) is suffix of pattern[:i)
        int k = 0;
        for (int i = 1; i < n; ++i)
        {
            // compute pi[i+1] whitch correspond to max
            // suffix of pattern[:i+1)
            while (k > 0 && pattern[k] != pattern[i])
                k = pi[k];

            if (pattern[k] == pattern[i])
                k += 1;

            pi[i + 1] = k;
        }

        return pi;
    }

    static List<int> getMatch(string src, string pattern, int[] pi)
    {
        int k = 0;
        List<int> positions = new List<int>();
        // pattern[:k) is suffix of src[:i)
        for (int i = 0; i < src.Length; ++i)
        {
            char c = src[i];
            while (k > 0 && pattern[k] != c)
                k = pi[k];

            if (pattern[k] == c)
                k += 1;

            if (k == pattern.Length)
            {
                positions.Add(i);
                k = pi[k];
            }
        }

        return positions;
    }
}