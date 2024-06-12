using System.Diagnostics;

namespace CsharpALG.Numerical;

/// <summary>
/// B-spline is a spline function that has minimal support with respect to given
/// degree, smoothness and domain partition.
///
/// references: https://en.wikipedia.org/wiki/B-spline
/// </summary>
public class BSpline
{
    public BSpline(double[] knots)
    {
        if (knots.Length < 2)
            throw new InvalidDataException("knots shall have length >= 2");

        // check if knots are sorted in increasing order
        for (int i=0; i < knots.Length - 1; ++i)
        {
            if (knots[i] >= knots[i+1])
                throw new InvalidDataException("knots shall be monotonically increasing");
        }

        this.knots = knots;
    }

    public int NumKnots
    {
        get => knots.Length;
    }

    /// <summary>
    /// evaluate BSpline of order at parameter x
    ///     B_{i, p}(x) = (t-t_i) / (t_{i+p} - t_i) B_{i, p-1}(x) +
    ///         (t_{i+p+1} - x) / (t_{i+p+1} - t_{i+1}) B_{i+1, p-1}(x)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public double[] Eval(double x, int order)
    {
        // check order
        if (order > OrderMax)
            throw new InvalidDataException($"order has value > {OrderMax}");

        // check range of x
        if (x < knots[0] || x > knots[^1])
        {
            throw new InvalidDataException($" x:{x} is out of range [{knots[0]}, {knots[^1]}]");
        }

        // evaluate B_{:, 0}
        // B_{i, 0}(x) = 1 if t_i <= x < t_{i+1} else 0
        double[] spine = new double[NumKnots-1];
        int spline_len = NumKnots - 1;
        for (int i=0; i < spline_len; ++i)
        {
            if (knots[i] <= x && x < knots[i+1])
            {
                spine[i] = 1.0;
            }
            else
            {
                spine[i] = 0.0;
            }
        }

        // allocate memory for dynamic iteration
        double[] splineNext = new double[NumKnots-1];
        int splineNextLen;

        ///     B_{i, p}(x) = (t-t_i) / (t_{i+p} - t_i) B_{i, p-1}(x) +
        ///         (t_{i+p+1} - t_{i+1}) B_{i+1, p-1}(x)
        for (int p=1; p <= order; ++p)
        {
            splineNextLen = spline_len - 1;
            Debug.Assert(splineNextLen > 0);

            for (int i=0; i < splineNextLen; ++i)
            {
                splineNext[i] = (x-knots[i]) / (knots[i+p] - knots[i]) * spine[i] +
                    (knots[i+p+1] - x) / (knots[i+p+1] - knots[i+1]) * spine[i+1];
            }

            // update spline
            (spine, splineNext) = (splineNext, spine);
            spline_len = splineNextLen;
        }

        return spine[0..spline_len];
    }

    public double[,] Transfrom(double[] x, int order)
    {
        int n = x.Length, dim = knots.Length - 1 - order;

        if (dim <= 0)
            throw new InvalidDataException($"order = {order} greater than OrderMax={OrderMax}");

        double[,] ys = new double[n, dim];
        for (int i=0; i < n; ++i)
        {
            double[] y = Eval(x[i], order);
            Debug.Assert(y.Length == dim);

            for (int j=0; j < dim; ++j)
            {
                ys[i, j] = y[j];
            }
        }

        return ys;
    }

    // spline of order p resides in knots
    // t_i to t_{i+p+1} thus
    // p_max + 1 < knots.Length
    public int OrderMax
    {
        get => knots.Length - 2;
    }

    // knots
    private double[] knots;
}


public class BSplineRegression
{
    public BSplineRegression(double[] knots, int order, bool useBias = false, double lambda=0.0)
    {
        Array.Sort(knots);
        splines = new BSpline(knots);
        xMin = knots[0]; xMax = knots[^1];
        this.order = order;
        this.lambda = lambda;
        this.useBias = useBias;
    }

    public void Fit(double[] x, double[] y)
    {
        var feature = splines.Transfrom(x, order);

        if (useBias)
        {
            feature = LinearAlg.AppendColumn(feature, 1.0);
        }

        weight = LinearAlg.LinearRegress(feature, y, lambda);
    }

    public double[] Predict(double[] x)
    {
        if (weight is null) throw new Exception("model is not trained");
        double[,] feature = splines.Transfrom(x, order);

        if (useBias)
        {
            feature = LinearAlg.AppendColumn(feature, 1.0);
        }
        
        return LinearAlg.MatMul(feature, weight);
    }

    private BSpline splines;
    private double xMin;
    private double xMax;
    private int order;
    private double[]? weight;
    private double lambda;
    private bool useBias;
}