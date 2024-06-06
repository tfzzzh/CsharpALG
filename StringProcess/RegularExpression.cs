namespace CsharpALG.StringProcess;

/// <summary>
/// Implementation of Thompson's method (https://en.wikipedia.org/wiki/Thompson%27s_construction)
/// for regular expression.
/// For simplicity only or operator "|", and closure operator "*" is supported
/// </summary>
public class RegularExpression
{
    public RegularExpression(string pattern)
    {
        n = pattern.Length;
        if (n == 0) throw new InvalidDataException("input pattern shall not be empty");

        this.pattern = pattern;
        nfa = new List<List<int> >();
        for (int i=0; i <= n; ++i)
            nfa.Add(new List<int>());

        parse();
    }

    // check if str matches the pattern
    public bool Match(string str)
    {
        // init states with start position
        List<int> states = getReachable(new List<int>([0]));

        // get char from str to transform states
        foreach(char c in str)
        {
            if (c == '*' || c == '(' || c == ')' || c == '|')
                throw new Exception("input str cannot contains *, (, ), |");

            List<int> successor = new List<int>();
            foreach(int state in states)
            {
                if (state < n && (pattern[state] == c || pattern[state] == '.'))
                {
                    successor.Add(state+1);
                }
            }

            states = getReachable(successor);
        }

        // check if final state have been reached
        foreach(int state in states)
            if (state == n) return true;

        return false;
    }

    // parse the regular expression into a NFA
    void parse()
    {
        var positions = new Stack<int>();
        for (int i=0; i < n; ++i)
        {
            char c = pattern[i];
            int lp = i; // start position of currently handled term
            int rp = i; // end position of currently handled term

            // term start one shall push this position into stack
            if (c == '(' || c == '|')
            {
                positions.Push(i);
            }
            // term found
            else if (c == ')')
            {
                if (positions.Count == 0) throw new Exception("parents in pattern not matched");
                if (pattern[positions.Peek()] == '(')
                {
                    lp = positions.Pop();
                }
                // top is '|'
                else
                {
                    int or = positions.Pop();
                    lp = positions.Pop();

                    // parse '|'
                    // add edge from lp to or + 1
                    // add edge from or to rp
                    nfa[lp].Add(or + 1);
                    nfa[or].Add(rp);
                }
            }

            // handle term*
            if (i+1 < n && pattern[i+1] == '*')
            {
                // add epsilon tranform (lp -> *) and (* -> lp)
                // that is add edge (lp, i+1) and (i+1, lp)
                nfa[lp].Add(i+1);
                nfa[i+1].Add(lp);
            }

            // add epsilon tranfrom for ( *  ) to next state
            if (pattern[i] == '(' || pattern[i] == ')' || pattern[i] == '*')
            {
                nfa[i].Add(i+1);
            }
        }

        if (positions.Count > 0) throw new Exception("invalid regular expression");
    }

    // get all reachable points from start_points in the nfa
    List<int> getReachable(List<int> start_points)
    {
        var reachable = new List<int>();
        if (start_points.Count == 0) return reachable;

        bool[] visited = new bool[n+1];
        var queue = new Queue<int>();
        foreach(int u in start_points)
        {
            if (!visited[u])
            {
                visited[u] = true;
                queue.Enqueue(u);
            }
        }

        while (queue.Count > 0)
        {
            int u = queue.Dequeue();
            reachable.Add(u);

            foreach(int v in nfa[u])
            {
                if (!visited[v])
                {
                    visited[v] = true;
                    queue.Enqueue(v);
                }
            }
        }

        return reachable;
    }

    private string pattern;
    private List<List<int> > nfa;
    int n; // length of the pattern && finish state
}