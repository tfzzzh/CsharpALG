
using CommunityToolkit.HighPerformance;

namespace CSharpALG.MachineLearning;

public class NNBruteForceSearch
{
    public NNBruteForceSearch(double[,] X, DistFunc distanceFunc)
    {
        this.X = X;
        this.distanceFunc = distanceFunc;
        size = X.GetLength(0);
    }

    /// <summary>
    /// given a query find its knearest neighbors
    /// </summary>
    /// <param name="query"></param>
    /// <param name="k"></param>
    /// <returns>a list of k (index, distance) tuples sorted by distance where index points to the data point X[index]</returns>
    /// <exception cref="InvalidDataException"></exception>
    public List<(int, double)> kNearestNeighbor(double[] query, int k)
    {
        // compute distances of query to all other points
        ReadOnlySpan2D<double> Xptr = X;
        double[] distances = new double[size];
        int[] indices = new int[size];
        for (int i=0; i < size; ++i)
        {
            distances[i] = distanceFunc(Xptr.GetRowSpan(i), query);
            indices[i] = i;
        }

        Array.Sort(distances, indices);

        List<(int, double)> result = new();
        for (int i=0; i < Math.Min(size, k); ++i)
        {
            result.Add((indices[i], distances[i]));
        }

        return result;
    }

    private double[,] X; // pointer to matrix X
    private int size;
    private DistFunc distanceFunc;
    public delegate double DistFunc(ReadOnlySpan<double> va , ReadOnlySpan<double> vb);
}