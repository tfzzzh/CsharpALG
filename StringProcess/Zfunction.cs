namespace CsharpALG.StringProcess;


public class ZFunction
{
    public static List<int> Match(string src, string pattern, char sep='#')
    {
        if (pattern.Length == 0) throw new InvalidDataException("pattern shall not be empty str");
        if (src.Length == 0) return new List<int>();

        string str = string.Concat([pattern, "#", src]);

        var zfunc = GetZFunc(str);
        List<int> positions = new List<int>();
        for (int i=0; i < src.Length; ++i)
        {
            // str[i to i+len) matches with pattern[0 to len)
            int r = zfunc[pattern.Length + 1 + i];
            int len = r - (pattern.Length + 1 + i);
            if (len ==  pattern.Length)
            {
                positions.Add(i + pattern.Length - 1);
            }
        }

        return positions;
    }

    static public int[] GetZFunc(string str)
    {
        int n = str.Length;
        if (n <= 0) throw new InvalidDataException("input str shall not be empty");

        int[] zfunc = new int[n];
        zfunc[0] = -1;
        int l = 0, r = 0; // r is the furthest char currently seen

        for (int i=1; i < n; ++i)
        {
            // initalize zfunc[i] with the help of [l, r)
            int init = i;
            if (l <= i && i < r)
            {
                /*
                0 ... i-l ... r-l
                l ... i   ... r
                */
                int idx = i - l;
                init = Math.Min(r, i + (zfunc[idx] - idx));
            }

            while (init < n && str[init] == str[init - i]) init += 1;

            // update zfunc
            zfunc[i] = init;

            // update [l, r)
            if (init > r)
            {
                l = i;
                r = init;
            }
        }

        return zfunc;
    }
}