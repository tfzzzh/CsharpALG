namespace CsharpALG.Numerical;

public static class NdArrayExtension
{
    public static double L2Norm(this NdArray<double> arr)
    {
        return LinearAlg.L2Norm(arr.Data);
    }
}