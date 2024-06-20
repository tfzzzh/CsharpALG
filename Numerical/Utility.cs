using System.Text;

namespace CsharpALG.Numerical;

public static class Utility
{
    public static IEnumerable<double> linspace(double start, double end, int size)
    {
        if (size < 2)
            throw new InvalidDataException($"size shall >= 2");

        if (size == 2)
        {
            yield return start;
            yield return end;
        }

        double step = end / (size-1) - start / (size-1);
        yield return start;

        for (int i=1; i < size - 1; ++i)
        {
            yield return i * step + start;
        }

        yield return end;
    }

    public static string ArrToString<T>(T[] arr)
    {
        if (arr.Length == 0) return "[]";
        string arrValues = String.Join(',', arr);
        return "[" + arrValues + "]";
    }

    static public bool IsClose(double a, double b, double atol=1e-8, double relTol=1e-8)
    {
        if (Double.IsNaN(a) || Double.IsInfinity(a) || Double.IsNaN(b) || Double.IsInfinity(b))
            return false;

        double diff = Math.Abs(a - b);
        double magtitude = Math.Max(Math.Abs(a), Math.Abs(b));
        if (diff < atol) return true;
        if (magtitude > atol && diff / magtitude < relTol) return true;
        return false;
    }
}