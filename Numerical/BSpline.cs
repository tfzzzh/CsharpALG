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