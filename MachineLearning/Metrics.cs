using CsharpALG.Numerical;

namespace CsharpALG.MachineLearning;

public static class Metrics
{
    public static double ZeroOneLoss(int[] yTrue, int[] yPredict)
    {
        if (yTrue.Length != yPredict.Length)
            throw new InvalidDataException($"yTrue and yPredict shall have the same length");

        if (yTrue.Length == 0)
            throw new InvalidDataException($"yTrue is empty");

        int notSame = 0;
        for (int i = 0; i < yTrue.Length; ++i)
        {
            if (yTrue[i] != yPredict[i]) notSame += 1;
        }

        return ((double)notSame) / yTrue.Length;
    }

    public static double ZeroOneLoss(NdArray<int> yTrue, NdArray<int> yPredict)
    {
        if (yTrue.Length != yPredict.Length)
            throw new InvalidDataException($"yTrue and yPredict shall have the same length");

        if (yTrue.Length == 0)
            throw new InvalidDataException($"yTrue is empty");

        int notSame = 0;
        for (int i = 0; i < yTrue.Length; ++i)
        {
            if (yTrue[i] != yPredict[i]) notSame += 1;
        }

        return ((double)notSame) / yTrue.Length;
    }
}