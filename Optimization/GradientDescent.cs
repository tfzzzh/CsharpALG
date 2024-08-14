using CsharpALG.Numerical;
namespace CsharpALG.Optimization;
public class GradientDescent
{
    public GradientDescent(NdArray<double>[] x, double learningRate, string lrSchedule = "Constant")
    {
        this.x = x;

        eta = learningRate;
        numStep = 0;

        if (lrSchedule == "Constant")
            lrScale = (int time) => 1.0;

        else if (lrSchedule == "PolyLR")
            lrScale = (int time) => PowScale(time, 0.5);

        else
            throw new NotImplementedException($"Only [Constant, PolyLR] schedulers are supported");
    }

    private static double PowScale(int time, double alpha = 0.5)
    {
        return Math.Pow(1.0 / time, alpha);
    }

    public void Step()
    {
        numStep += 1;
        double lr = eta * lrScale(numStep);

        foreach (var arr in x)
        {
            for (long i = 0; i < arr.Length; ++i)
            {
                if (arr.Grad is null)
                    throw new Exception("Gradient of x is not computed");

                arr.Data[i] -= lr * arr.Grad.Data[i];
            }
        }
    }

    public void Minimize(
        Func<NdArray<double>[], double> computeloss,
        Func<NdArray<double>[], NdArray<double>[]> computeGrad,
        int maxIter,
        double tol = 1e-4
    )
    {
        for (int i = 0; i < maxIter; ++i)
        {
            double gradNorm = 0.0;
            var grads = computeGrad(x);
            for (int j = 0; j < x.Length; ++j)
            {
                x[j].Grad = grads[j];

                double gradNormCurr = grads[j].L2Norm();
                gradNormCurr *= gradNormCurr;
                gradNorm += gradNormCurr;
            }

            gradNorm = Math.Sqrt(gradNorm);

            Step();

            double loss = computeloss(x);
            Console.WriteLine($"epoch = {numStep}, loss = {loss}, gradNorm = {gradNorm}");

            if (gradNorm < tol) break;
        }
    }

    private NdArray<double>[] x;
    private double eta; // learning rate
    private int numStep; // called step method numStep times
    private delegate double LRScheduler(int time);
    LRScheduler lrScale;
}


static class ExampleGradientDescent
{

    public static void Run()
    {
        int n = 5, d = 10;
        var xs = new NdArray<double>[n];
        for (int i=0; i < n; ++i)
        {
            double[] arr = (double[]) LinearAlg.RandomUniform(-5.0, 5.0, [d]);
            xs[i] = new NdArray<double>(arr, [d]);
        }

        var opt = new GradientDescent(xs, 2e-3);
        opt.Minimize(computeloss, computeGrad, 100);

        Console.WriteLine($"loss = {computeloss(xs)}, minimizer:");
        foreach(var arr in xs)
        {
            Console.WriteLine(arr.ToString());
        }
    }

    // test for function
    // sum_{i=1}^{10} (x_i^2 - 10 cos(2pi x_i))
    static double computeloss(NdArray<double>[] x)
    {
        double loss = 0.0;
        for (int i=0; i < x.Length; ++i)
        {
            for (int j=0; j < x[i].Length; ++j)
                loss += x[i][j] * x[i][j] - 10 * Math.Cos(2 * Math.PI * x[i][j]);
        }
        return loss;
    }


    static NdArray<double>[] computeGrad(NdArray<double>[] x)
    {
        NdArray<double>[] grads = new NdArray<double>[x.Length];
        for (int i=0; i < x.Length; ++i)
            grads[i] = new NdArray<double>(x[i].Shape);

        // grad = 2 x + 10 * 2p * sin(2pi x)
        for (int i=0; i < x.Length; ++i) {
            for (int j=0; j < grads[i].Length; ++j)
            {
                grads[i].Data[j] = 2 * x[i].Data[j] +
                    10 * 2 * Math.PI * Math.Sin(2*Math.PI* x[i].Data[j]);
            }
        }

        return grads;
    }
}