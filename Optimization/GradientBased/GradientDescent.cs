using CsharpALG.Numerical;
namespace CsharpALG.Optimization;
public class GradientDescent: IGradientBasedMinimizer
{
    public GradientDescent(NdArray<double>[] x, double learningRate, string lrSchedule = "Constant",
        ILineSearch? ls = null
    )
    {
        (this.x, eta, numStep) = (x, learningRate, 0);

        if (lrSchedule == "Constant")
            lrScale = (int time) => 1.0;

        else if (lrSchedule == "PolyLR")
            lrScale = (int time) => PowScale(time, 0.5);

        else
            throw new NotImplementedException($"Only [Constant, PolyLR] schedulers are supported");

        this.ls = ls;
    }

    private static double PowScale(int time, double alpha = 0.5)
    {
        return Math.Pow(1.0 / time, alpha);
    }

    public void Step()
    {
        numStep += 1;
        double lr = eta * lrScale(numStep);
        updateParam(lr);
    }

    public void Step(Func<NdArray<double>[], double> lossfunc, Action<NdArray<double>[]> backwardfunc)
    {

        numStep += 1;
        double lr = eta * lrScale(numStep);

        if (ls != null)
        {
            lr = searchLearningRate(lossfunc, backwardfunc, lr);
        }

        updateParam(lr);
    }

    private double searchLearningRate(Func<NdArray<double>[], double> lossfunc, Action<NdArray<double>[]> backwardfunc, double lr)
    {
        // set parameter for line search
        var fx = lossfunc(x);
        var dx = x.Select(p => -(p.Grad!)).ToArray();
        ls!.SetSearchInterval(1e-8, lr);
        ls.SetLossAndBackward(lossfunc, backwardfunc);
        var lsResult = ls.SearchLearningRate(x, dx!, fx);

        // when line search failed use the specified lr
        if (!lsResult.LearningRate.HasValue)
        {
            Console.WriteLine($"Line search failed with reason: {lsResult.What}");
        }
        else
        {
            lr = lsResult.LearningRate.Value;
        }

        return lr;
    }

    private void updateParam(double lr)
    {
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

    public NdArray<double>[] Params {get => x;}
    private NdArray<double>[] x;
    private double eta; // learning rate
    private int numStep; // called step method numStep times
    private delegate double LRScheduler(int time);
    LRScheduler lrScale;
    ILineSearch? ls;
}