namespace CsharpALG.Numerical;

public static class LinearAlg
{
    /// <summary>
    /// Decompose matrix A into PAQ^T = LU where P, Q are permutation matrix
    /// which select pivots for the decompostion
    ///
    /// reference: Matrix Computation 4th Edition
    /// </summary>
    /// <param name="A">When the function return, the lower triangle part of A stores L
    ///     the upper triangle part of A stores U </param>
    /// <param name="rowPiv">during pivot, row i has been swapped with rowPiv[i]</param>
    /// <param name="colPiv">during pivot, col j has been swapped with colPiv[j]</param>
    /// <param name="tol">when pivot < tol the matrix will be treated as rank deficient </param>
    /// <returns>rank of matrix A</returns>
    public static int LUDecompositionFullPivot(
        double[,] A,
        out int[] rowPiv,
        out int[] colPiv,
        double tol = 1e-10
    )
    {
        // check A shall be square matrix with non-zero dimensions
        if (A.GetLength(0) != A.GetLength(1))
            throw new InvalidDataException("A shall be a square matrix");
        if (A.GetLength(0) == 0)
            throw new InvalidDataException("A shall not be an empty matrix");

        // init rowPiv and colPiv with -1
        int n = A.GetLength(0);
        rowPiv = new int[n];
        Array.Fill(rowPiv, -1);

        colPiv = new int[n];
        Array.Fill(colPiv, -1);

        for (int i = 0; i < n; ++i)
        {
            // find max |value| in A[i:,i:]
            double pivotValue = -1.0;
            int pivotRow = -1, pivotCol = -1;
            for (int k1 = i; k1 < n; ++k1)
            {
                for (int k2 = i; k2 < n; ++k2)
                    if (pivotValue < Math.Abs(A[k1, k2]))
                    {
                        pivotValue = Math.Abs(A[k1, k2]);
                        pivotRow = k1;
                        pivotCol = k2;
                    }
            }

            // handle rank deficiency
            if (pivotValue < tol) return i;

            // store pivot position
            rowPiv[i] = pivotRow;
            colPiv[i] = pivotCol;

            // swap A[pivotRow, pivotCol] to position (i, i) via
            // swap A[i, :] with A[pivotRow,:] and A[:,i] with A[:,pivotCol]
            for (int j = 0; j < n; ++j)
                (A[i, j], A[pivotRow, j]) = (A[pivotRow, j], A[i, j]);

            for (int k = 0; k < n; ++k)
                (A[k, i], A[k, pivotCol]) = (A[k, pivotCol], A[k, i]);

            // update A[i+1:n, i]
            for (int k = i + 1; k < n; ++k)
                A[k, i] = A[k, i] / A[i, i];

            // update A[i+1:n, i+1:n]
            // A[i+1:n, i+1:n] -= A[i+1:n, i] * A[i, i+1:n]
            for (int k1 = i + 1; k1 < n; ++k1)
                for (int k2 = i + 1; k2 < n; ++k2)
                    A[k1, k2] -= A[k1, i] * A[i, k2];
        }

        return n;
    }

    /// <summary>
    /// Matrix multplication
    /// </summary>
    /// <param name="A">matrix of shape (m, n)</param>
    /// <param name="B">matrix of shape (n, q)</param>
    /// <returns>matrix C of shape (m, q)</returns>
    public static double[,] MatMul(double[,] A, double[,] B)
    {
        int m = A.GetLength(0), n = A.GetLength(1), p = B.GetLength(0), q = B.GetLength(1);
        // check if one dimension of A, B being empty
        if (m == 0 || n == 0 || p == 0 || q == 0)
            throw new InvalidDataException("both input matrix A and B shall not empty");

        // check if dimemsion of A, B compatible
        if (n != p)
            throw new InvalidDataException($"matrix A of shape [{m}, {n}] is not compatible with matrix B of shape [{p}, {q}]");

        double[,] result = new double[m, q];
        for (int i=0; i < m; ++i)
            for (int j=0; j < q; ++j)
                result[i, j] = 0.0;

        for (int k=0; k < n; ++k)
            for (int i=0; i < m; ++i)
                for (int j=0; j < q; ++j)
                    result[i, j] += A[i, k] * B[k, j];

        return result;
    }

    public static double[,] AppendColumn(double[,] A, double x)
    {
        int m = A.GetLength(0), n = A.GetLength(1);

        double[,] X = new double[m, n+1];
        for (int i=0; i < m; ++i)
        {
            for (int j=0; j < n; ++j)
            {
                X[i, j] = A[i, j];
            }

            X[i, n] = x;
        }

        return X;
    }

    public static double[] MatMul(double[,] A, double[] b)
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

    public static double[,] MatMinus(double[,] A, double[,] B)
    {
        int m = A.GetLength(0), n = A.GetLength(1);
        if (m != B.GetLength(0) || n != B.GetLength(1))
            throw new InvalidDataException("matrix shape of A, B is not the same");

        double[,] C = new double[m, n];
        for (int i=0; i < m; ++i)
            for (int j=0; j < n; ++j)
                C[i, j] = A[i, j] - B[i, j];

        return C;
    }

    public static double[] Minus(double[] a, double[] b)
    {
        int m = a.Length;
        if (m != b.Length)
            throw new InvalidDataException("length of vector a and b are not the same");

        double[] c = new double[m];
        for (int i=0; i < m; ++i)
            c[i] = a[i] - b[i];
        return c;
    }

    public static double L2Norm(double[, ] A)
    {
        int m = A.GetLength(0), n = A.GetLength(1);
        if (m == 0 || n == 0) return 0.0;

        double result = 0.0;
        for (int i=0; i < m; ++i)
            for (int j=0; j < n; ++j)
                result += A[i, j] * A[i, j];

        return Math.Sqrt(result);
    }

    public static double L2Norm(double[] a)
    {
        int m = a.Length;
        if (m == 0) return 0.0;

        double result = 0.0;
        for (int i=0; i < m; ++i)
            result += a[i] * a[i];

        return Math.Sqrt(result);
    }

    /// <summary>
    /// Solve linear equation A x = b via LU decomposition
    /// </summary>
    /// <param name="A"></param>
    /// <param name="b"></param>
    /// <param name="tol"></param>
    /// <returns></returns>
    public static double[] Solve(double[,] A, double[] b, double tol=1e-10)
    {
        // check size of the input
        int n = A.GetLength(0);
        if (n != A.GetLength(0))
            throw new InvalidDataException("matrix A is not square");
        if (n != b.Length)
            throw new InvalidDataException(
                $"Shape of A ({n},{n}) and length of b ({b.Length}) is not compatible"
            );
        if (n == 0)
            throw new InvalidDataException("empty matrix is not supported");

        // allocate array to store LU decompsition of A
        double[,] LU = (double[,]) A.Clone();
        int[] rowPivot;
        int[] colPivot;
        int rank = LUDecompositionFullPivot(LU, out rowPivot, out colPivot, tol);

        if (rank < n)
            throw new InvalidDataException($"Matrix A with size ({n},{n}) is rank deficient with rank {rank}");

        // now PAQ^T = L * U where P, Q encodes row and col pivots.
        // The equation A x = b <=> P A Q^T (Qx) = Pb
        //                      <=> L * U (Qx) = Pb
        // Thus one can solve it via
        // 1. solve Ly = Pb
        // 2. solve Uz = y
        // 3. x = Q^T y
        // ## construct Pb
        double[] pb = (double[]) b.Clone();
        for (int i=0; i < n; ++i)
        {
            (pb[i], pb[rowPivot[i]]) = (pb[rowPivot[i]], pb[i]);
        }

        // ## solve Ly = Pb
        for (int i=1; i < n; ++i)
        {
            for (int j=0; j < i; ++j)
                pb[i] -= LU[i, j] * pb[j];
        }

        // ## solve Uz = y
        for (int i=n-1; i >= 0; --i)
        {
            for (int j=i+1; j < n; ++j)
                pb[i] -= LU[i, j] * pb[j];
            pb[i] = pb[i] / LU[i, i];
        }

        // ## x = Q^T y
        for (int i=n-1; i >=0; --i)
        {
            (pb[i], pb[colPivot[i]]) = (pb[colPivot[i]], pb[i]);
        }

        return pb;
    }

    /// <summary>
    /// Solve linear regression
    ///     min_{x} \| Ax - b \|^2 + \lambda \|x\|^2
    ///
    /// Take gradient and set it to 0, one get the equation:
    ///     A^T(Ax - b) + \lambda x = 0
    ///     (A^T * A + \lambda I) x = A^T b
    ///
    /// </summary>
    /// <param name="A"></param>
    /// <param name="b"></param>
    /// <param name="lambda"></param>
    /// <returns></returns>
    static public double[] LinearRegress(double[,] A, double[] b, double lambda)
    {
        // compute (A^T * A + \lambda I)
        int n = A.GetLength(0), p = A.GetLength(1);
        double[,] C = new double[p, p];
        for (int i=0; i < p; ++i) C[i, i] = lambda;

        for (int k=0; k < n; ++k)
        {
            for (int i=0; i < p; ++i)
            {
                for (int j=0; j < p; ++j)
                {
                    C[i, j] += A[k,i] * A[k, j];
                }
            }
        }

        // compute A^T b
        double[] y = new double[p];
        for (int i=0; i < n; ++i)
        {
            for (int j=0; j < p; ++j)
            {
                y[j] += A[i, j] * b[i];
            }
        }

        double[] x = Solve(C, y);

        return x;
    }

    static public Array RandomUniform(double minValue, double maxValue, int[] shape)
    {
        long length = 1;
        foreach(int dim in shape)
            length *= dim;

        Array array = Array.CreateInstance(typeof(double), shape);
        var index = new List<int>();
        _randomUniform(array, 0, minValue, maxValue, shape, index);

        return array;
    }

    static void _randomUniform(
        Array arr, int i, double minValue, double maxValue, int[] shape,
        List<int> index
    )
    {
        var rand = new Random();
        if (i >= shape.Length)
        {
            arr.SetValue(
                rand.NextDouble() * (maxValue - minValue) + minValue,
                index.ToArray()
            );
            return;
        }

        for (int idx = 0; idx < shape[i]; ++idx)
        {
            index.Add(idx);
            _randomUniform(arr, i+1, minValue, maxValue, shape, index);
            index.RemoveAt(index.Count - 1);
        }
    }
}