namespace CsharpALG.Numerical.ArrayExtension;

public static class MatrixExtension
{
    public static double[] MatMul(this double[,] A, double[] b)
    {
        int m = A.GetLength(0), n = A.GetLength(1), p = b.Length;
        // check if one dimension of A, B being empty
        if (m == 0 || n == 0 || p == 0)
            throw new InvalidDataException("both input matrix A and vector b shall not be empty");

        // check if dimemsion of A, B compatible
        if (n != p)
            throw new InvalidDataException($"matrix A of shape [{m}, {n}] can't multiply with b of shape [{p}]");

        double[] c = new double[m];
        for (int i=0; i < m; ++i)
        {
            c[i] = 0.0;
            for (int j=0; j < n; ++j)
                c[i] += A[i,j] * b[j];
        }

        return c;
    }
}