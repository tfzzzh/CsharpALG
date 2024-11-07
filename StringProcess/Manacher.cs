using System.Text;

namespace CsharpALG.StringProcess;

/// <summary>
/// Manacher alogrithm find all palindrome sub-string in O(N)
/// </summary>
public class Manacher {
    // Assumption: str only contains english alphabeta
    public static ManacherResult FindPalindrome(string str)
    {
        // pad str (say "abc") with "#" (say "#a#b#c#")
        StringBuilder builder = new();
        foreach(char c in str) {
            builder.Append('#');
            builder.Append(c);
        }
        builder.Append('#');
        var padded = builder.ToString();

        var manacherArr = manacherOdd(padded);
        var lenEven = extractEvenLen(manacherArr);
        var lenOdd = extractOddLen(manacherArr);

        return new ManacherResult(){
            LenEven = lenEven,
            LenOdd = lenOdd,
            Str = str
        };
    }

    public static void DisplayResult(ManacherResult mr)
    {
        string str = mr.Str;
        int tot = 0;
        Console.WriteLine($"sub-palindromes of {str}");
        Console.WriteLine("odd palindromes:");
        for (int i=0; i < str.Length; ++i) {
            Console.Write($"centered at {i}: ");
            int len = mr.LenOdd[i];
            tot += len;
            while (len > 0) {
                Console.Write($"{str.Substring(i-len+1, 2*len-1)} ");
                len -= 1;
            }
            Console.WriteLine();
        }

        Console.WriteLine("even palindromes:");
        for (int i=0; i < str.Length; ++i) {
            Console.Write($"centered at {i-1},{i}: ");
            int len = mr.LenEven[i];
            tot += len;
            while (len > 0) {
                Console.Write($"{str.Substring(i-len, 2*len)} ");
                len -= 1;
            }
            Console.WriteLine();
        }
        Console.WriteLine($"total palindromes {tot}");
    }

    /*
    str is of the form: "#a#b#c#d#" manacher_odd returns max length of
    a sub-palindrome centered at i, with 1 <= i <= n-2
    */
    private static int[] manacherOdd(string str) {
        int n = str.Length;
        int[] zlen = new int[n];
        zlen[0] = zlen[n-1] = -1;

        int l = 1, r = 1; // (l, r) is the furthested seen polindrome
        for (int i=1; i < n-1; ++i) {
            int len = 0;
            // init len when i is in the range (l, r)
            if (l < i && i < r) {
                /*
                layout: l...j...i...r with j = l + (r-i)
                suppose (i-len, i+len) is palindrome so it (j-len, j+len)
                i + len <= r
                */
                len = Math.Min(zlen[l+r-i], r-i);
            }

            while (i-len >= 0 && i + len < n && str[i-len] == str[i+len])
                len += 1;

            zlen[i] = len;
            // update (l, r)
            if (r < i + len) {
                l = i - len;
                r = i + len;
            }
        }

        return zlen;
    }

    /*
    say the transformed str is "#a#b#a#" one extract length
    of sub-polindrome with odd length centered at [a, b, a] respectively
    */
    private static int[] extractOddLen(int[] manacherArr) {
        int n = manacherArr.Length / 2;
        int[] lenOdd = new int[n];
        for (int i=0; i < n; ++i) {
            /*
            i=0 -> manacherArr[1] / 2
            i=1 -> manacherArr[3] / 2
            */
            lenOdd[i] = manacherArr[i*2+1] / 2;
        }
        return lenOdd;
    }

    /*
    extract length of sub-polindrome with even length centered at
    index (i-1, i) with i >= 1
    */
    private static int[] extractEvenLen(int[] manacherArr) {
        int n = manacherArr.Length / 2;
        int[] lenEven = new int[n];

        for (int i=1; i < n; ++i) {
            /*
            say: "#a#a#a#"
            i=1 -> manacherArr[2] / 2
            i=2 -> manacherArr[4] / 2
            */
            lenEven[i] = manacherArr[2*i] / 2;
        }

        return lenEven;
    }
}

public record ManacherResult
{
    public required int[] LenOdd;
    public required int[] LenEven;
    public required string Str;
}


static class ExampleManacher
{
    public static void Run() {
        string str = "aaabaaxxaab";
        var result = Manacher.FindPalindrome(str);
        Manacher.DisplayResult(result);
    }
}