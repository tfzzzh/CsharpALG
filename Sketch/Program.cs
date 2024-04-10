// See https://aka.ms/new-console-template for more information
using CsharpALG.Sketch;


test_hyperloglog_sequantial_list(400, 17);
void test_hyperloglog_sequantial_list(int m, int n)
{
    Console.WriteLine($"count {m} distinct number by hyperloglog");
    var random = new Random();
    // int len = 2*m+1;
    long[] arr = new long[m*n];
    for (int i=0; i < n; ++i)
    {
        for (int j=i*m; j < (i+1)*m; ++j)
            arr[j] = j-i*m;
    }
    random.Shuffle(arr);

    var counter = new HyperLogLog(12);
    foreach(var elem in arr)
    {
        counter.Insert(elem);
    }

    Console.WriteLine($"number of distinct elements via raw estimator : {counter.CountDistinct()}");
    Console.WriteLine($"number of distinct elements via ertl estimator: {counter.CountDistinct("ertl")}");
}
