// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using CsharpALG.Graph;
test_topological_sort();
test_strongly_connected_components();

void test_topological_sort()
{
    // linear graph 0->1->2->3
    int[,] edges0 = new int[3, 2] { { 0, 1 }, { 1, 2 }, { 2, 3 } };
    var alg0 = new TopologicalSort(4, edges0);
    List<int> order;
    Debug.Assert(alg0.SortViaDFS(out order));
    Console.WriteLine("order of a linear graph (dfs sort):");
    foreach (int u in order)
        Console.Write($"{u} ");
    Console.WriteLine();
    Debug.Assert(alg0.SortViaBFS(out order));
    Console.WriteLine("order of a linear graph (bfs sort):");
    foreach (int u in order)
        Console.Write($"{u} ");
    Console.WriteLine();

    // graph with loop
    // 0->1->2->3->4->0
    int[,] edges1 = new int[5, 2] { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 0 } };
    var alg1 = new TopologicalSort(5, edges1);
    Debug.Assert(!alg1.SortViaDFS(out order));
    Debug.Assert(!alg1.SortViaBFS(out order));


    // complex case
    int[,] edges2 = new int[10, 2]{
        {0, 1}, {0, 2}, {3, 2}, {1, 2},
        {1, 5}, {4, 5}, {4, 6}, {6, 7},
        {5, 7}, {9, 10}
    };
    var alg2 = new TopologicalSort(11, edges2);
    Debug.Assert(alg2.SortViaDFS(out order));
    Console.WriteLine("order of a  dag (dfs sort):");
    foreach (int u in order)
        Console.Write($"{u} ");
    Console.WriteLine();
    Debug.Assert(alg2.SortViaBFS(out order));
    Console.WriteLine("order of a dag (bfs sort):");
    foreach (int u in order)
        Console.Write($"{u} ");
    Console.WriteLine();
}

void test_strongly_connected_components()
{
    // case: tree
    List<List<int>> graph = new List<List<int>>() {
        new List<int>(){1, 2},
        new List<int>(){3, 4},
        new List<int>(){5, 6, 7},
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>()
    };

    _test_strongly_connected_components(graph, 8);

    // case one loop
    graph = new List<List<int>>() {
        new List<int>(){1},
        new List<int>(){2},
        new List<int>(){3},
        new List<int>(){4},
        new List<int>(){0}
    };

    _test_strongly_connected_components(graph, 1);

    // multiple components
    graph = new List<List<int>>() {
        new List<int>(){1,2},
        new List<int>(){2},
        new List<int>(){3},
        new List<int>(){0},
        new List<int>(){5,6},
        new List<int>(){6,7},
        new List<int>(){},
        new List<int>(){4},
        new List<int>(){}
    };

    _test_strongly_connected_components(graph, 4);
}

void _test_strongly_connected_components(List<List<int>> graph, int trueComponent, bool echo = true)
{
    var alg = new StronglyConnectedComponent(graph);
    Debug.Assert(alg.GetNumComponents() == trueComponent);

    if (echo)
    {
        var components = alg.ListComponents();
        Console.WriteLine("components of the graph are:");

        for (int i = 0; i < components.Count; ++i)
        {
            Console.Write($"[{i}]: ");
            for (int j = 0; j < components[i].Count; ++j)
            {
                Console.Write($"{components[i][j]} ");
            }
            Console.WriteLine();
        }
    }
}