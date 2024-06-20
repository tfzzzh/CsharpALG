namespace CSharpALG.MachineLearning;

static class DataChecker
{
    public static void CheckNonEmpty(Array arr)
    {
        int numDims = arr.Rank;
        for (int i=0; i < numDims; ++i)
        {
            if (arr.GetLength(i) == 0)
            {
                throw new InvalidDataException($"dimension {i} of the {numDims}-dim array is 0");
            }
        }
    }

    public static void CheckXYSameNumEntries(Array X, Array y)
    {
        if (X.GetLength(0) != y.GetLength(0))
        {
            throw new InvalidDataException($"X contains {X.GetLength(0)} entries while y contains {y.GetLength(0)}");
        }
    }

    public static void CheckAllValid(double[,] X)
    {
        int n = X.GetLength(0), d = X.GetLength(1);
        for (int i=0; i < n; ++i)
            for (int j=0; j < d; ++j)
            {
                if (Double.IsNaN(X[i, j]))
                {
                    throw new InvalidDataException($"X[{i},{j}] is NaN");
                }
                else if (Double.IsInfinity(X[i, j]))
                {
                    throw new InvalidDataException($"X[{i},{j}] is Inf");
                }
            }
    }

    public static void CheckClassficationData(double[,] X, int [] y)
    {
        CheckNonEmpty(X);
        CheckNonEmpty(y);
        CheckXYSameNumEntries(X, y);
        CheckAllValid(X);
    }
}