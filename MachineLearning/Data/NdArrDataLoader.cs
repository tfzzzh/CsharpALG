using CsharpALG.Numerical;

namespace CsharpALG.MachineLearning.Data;

public class NdArrayTrainData<FeatureType, LabelType>
{
    public NdArrayTrainData(NdArray<FeatureType> feature, NdArray<LabelType> label,
        int batchsize,
        bool shuffle=false
    ){
        x = feature;
        y = label;
        BatchSize = batchsize;

        int n = x.Shape[0];
        this.shuffle = shuffle;
        if (shuffle)
        {
            this.shuffle = true;
            perm = Utility.RandomPerm(n);
            x = x.TakeRow(perm);
            y = y.TakeRow(perm);
        }

        // (NumChunks-1) * batchsize < n <= NumChunks * batchsize
        NumChunks = (int) Math.Ceiling(((double) n) / batchsize);
        NumEntries = n;
    }

    public IEnumerable<(NdArray<FeatureType>, NdArray<LabelType>)> GenBatches()
    {
        for (int i = 0; i < NumChunks; ++i)
        {
            int start = i * BatchSize;
            int end = Math.Min((i+1) * BatchSize, NumEntries);

            var xBatch = x.Slice(start, end);
            var yBatch = y.Slice(start, end);

            yield return (xBatch, yBatch);
        }
    }

    public int BatchSize{get; private set;}
    public int NumChunks{get;}
    public int NumEntries{get;}
    private bool shuffle;
    private int[]? perm;
    // feature and label
    private NdArray<FeatureType> x;
    private NdArray<LabelType> y;
}


static class ExampleUseNdArrayTrainData
{
    public static void Run()
    {
        var feature = NdArray<double>.From(
            new double[6, 2]{
                {1.0, 1.0},
                {2.0, 2.0},
                {3.0, 3.0},
                {4.0, 4.0},
                {5.0, 5.0},
                {6.0, 6.0}
            }
        );

        var label = NdArray<int>.From(
            [1,2,3,4,5,6]
        );

        run(feature, label, true);
        run(feature, label, false);
    }

    static void run(NdArray<double> feature, NdArray<int> label, bool shuffle)
    {
        var loader = new NdArrayTrainData<double, int>(feature, label, 5, shuffle);
        foreach ((var X, var y) in loader.GenBatches())
        {
            Console.WriteLine($"features:\n{X}");
            Console.WriteLine($"labels: \n{y}");
        }
    }
}