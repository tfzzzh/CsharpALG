using CsharpALG.StringProcess;
test_match();
test_zfunc();

void test_match()
{
    _test_match("aaaaa", "aaa", KnuthMorrisPratt.Match);
    _test_match("aaaaa", "aaa", (x, y) => ZFunction.Match(x, y));
    _test_match("aaaabaaacaaad","aaaac", KnuthMorrisPratt.Match);
    _test_match("aaaabaaacaaad","aaaac",(x, y) => ZFunction.Match(x, y));
    _test_match("aaaabaaaabaaaab","aaab", KnuthMorrisPratt.Match);
    _test_match("aaaabaaaabaaaab","aaab", (x, y) => ZFunction.Match(x, y));
    _test_match("aabaabaabaaaabaa", "aabaabaa", KnuthMorrisPratt.Match);
    _test_match("aabaabaabaaaabaa", "aabaabaa", (x, y) => ZFunction.Match(x, y));
}

void _test_match(string src, string pattern, Func<string, string, List<int> > method)
{
    var positions = KnuthMorrisPratt.Match(src, pattern);

    if (positions.Count > 0)
    {
        Console.WriteLine($"match points of {src} and {pattern} are:");
        foreach(int p in positions)
        {
            Console.Write($"{p} ");
        }
        Console.WriteLine();
    }
    else
    {
         Console.WriteLine($"{src} and {pattern} do not match");
    }
}

void test_zfunc()
{
    _test_zfunc("aaaa");
    _test_zfunc("aaaab");
    _test_zfunc("aaaabaaaab");
}

void _test_zfunc(string str)
{
    Console.WriteLine($"zfunc of {str}");
    int[] zfunc = ZFunction.GetZFunc(str);
    for (int i=1; i < str.Length; ++i)
    {
        Console.WriteLine($"str {i} to {zfunc[i]} : {str.Substring(i, zfunc[i] - i)} is a prefix");
    }
}