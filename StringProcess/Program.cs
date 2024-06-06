using System.Diagnostics;
using CsharpALG.StringProcess;
test_match();
test_zfunc();
test_regular_expression();

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

void test_regular_expression()
{
    // _test_regular_expression("", "a*", true);
    // _test_regular_expression("", "(abc)*", true);
    // _test_regular_expression("abcabc", "(abc)*", true);
    // _test_regular_expression("abcab", "(abc)*", false);
    // _test_regular_expression("abc", "(abc|def)", true);
    // _test_regular_expression("def", "(abc|def)", true);
    // _test_regular_expression("abd", "(abc|def)", false);
    // _test_regular_expression("ghi", "(((abc|def)|ghi)|jke)", true);
    _test_regular_expression("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaac", "(a|aa)*b", false);
    _test_regular_expression("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaac", "((a*)*|b*)", false);
    _test_regular_expression("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "((a*)*|b*)", true);
    _test_regular_expression("xyzABDDDDEFG", ".*AB(((C|D*E)F)*G)", true);
    _test_regular_expression("xyzABDDDDFG", ".*AB(((C|D*E)F)*G)", false);
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

void _test_regular_expression(string src, string pattern, bool? result = null)
{
    var alg = new RegularExpression(pattern);
    bool isMatch = alg.Match(src);
    if (result is not null)
        Debug.Assert(isMatch == result.Value);

    if (isMatch)
    {
        Console.WriteLine($"'{src}' matches '{pattern}'");
    }
    else
    {
        Console.WriteLine($"'{src}' does not match '{pattern}'");
    }
}