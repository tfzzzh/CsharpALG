using System.Diagnostics;

namespace CsharpALG.Graph;

/// <summary>
/// Recursive first search aims to mimic the operation of A* search, but
/// using linear memory.
/// </summary>
public class RecursiveBestFirstSearch
{
    public RecursiveBestFirstSearch(List<List<int[]>> graph, int src, int dst, int[] heuristic)
    {
        this.graph = graph;
        this.src = src;
        this.dst = dst;
        this.heuristic = heuristic;
    }

    public void SetProblem(List<List<int[]>> graph, int src, int dst, int[] heuristic)
    {
        this.graph = graph;
        this.src = src;
        this.dst = dst;
        this.heuristic = heuristic;
        this.target = null;
    }

    public int ComputeDistance()
    {
        var root = new Node(src, null, 0);
        (var resultNode, var resultScore) = search(root, int.MaxValue);

        if (resultNode is null)
            return int.MaxValue;

        Debug.Assert(resultNode.cost == resultScore);
        target = resultNode;
        return resultScore;
    }

    public void PrintRoute()
    {
        if (target is null)
        {
            Console.WriteLine($"path from {src} to {dst} not exist");
            return;
        }

        var path = target!.Path;
        Console.Write($"Path:({src} -> {dst}):");
        foreach (var u in path)
        {
            Console.Write($"{u} ");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// recursive search
    /// </summary>
    /// <param name="node">node to expand</param>
    /// <param name="flimit">stop search when successor greater than limit</param>
    /// <returns>
    ///     Node: when not null the target node is found
    /// </returns>
    private (Node?, int) search(Node node, int flimit)
    {
        // found the target
        if (node.u == dst)
        {
            return (node, node.cost);
        }

        // expand the node to search
        var successor = node.Children(graph);

        // when node is a dead end return infinity
        // current search failed
        if (successor.Count == 0) return (null, int.MaxValue);

        // estimate of distance from src to dst when passing
        // the successor
        foreach (var next in successor)
        {
            next.score = Math.Max(node.score, next.cost + heuristic[next.u]);
        }


        // define a function find largest and second largest
        var getMinAndSecond = () =>
        {
            int minScore = int.MaxValue;
            int secScore = int.MaxValue;
            Node? minNode = null;
            Node? secNode = null;

            foreach (Node child in successor)
            {
                if (child.score < minScore)
                {
                    secScore = minScore;
                    secNode = minNode;
                    minScore = child.score;
                    minNode = child;

                }
                else if (child.score < secScore)
                {
                    secScore = child.score;
                    secNode = child;
                }
            }

            return (minNode, secNode);
        };

        // in each iteration, we choose the node with min score to expand
        // until the flimit hit or result found
        while (true)
        {
            (var child, var alternative) = getMinAndSecond();
            int childScore = child!.score;
            int altScore = alternative is null ? int.MaxValue : alternative.score;

            // we shall not expand current node since flimit hit
            if (childScore > flimit)
            {
                return (null, childScore);
            }

            // search child to update score
            (var candidate, int scoreRefine) = search(child, Math.Min(flimit, altScore));
            child.score = scoreRefine;

            if (candidate is not null)
                return (candidate, scoreRefine);
        }
    }

    // data segement
    private List<List<int[]>> graph;
    private int src;
    private int dst;
    private int[] heuristic;
    private Node? target;

    // Node for RBFS
    class Node
    {
        public Node(int u, Node? parent, int cost)
        {
            this.u = u;
            this.parent = parent;
            this.cost = cost;
        }

        public List<Node> Children(List<List<int[]>> graph)
        {
            List<Node> children = new();
            foreach (var edge in graph[u])
            {
                int v = edge[0], w = edge[1];
                var child = new Node(v, this, cost + w);
                children.Add(child);
            }
            return children;
        }

        public List<int> Path
        {
            get
            {
                List<int> path = new();

                Node? curr = this;
                while (curr is not null)
                {
                    path.Add(curr.u);
                    curr = curr.parent;
                }

                path.Reverse();
                return path;
            }
        }

        public int u; // graph node id
        public Node? parent; // u is add to the search path from parent
        public int cost; // cost to u from root
        public int score;
    }
}