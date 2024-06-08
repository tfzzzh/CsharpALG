using System.Diagnostics;

namespace CsharpALG.StringProcess;

class SuffixAutoma
{
    public SuffixAutoma(string text)
    {
        states = new List<State>();

        // insert state correspond to empty substr
        states.Add(new State(){Link = -1, Len = 0});

        // extend states in a online way
        foreach(char c in text)
        {
            extendAutoma(c);
        }
    }

    public static string FindLargestCommonSubstr(string str1, string str2)
    {
        // build a suffix automa for str1
        var automa = new SuffixAutoma(str1);

        // find longgest substr in str1 correspond to str2[:i)
        // initially only empty substr is matched
        int p = 0, len = 0;
        int pos = -1, maxLen = 0;
        for (int i=0; i < str2.Length; ++i)
        {
            char c = str2[i];
            // str2[:i) matches state p, when seen c
            // one shall search suffix corrspond to p
            while (p > 0 && !automa.states[p].Trans.ContainsKey(c))
            {
                p = automa.states[p].Link;
                len = automa.states[p].Len;
            }

            if (automa.states[p].Trans.ContainsKey(c))
            {
                p = automa.states[p].Trans[c];
                len += 1;
            }

            if (len > maxLen)
            {
                pos = i;
                maxLen = len;
            }
        }

        if (maxLen == 0)
            return "";

        Debug.Assert(pos >= 0 && pos - maxLen + 1 >= 0);
        return str2.Substring(pos-maxLen+1, maxLen);
    }

    // suppose automa for s is already build, now we extend it to s + c
    private void extendAutoma(char c)
    {
        // build a new state for s + c
        int curr = states.Count;
        states.Add(new State(){Len = states[last].Len + 1});

        // for a suffix p of s, p + c is a suffix of s + c
        // thus one need to add link from p to the new state when
        // p does not have an leaving edge labeled with c
        int p = last;
        while (p != -1 && !states[p].Trans.ContainsKey(c))
        {
            states[p].Trans.Add(c, curr);
            p = states[p].Link;
        }

        // all suffix of s + c doest not appear in s
        if (p == -1)
        {
            states[curr].Link = 0;
        }
        else
        {
            // p -c-> q
            int q = states[p].Trans[c];

            // if the longest substr in q is a suffix of s + c
            if (states[p].Len + 1 == states[q].Len)
            {
                states[curr].Link = q;
            }
            // q contains longer string that is not a suffix of s + c
            else
            {
                // clone state q
                int clone = states.Count;
                states.Add(states[q].Clone());
                states[clone].Len = states[p].Len + 1;

                // clone contains suffix of s + c which divided from q
                // all p's parents if enter to q now shall enter clone
                while (p != -1 && states[p].Trans[c] == q)
                {
                    states[p].Trans[c] = clone;
                    p = states[p].Link;
                }

                states[q].Link = clone;
                states[curr].Link = clone;
            }
        }

        last = curr;
    }

    // data segement
    private List<State> states;
    // state id correspond to last seen str, when complete it contains
    // the entire text
    private int last;

    // state of the automa
    class State
    {
        public State()
        {
            Trans = new Dictionary<char, int>();
            Link = -1;
            Len = 0;
        }

        public State Clone()
        {
            State state = new State();
            state.Link = Link;
            state.Len = Len;

            foreach(var kv in Trans)
            {
                state.Trans.Add(kv.Key, kv.Value);
            }

            return state;
        }

        // link to its parent state
        public int Link;
        // max length of the substr belongs to the state
        public int Len;
        // out edges
        public Dictionary<char, int> Trans;
    }
}