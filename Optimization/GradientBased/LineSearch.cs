using CsharpALG.Numerical;
namespace CsharpALG.Optimization;


public interface ILineSearch
{
    /// <summary>
    /// Find the learning rate for a given search direction
    /// </summary>
    /// <param name="x">parameter to optimize</param>
    /// <param name="dx">the search direction</param>
    /// <param name="fx">current loss value</param>
    /// <returns></returns>
    LineSearchResult SearchLearningRate(
        NdArray<double>[] x,
        NdArray<double>[] dx,
        double fx
    );

    /// <summary>
    /// pass the handler for computing loss and gradient to the searcher
    /// </summary>
    /// <param name="Func<NdArray<double>"></param>
    /// <param name="lossfunc"></param>
    /// <param name="backwardfunc"></param>
    void SetLossAndBackward(
            Func<NdArray<double>[], double> lossfunc,
            Action<NdArray<double>[]> backwardfunc
    );

    void SetSearchInterval(double lrMin, double lrMax);

    /// <summary>
    /// y = x + lr * dir
    /// </summary>
    /// <param name="x"></param>
    /// <param name="lr"></param>
    /// <param name="dir"></param>
    /// <param name="y"></param>
    protected static void combine(NdArray<double>[] x, double lr, NdArray<double>[] dir, NdArray<double>[] y)
    {
        int numParam = x.Length;
        for (int i = 0; i < numParam; ++i)
        {
            var di = dir[i].Data;
            var xi = x[i].Data;
            var yi = y[i].Data;

            for (int j = 0; j < xi.Length; ++j)
            {
                yi[j] = xi[j] + lr * di[j];
            }
        }
    }

    protected static double directionalDerivate(NdArray<double>[] x, NdArray<double>[] dir)
    {
        int numParam = x.Length;
        double result = 0.0;
        for (int i = 0; i < numParam; ++i)
        {
            var gi = x[i].Grad!.Data;
            var di = dir[i].Data;

            for (int j = 0; j < gi.Length; ++j)
            {
                result += gi[j] * di[j];
            }
        }
        return result;
    }

    protected static NdArray<double>[] allocateLike(NdArray<double>[] x)
    {
        // initialize y
        NdArray<double>[] y = new NdArray<double>[x.Length];
        for (int i = 0; i < x.Length; ++i)
        {
            y[i] = x[i].Clone();
        }

        return y;
    }
};


// Result Returned by Line Search
public class LineSearchResult
{
    // when failed null is returned
    public double? LearningRate;
    // when failed this field contains the reason for why
    // line search failed
    public string? What;
};


/// <summary>
/// Find a learning rate lr, s.t. f(x + lr * d) <= f(x) + lr * grad \dot d
/// </summary>
public class Armijo : ILineSearch
{
    public Armijo(double lr, double c1, double eps = 1e-8, double rho = 0.8)
    {
        (this.c1, lrMax, lrMin, this.rho, lossfunc) = (c1, lr, eps, rho, null);
    }

    public LineSearchResult SearchLearningRate(
        NdArray<double>[] x,
        NdArray<double>[] dx,
        double fx
    )
    {
        var gradEta = ILineSearch.directionalDerivate(x, dx);
        var guess = lrMax;

        // error: search direction has acute angle with the gradient
        if (gradEta > 0)
        {
            return new LineSearchResult()
            {
                LearningRate = null,
                What = $"search direction is not a descent direction."
            };
        }

        NdArray<double>[] y = ILineSearch.allocateLike(x);

        while (guess >= lrMin)
        {
            // combine
            ILineSearch.combine(x, guess, dx, y);

            // check Armijo condition
            var fNew = lossfunc!(y);
            if (fNew <= fx + c1 * guess * gradEta)
            {
                var result = new LineSearchResult() { LearningRate = guess, What = null };
                return result;
            }

            guess *= rho;
        }

        return new LineSearchResult()
        {
            LearningRate = null,
            What = $"candidate {guess} less than the specified min lr {lrMin}"
        };
    }

    public void SetLossAndBackward(
        Func<NdArray<double>[], double> lossfunc,
        Action<NdArray<double>[]> backwardFunc
    )
    {
        this.lossfunc = lossfunc;
    }

    public void SetSearchInterval(double lrMin, double lrMax)
    {
        (this.lrMin, this.lrMax) = (lrMin, lrMax);
    }

    // parameter for sufficient descent
    double c1;
    // shrinkage parameter
    double rho;
    // Min value of the learning rate
    double lrMin;
    // Max value of the learning rate
    double lrMax;
    // loss function to evalute current parameters;
    Func<NdArray<double>[], double>? lossfunc;
}


public class WeakWolf : ILineSearch
{
    public WeakWolf(double lrMax, double c1, double c2, double lrMin=0.0, double tol=1e-8)
    {
        (this.c1, this.c2, this.lrMin, this.lrMax) = (c1, c2, lrMin, lrMax);
        this.tol = tol;
    }

    public LineSearchResult SearchLearningRate(
        NdArray<double>[] x,
        NdArray<double>[] dx,
        double fx
    )
    {
        // f'(x, d)
        var gradEta = ILineSearch.directionalDerivate(x, dx);

        // error: search direction has acute angle with the gradient
        if (gradEta > 0)
        {
            return new LineSearchResult()
            {
                LearningRate = null,
                What = $"search direction is not a descent direction."
            };
        }

        // initialize y
        var xNew = ILineSearch.allocateLike(x);
        double left = lrMin, right = lrMax;
        while (Math.Abs(left - right) >= tol)
        {
            double mid = left / 2 + right / 2;
            ILineSearch.combine(x, mid, dx, xNew);

            // if sufficent decent condition not satisfied, we
            // shrink the learning rate
            var fNew = lossfunc!(xNew);
            if (fNew > fx + c1 * mid * gradEta)
            {
                right = mid;
            }
            else
            {
                // check the curvature condition
                backwardfunc!(xNew);

                // compute f'(Xnew, d)
                var gradEtaNew = ILineSearch.directionalDerivate(x: xNew, dir: dx);

                // when curvature condition satisfied, one found the result
                if (gradEtaNew >= c2 * gradEta)
                {
                    return new LineSearchResult() { LearningRate = mid, What = null }; ;
                }

                // learnig rate is too small increase it
                left = mid;
            }
        }

        return new LineSearchResult()
        {
            LearningRate = null,
            What = $"WeakWolf failed: search interval [{left}, {right}] has length less than tolerance {tol}"
        };
    }

    public void SetLossAndBackward(
        Func<NdArray<double>[], double> lossfunc,
        Action<NdArray<double>[]> backwardfunc
    )
    {
        this.lossfunc = lossfunc;
        this.backwardfunc = backwardfunc;
    }

    public void SetSearchInterval(double lrMin, double lrMax)
    {
        (this.lrMin, this.lrMax) = (lrMin, lrMax);
    }

    // parameter for sufficient descent
    double c1;
    // parameter for curvature
    double c2;
    // Min value of the learning rate
    double lrMin;
    // Max value of the learning rate
    double lrMax;
    // when search interval [left, right] has length < tol, search failed
    double tol;
    // loss function to evalute current parameters;
    Func<NdArray<double>[], double>? lossfunc;
    // grad function to compute gradient for the parameter
    Action<NdArray<double>[]>? backwardfunc;
}

// Test Line Search
static class ExampleLineSearch
{
    /*
    use g^T (x - x0) + L/2 \|x-x0\|^2 as a test function
    */
    static void run(NdArray<double> g, NdArray<double> x0, double l, ILineSearch searcher) {

        Func<NdArray<double>[], double> lossfunc = (NdArray<double>[] x) => {
            var xarr = x[0].Data; var x0arr = x0.Data; var garr = g.Data;
            var n = xarr.Length;

            double loss = 0.0;
            for (int i=0; i < n; ++i) {
                double diff = xarr[i] - x0arr[i];
                loss += garr[i] * diff + l / 2.0 * diff * diff;
            }

            return loss;
        };

        Action<NdArray<double>[]>? backwardfunc = (NdArray<double>[] x) => {
            x[0].Grad = x[0].Clone();
            var grad = x[0].Grad!.Data;

            var xarr = x[0].Data; var x0arr = x0.Data; var garr = g.Data;
            var n = xarr.Length;
            // grad = g + L (x - x0)
            for (int i=0; i < n; ++i) {
                grad[i] = garr[i] + l * (xarr[i] - x0arr[i]);
            }
        };

        backwardfunc([x0]);
        var dx = -(x0.Grad!); // use -grad as search direction


        searcher.SetLossAndBackward(lossfunc, backwardfunc);
        var result = searcher.SearchLearningRate(x:[x0], dx:[dx], fx:0.0);

        Console.WriteLine($"Line search result: {result.LearningRate} error msg {result.What}");

        if (result.LearningRate.HasValue) {
            var y = x0.Clone();
            for (int i=0; i < y.Data.Length; ++i) {
                y.Data[i] = x0.Data[i] + result.LearningRate.Value * dx.Data[i];
            }

            Console.WriteLine($"descent progress:{lossfunc([x0]) - lossfunc([y])}");
        }
    }

    public static void Run() {
        NdArray<double> g = new NdArray<double>([0.1, 0.2, -0.3], [1, 3]);
        NdArray<double> x = new NdArray<double>([-0.3, 0.4, -1.5], [1, 3]);
        double l = 5.9;
        ILineSearch searcher = new Armijo(2.0, 0.5, rho:0.5);
        run(g, x, l, searcher);
        searcher = new WeakWolf(lrMax:2.0, c1:0.5, c2:0.4);
        run(g, x, l, searcher);
    }
}