using CsharpALG.Numerical;
using CsharpALG.Optimization;
using CsharpALG.MachineLearning.Data;
namespace CsharpALG.MachineLearning;

/// <summary>
/// Solve binary SVM via sub-gradient descent
/// reference: Pegasos: Primal Estimated sub-GrAdient SOlver for SVM
/// </summary>
public class PegasosSVM
{
    public PegasosSVM(
        int maxIter = 1000, double eta = 1e-3, int batchsize = 128,
        double weight_decay = 0.0, double tol = 1e-4, bool verbose = true,
        string lrSchedule = "PolyLR"
    )
    {
        this.maxIter = maxIter;
        this.eta = eta;
        this.verbose = verbose;
        this.batchsize = batchsize;
        this.weight_decay = weight_decay;
        this.lrSchedule = lrSchedule;
    }

    public NdArray<double> Predict(NdArray<double> X)
    {
        if (weight is null)
            throw new Exception("model is not trained");

        if (X.Shape[1] != weight.Shape[0])
            throw new InvalidDataException("input matrix and model's shape are not compatible:");

        int n = X.Shape[0], d = X.Shape[1];
        NdArray<double> preds = new([n]);
        for (int i = 0; i < n; ++i)
        {
            double pred = 0.0;
            for (int j = 0; j < d; ++j)
                pred += X[i, j] * weight[j];
            preds[i] = pred;
        }
        return preds;
    }

    public NdArray<int> PredictLabel(NdArray<double> X)
    {
        var pred = Predict(X);
        int n = (int) pred.Length;
        int[] label = new int[n];
        for (int i=0; i < n; ++i)
        {
            label[i] = pred[i] >= 0 ? 1 : -1;
        }

        return new NdArray<int>(label, [n]);
    }

    public void Fit(NdArray<double> X, NdArray<int> y)
    {
        var loader = new NdArrayTrainData<double, int>(X, y, batchsize, true);

        int dim = X.Shape[1];
        init(dim);
        var opt = new GradientDescent([weight!], eta, lrSchedule);
        for (int t = 0; t < maxIter; ++t)
        {
            double loss = 0.0;
            foreach ((var xBatch, var yBatch) in loader.GenBatches())
            {
                var pred = Predict(xBatch);

                /*
                grad = mean(l_i' x[i]) + weight_decay * w
                */
                var grad = weight!.Grad;
                for (int i = 0; i < dim; ++i) grad![i] = 0.0;

                int n = xBatch.Shape[0];
                for (int i = 0; i < n; ++i)
                {
                    if (yBatch[i] * pred[i] < 1.0)
                    {
                        for (int j = 0; j < dim; ++j)
                        {
                            grad![j] += -yBatch[i] * xBatch[i, j];
                        }
                    }

                    loss += Math.Max(0.0, 1.0 - yBatch[i] * pred[i]);
                }

                for (int i = 0; i < dim; ++i)
                    grad![i] = grad![i] / n + weight_decay * weight[i];

                opt.Step();
            }

            // check loss
            if (verbose)
                Console.WriteLine($"epoach = {t}, Trainloss = {loss}");
        }
    }

    void init(int dim)
    {
        weight = new NdArray<double>([dim]);
        for (int i = 0; i < dim; ++i) weight[i] = 0.0;

        var grad = new NdArray<double>([dim]);
        for (int i = 0; i < dim; ++i) grad[i] = 0.0;
        weight.Grad = grad;
    }

    double eta;
    int maxIter;
    int batchsize;
    bool verbose;
    double weight_decay;
    string lrSchedule;
    private NdArray<double>? weight;
}

static class PegasosSVMExample
{
    public static void Run()
    {
        var dataset = new BinaryClassificationLinearSeperable(50, [-1, 1]);
        double[,] Xtrain, Xvalid;
        int[] ytrain, yvalid;

        dataset.Generate(400, out Xtrain, out ytrain);
        dataset.Generate(700, out Xvalid, out yvalid); // error for 300?

        runModel(Xtrain, ytrain, Xvalid, yvalid, 40);
    }

    private static void runModel(double[,] XTrain, int[] yTrain, double[,] XValid, int[] yValid, int maxIter=10)
    {
        double lambda = 0.01;
        var model = new PegasosSVM(maxIter, 1.0, weight_decay: lambda, lrSchedule: "Constant");

        NdArray<double> X = NdArray<double>.From(XTrain);
        NdArray<int> y = NdArray<int>.From(yTrain);
        NdArray<double> Xv = NdArray<double>.From(XValid);
        NdArray<int> yv = NdArray<int>.From(yValid);
        model.Fit(X, y);

        double errTrain = Metrics.ZeroOneLoss(y, model.PredictLabel(X));
        double errValid = Metrics.ZeroOneLoss(yv, model.PredictLabel(Xv));

        Console.WriteLine($"train svm finish. Train error = {errTrain}, valid error = {errValid}");

    }
}