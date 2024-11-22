using CsharpALG.Numerical;
namespace CsharpALG.Optimization;
public class GradientDescent: IGradientBasedMinimizer
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

    // public void Minimize(
    //     Func<NdArray<double>[], double> computeloss,
    //     Func<NdArray<double>[], NdArray<double>[]> computeGrad,
    //     int maxIter,
    //     double tol = 1e-4
    // )
    // {
    //     for (int i = 0; i < maxIter; ++i)
    //     {
    //         double gradNorm = 0.0;
    //         var grads = computeGrad(x);
    //         for (int j = 0; j < x.Length; ++j)
    //         {
    //             x[j].Grad = grads[j];

    //             double gradNormCurr = grads[j].L2Norm();
    //             gradNormCurr *= gradNormCurr;
    //             gradNorm += gradNormCurr;
    //         }

    //         gradNorm = Math.Sqrt(gradNorm);

    //         Step();

    //         double loss = computeloss(x);
    //         Console.WriteLine($"epoch = {numStep}, loss = {loss}, gradNorm = {gradNorm}");

    //         if (gradNorm < tol) break;
    //     }
    // }

    public NdArray<double>[] Params {get => x;}
    private NdArray<double>[] x;
    private double eta; // learning rate
    private int numStep; // called step method numStep times
    private delegate double LRScheduler(int time);
    LRScheduler lrScale;
}