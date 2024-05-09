using System.Diagnostics;
using CsharpALG.MaxFlow;
testMaxFlow();

void testMaxFlow()
{
    // case1
    int[,] capacity = new int[6,6]{
        {16, 13, 0, 0, 0, 0},
        {0, 0, 0, 12, 0, 0},
        {0, 4, 0, 0, 14, 0},
        {0, 0, 9, 0, 0, 20},
        {0, 0, 0, 7, 0, 4},
        {0, 0, 0, 0, 0, 0}
    };

    var alg = new EdmondsKarp(capacity, 0, 5);
    int mf = alg.ComputeMaxFlow();
    Console.WriteLine($"the maxflow computed is {mf}");
    alg.DisplayMaxFlow();

    var alg_rf = new RelabelToFront(capacity, 0, 5);
    int mf_rf = alg_rf.ComputeMaxFlow();
    Debug.Assert(mf_rf == mf);

    // case2
    capacity = new int[6,6]{
        {14, 1, 13, 8, 8, 16},
        {7, 0, 5, 13, 2, 2},
        {8, 3, 19, 8, 8, 12},
        {12, 1, 15, 4, 6, 11},
        {4, 4, 8, 14, 2, 1},
        {5, 10, 17, 18, 0, 17}
    };
    alg = new EdmondsKarp(capacity, 0, 5);
    mf = alg.ComputeMaxFlow();
    Console.WriteLine($"the maxflow computed is {mf}");
    alg.DisplayMaxFlow();

    alg_rf = new RelabelToFront(capacity, 0, 5);
    mf_rf = alg_rf.ComputeMaxFlow();
    Debug.Assert(mf_rf == mf);

}

