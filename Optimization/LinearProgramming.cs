using System.Diagnostics;
using CsharpALG.Numerical;

namespace CsharpALG.Optimization;

/// <summary>
/// This algorithm use simplex method to solve standard linear programming.
/// The optimization problem is assumed to have the following form
///
/// \max_{x} c^T x
///     s.b.t x >=0  Ax <= b
///
/// where x and b are vector of dimension n, c is a vector of dimension m
/// A is a matrix of shape (m, n)
/// </summary>
public class LinearProgramming
{
    public LinearProgramming(double[,] A, double[] b, double[] c)
    {
        // TODO: check A's shape and b, c are compatible, non-empty
        numConstraint = A.GetLength(0);
        numVariable = A.GetLength(1);

        // book mark inputs
        (AOrigin, bOrigin, cOrigin) = (A, b, c);

        // allocate memories with reserved slots for dummy variable x[m+n]
        nonBasic = new int[numVariable + 1];
        basic = new int[numConstraint];
        this.A = new double[numConstraint, numVariable + 1];
        this.b = new double[numConstraint];
        this.c = new double[numVariable + 1];

    }

    // solution states
    public enum ResultCode
    {
        SUCCESS,
        UNBOUNDED,
        INFEASIBLE
    }

    public class Result
    {
        public ResultCode Code;
        public double[]? primal; // solution of this LP
        public double[]? dual; // solution of dual LP
        public double primeGain;
        public double dualCost;
    }

    public Result Optimize(bool echo = true)
    {
        bool success = initialize();
        if (!success)
        {
            return new Result { Code = ResultCode.INFEASIBLE };
        }

        bool isFinish = false, isUnbounded = false;
        int iter = 0;
        while (!isFinish)
        {
            step(numConstraint, numVariable, out isUnbounded, out isFinish);
            iter += 1;

            if (echo)
            {
                if (!isUnbounded)
                    Console.WriteLine($"iter={iter}, gain = {gain}");
            }
        }

        if (isUnbounded)
        {
            return new Result { Code = ResultCode.UNBOUNDED };
        }

        double[] xPrime = new double[numVariable];
        double[] yDual = new double[numConstraint];

        for (int i = 0; i < numConstraint; ++i)
        {
            int idx = basic[i];
            if (idx < numVariable)
            {
                xPrime[idx] = b[i];
            }
        }

        // dual cost: \sum b_i y_i
        double dualCost = 0.0;
        for (int i = 0; i < numVariable; ++i)
        {
            int idx = nonBasic[i];
            if (idx >= numVariable)
            {
                int dual_idx = idx - numVariable;
                yDual[dual_idx] = -c[i];
                dualCost += yDual[dual_idx] * bOrigin[dual_idx];
            }
        }

        return new Result() { Code = ResultCode.SUCCESS, primal = xPrime, primeGain = gain, dual = yDual, dualCost = dualCost };
    }

    /// <summary>
    /// if the problem is feasible, initialze the solver and return true
    /// otherwise return false to indicate the problem is unfeasible
    /// </summary>
    /// <returns></returns>
    bool initialize()
    {
        // fill nonBasic with 0, 1, ... numVariable-1, numConstraint + numVariable
        // where numConstraint + numVariable is the dummy non-basic variable
        for (int i = 0; i < numVariable; ++i) nonBasic[i] = i;
        nonBasic[numVariable] = numConstraint + numVariable;

        // fill basic varible to numVariable, ... numVariable + numConstraint-1
        for (int i = 0; i < numConstraint; ++i) basic[i] = numVariable + i;

        // init constaint: Xb = b - A XN + x{dummy}
        for (int i = 0; i < numConstraint; ++i)
        {
            for (int j = 0; j < numVariable; ++j)
                A[i, j] = AOrigin[i, j];

            A[i, numVariable] = -1.0;
        }

        // check if bOrigin >= 0 in this case bOrigin is feasible
        bool isBiasFeasible = true;
        for (int i = 0; i < numConstraint && isBiasFeasible; ++i)
            isBiasFeasible = bOrigin[i] >= 0;

        if (isBiasFeasible)
        {
            for (int i = 0; i < numConstraint; ++i)
            {
                b[i] = bOrigin[i];
            }

            for (int i = 0; i < numVariable; ++i)
            {
                c[i] = cOrigin[i];
            }

            gain = 0.0;
            return true;
        }
        // else when bias is not a basic solution due to some of its component < 0
        else
        {
            return findFeasibleSolution();
        }
    }


    /* we construct a feasible solution via solve
        max -x[m+n] s.b.t                               (2)
        x_{basic} = b_{basic} + x[m+n] - A x_{nonbasic};
        x >= 0
    */
    bool findFeasibleSolution()
    {
        // set b[]
        int leaving = -1;
        double minB = double.PositiveInfinity;
        for (int i = 0; i < numConstraint; ++i)
        {
            b[i] = bOrigin[i];
            if (b[i] < minB)
            {
                minB = b[i];
                leaving = i;
            }
        }


        // set c[] to [0, 0, .... -1]
        int dummy = numConstraint + numVariable;
        Array.Fill(c, 0.0);
        c[numVariable] = -1;

        // set gain
        gain = 0.0;

        // swap x[m+n] with some basic variable x[b] to init problem (2)
        // the leaving variable shall have smallest value in bias
        pivot(numConstraint, numVariable+1, leaving, numVariable);

        // solve the optimization poblem (2)
        bool isFinish = false, isUnbounded = false;
        while (!isFinish)
        {
            step(numConstraint, numVariable+1, out isUnbounded, out isFinish);
        }

        // when problem (2) is unbounded or its max gain is not 0.0
        if (isUnbounded || !Utility.IsClose(gain, 0.0, eps, eps)) return false;

        // when x_{m+n} is in basic variable one shall swap it with some nonbasic varialbe without violate
        // the constraint basic >= 0
        int idx_dummy = Array.IndexOf(basic, dummy);
        if (idx_dummy != -1)
        {
            Console.WriteLine("initalize: when initalization finish, dummy variable is in basic set");
            Debug.Assert(Utility.IsClose(b[idx_dummy], 0.0, eps, eps));

            // in row idx_dummy, select column with max A[idx_dummy, col] and swap it out
            int col = -1; double maxA = double.NegativeInfinity;
            for (int j=0; j <= numVariable; ++j)
            {
                if (Math.Abs(A[idx_dummy, j]) > maxA)
                {
                    maxA = Math.Abs(A[idx_dummy, j]);
                    col = j;
                }
            }

            Debug.Assert(!Utility.IsClose(A[idx_dummy, col], 0.0, eps, eps));

            pivot(numConstraint, numVariable+1, idx_dummy, col);
            Debug.Assert(Utility.IsClose(gain, 0.0, eps, eps));
        }

        // remove x_{m+n} from nonbasic variables
        // swap A[:, idx_dummy] with A[:, ^1]
        // swap c[idx_dummy] wity c[^1]
        // swap nonBasic[idx_dummy] with nonBasic[^1]
        // shrink non basic variables by 1
        idx_dummy = Array.IndexOf(nonBasic, dummy);
        Debug.Assert(idx_dummy != -1);
        for (int i=0; i < numConstraint; ++i)
        {
            (A[i, idx_dummy], A[i, numVariable]) = (A[i, numVariable], A[i, idx_dummy]);
        }
        (c[idx_dummy], c[numVariable]) = (c[numVariable], c[idx_dummy]);
        (nonBasic[idx_dummy], nonBasic[numVariable]) = (nonBasic[numVariable], nonBasic[idx_dummy]);

        // recompute c and gain
        /*
        at start the objective is \sum_{i < n} cOrigin[i] * x[i]  after initial pivot, the basic
        and nonbasic variable follows:
            x[basic] = b[basic] - A[i, nonBasic] @ x[nonBasic]
        substitue basic varible in the objective with above eqaution one have

            objective = \sum_{i < n, i \in nonBasic} cOrigin[i] * x[i] +
                \sum_{i < n, i \in basic} cOrigin[i] * x[i]

            = \sum_{i < n, i \in nonBasic} cOrigin[i] * x[i] +
                \sum_{i < n, i \in basic} cOrigin[i] * (b[i] - A[i, nonBasic] @ x[nonBasic])

            = \sum_{i < n, i \in basic} cOrigin[i] * b[i] +
                (gain)
                \sum_{i \in nonBasic} cOrigin[i] * x[i] I{i < n} +
                \sum_{i in nonBasic} (\sum_{k < n, k \in basic} cOrigin[k] * -A[k, i]) * x[i]
        */
        gain = 0.0;
        for (int i=0; i < numConstraint; ++i)
        {
            int k = basic[i];
            if (k < numVariable)
            {
                gain += cOrigin[k] * b[i];
            }
        }
        // Console.WriteLine($"init gain: {gain}");
        // Console.WriteLine($"init x ");
        // for (int i=0; i < numVariable + numConstraint; ++i)
        // {
        //     if (basic.Contains(i))
        //     {
        //         int idx = Array.IndexOf(basic, i);
        //         Console.WriteLine($"{b[idx]},");
        //     }
        //     else
        //     {
        //          Console.WriteLine($"{0.0}");
        //     }
        // }

        //\sum_{i \in nonBasic} ( cOrigin[i] I{i < n} - \sum_{k < n, k \in basic} cOrigin[k] * A[k, i]) x[i]
        for (int j=0; j < numVariable; ++j)
        {
            int k = nonBasic[j];
            if (k >= numVariable)
            {
                c[j] = 0.0;
            }
            else
            {
                c[j] = cOrigin[k];
            }

            for (int i=0; i < numConstraint; ++i)
            {
                k = basic[i];
                if (k < numVariable)
                {
                    c[j] -= A[i, j] * cOrigin[k];
                }
            }
        }

        // Console.WriteLine($"init c ");
        // for (int i=0; i < numVariable + numConstraint; ++i)
        // {
        //     if (basic.Contains(i))
        //     {
        //         Console.WriteLine($"{0.0},");
        //     }
        //     else
        //     {
        //          int idx = Array.IndexOf(nonBasic, i);
        //          Console.WriteLine($"{c[idx]}");
        //     }
        // }

        return true;
    }

    /// <summary>
    /// let e being an entering position, l being a leaving position update the state of LP
    ///
    /// <param name="m">number of row of matrix A (or number of basic variable)</param>
    /// <param name="n">number of col of matrix A (or number of nonbasic variable)</param>
    /// <param name="l">variable x[basic[l]] will leaving the basic set</param>
    /// <param name="e">variable x[nonBasic[e]] will entering the basic set</param>
    /// <summary>
    void pivot(int m, int n, int l, int e)
    {
        // solve: x_l = b_l - \sum_{j\neq e} A_{l, j} x_j - A_{l, e} x_e in terms of x_e
        // check if A{l, e} close to 0.0
        Debug.Assert(l < m && l >= 0);
        Debug.Assert(e < n && e >= 0);
        Debug.Assert(!CsharpALG.Numerical.Utility.IsClose(A[l, e], 0.0, eps, eps));

        double Ale = A[l, e];
        A[l, e] = 1.0;
        b[l] /= Ale;
        for (int j = 0; j < n; ++j)
        {
            A[l, j] /= Ale;
        }

        // substitute x_e of other equation. suppose the above update
        // correspond to equation: x_e = \hat{b}_e - \sum_{j\neql} \hat{A}_{e, j} x_j - hat{A}_{e, l} x_l
        // substitue x_e in the equation:
        // x_i = b_i - \sum_{j\neq e} A_{i, j} x_j - A{i, e} x_e, one get:
        // x_i = (b_i - A{i, e}\hat{b}_e) - \sum_{j\neq e}(A_{i, j} - A{i, e} * \hat{A}_{e, j}) x_j
        //      - (A{i, e} * - hat{A}_{e, l}) x_l
        for (int i = 0; i < m; ++i)
        {
            if (i != l)
            {
                double Aie = A[i, e];
                A[i, e] = 0.0;
                b[i] -= Aie * b[l];
                for (int j = 0; j < n; ++j)
                {
                    A[i, j] -= Aie * A[l, j];
                }
            }
        }

        // update coefficient by substute xe
        // before update the objective function is:
        //  gain + \sum_{j\neq e} c_j x_j + c_e x_e
        // after substitution with
        //  x_e = \hat{b}_e - \sum_{j\neql} \hat{A}_{e, j} x_j - hat{A}_{e, l} x_l
        // it becomes:
        //  (gain + c_e * \hat{b}_e) + \sum_{j\neq e} (c_j - c_e * \hat{A}_{e, j}) x_j
        //      + (-ce * hat{A}_{e, l}) x_l
        double c_e = c[e];
        c[e] = 0.0;
        gain = gain + c_e * b[l];
        for (int j = 0; j < n; ++j)
        {
            c[j] -= c_e * A[l, j];
        }

        // swap indices of basic and nonbasic variable
        (basic[l], nonBasic[e]) = (nonBasic[e], basic[l]);
    }

    // Perform one step of simplex iteration. the gain should increase
    // without violate the constraint
    // m: number of rows of A, n number of cols of A
    void step(int m, int n, out bool isUnbounded, out bool isFinish)
    {
        // find idx s.t. c[idx] is the max value in c
        int idx = 0;
        for (int i = 0; i < n; ++i)
            if (c[i] > c[idx])
                idx = i;

        // all coefficent can be treated as negative loop finish, result found
        if (c[idx] <= eps)
        {
            isUnbounded = false;
            isFinish = true;
            return;
        }

        // c[idx] > 0
        // find the leaving variable such that b[l] /= A[l, idx] is minimized
        int l = -1; double margin = double.PositiveInfinity;
        for (int i = 0; i < m; ++i)
        {
            if (A[i, idx] > eps)
            {
                double marginCurr = b[i] / A[i, idx];
                if (marginCurr < margin)
                {
                    margin = marginCurr;
                    l = i;
                }
            }
        }

        // when idx can increase to infinite, the problem is unbounded
        if (double.IsPositiveInfinity(margin))
        {
            isFinish = true;
            isUnbounded = true;
            return;
        }

        // perform pivot
        pivot(m, n, l, idx);

        isFinish = false;
        isUnbounded = false;
    }


    // pointer to original problem
    public double[,] AOrigin;
    public double[] bOrigin;
    public double[] cOrigin;
    public int numConstraint; // number of constrains (or number of basic variables)
    public int numVariable; // number of variable dimension (at initialization a dummy variable x_(m+n) may introduce)

    // stores the state of current linear programming
    // public int m; // number of basis variable (alias of numConstraint)
    // public int n; // number of Non-basis variable (equal to numVariable+1 at initalization )
    public int[] nonBasic; // indices of non-basic variables
    public int[] basic; // indices of basic variables
    public double[,] A; // weight matrix with Basic as row indices and nonBasic as col indices
    public double[] b; // bias with Basic as its indices, when start optimization b >= 0
    public double[] c; // weights of the objective function with nonBasis as its index
    public double gain; // value of the objective function when nonBasic variables are set to 0.0
    const double eps = 1e-10; // we treat x >= -eps as x >= 0
}

public class LPExample
{

    public static void Run(string path)
    {
        int i = 0;
        foreach (var (A, b, c) in ReadTestCases(path))
        {
            Console.WriteLine($"LP run test case {i}");
            run(A, b, c);
            i += 1;
        }
    }

    public static IEnumerable<(double[,], double[], double[])> ReadTestCases(string path)
    {
        var lines = File.ReadLines(path);
        double [,] A;
        double[] b;
        double[] c;

        using (var iter = lines.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                string head = iter.Current;
                var headComponents = head.Split(',');
                Debug.Assert(headComponents.Length == 2);
                int m = int.Parse(headComponents[0]);
                int n = int.Parse(headComponents[1]);

                A = new double[m, n];
                for (int i=0; i < m; ++i)
                {
                    iter.MoveNext();
                    string line = iter.Current;
                    var digits = line.Split(',');
                    Debug.Assert(digits.Length == n);

                    for (int j=0; j < n; ++j)
                    {
                        A[i, j] = double.Parse(digits[j]);
                    }
                }

                {
                    b = new double[m];
                    iter.MoveNext();
                    string line = iter.Current;
                    var digits = line.Split(',');
                    Debug.Assert(digits.Length == m);

                    for (int i=0; i < m; ++i)
                    {
                        b[i] = double.Parse(digits[i]);
                    }
                }

                {
                    c = new double[n];
                    iter.MoveNext();
                    string line = iter.Current;
                    var digits = line.Split(',');
                    Debug.Assert(digits.Length == n);

                    for (int i=0; i < n; ++i)
                    {
                        c[i] = double.Parse(digits[i]);
                    }
                }

                yield return (A, b, c);
            }
        }
    }

    public static void run(double[,] A, double[] b, double[] c)
    {
        var lp = new LinearProgramming(A, b, c);
        var result = lp.Optimize();

        switch (result.Code)
        {
            case LinearProgramming.ResultCode.INFEASIBLE:
                Console.WriteLine("the problem is infeasible");
                break;

            case LinearProgramming.ResultCode.UNBOUNDED:
                Console.WriteLine("the problem is unbounded");
                break;

            case LinearProgramming.ResultCode.SUCCESS:
                Console.WriteLine($"primal gain = {result.primeGain}, dual cost = {result.dualCost}");
                Console.WriteLine($"primal solution equals {Utility.ArrToString(result.primal!)}");
                Console.WriteLine($"dual solution equals {Utility.ArrToString(result.dual!)}");
                break;
        }
    }
}