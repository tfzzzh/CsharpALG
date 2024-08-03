using System.Diagnostics;
using CsharpALG.Numerical;
using CsharpALG.Numerical.ArrayExtension;
namespace CsharpALG.MachineLearning;

/// <summary>
/// Traing a binary kernel svm
///
/// kernel svm can be formulated as a optimization problem over RKHS
///
///     \min_{\phi} \|\phi\|^2 /2 + C \sum_{i=1}^N \xi_i
///         s.b.t y_i(\phi(x_i) + b) >= 1
///
/// The problem can be solved in a dual form:
///
///     \max{\lambda \in R^{N}} -1/2 \sum_{i, j} \lambda_i\lambda_j y_iy_j K_{i,j}
///             + \sum_{i} \lambda_i
///         s.b.t \lambda >=0 && \lambda <= C && \sum_i \lambda_i y_i = 0
///
/// with slackness condition:
///     \lambda_i (y_i(\phi(x_i) + b) -1 + \xi_i) = 0
///     (C_i - \lambda_i)x_i = 0
///
/// </summary>
public class KernelSVM
{
    public KernelSVM(
        KernelType kernel,
        double C,
        int maxIter = 10
    )
    {
        this.kernel = kernel;
        this.maxIter = maxIter;
        this.C = C;
        picker = new Random();
    }

    public double Fit(double[,] X, int[] y)
    {
        // check data
        DataChecker.CheckClassficationData(X, y);

        // point to X and allocate memory for lambda
        int n = X.GetLength(0);
        initModel(X, y, n);

        // compute kernel matrix K where K[i, j] = kernel(X[i], X[j])
        double[,] Ker = kernel.Apply(X, X);

        // upgrade lambda
        // in each epoch each \lambda will be updated
        // var picker = new Random();
        double loss = double.MaxValue;
        for (int t = 0; t < maxIter; ++t)
        {
            for (int i = 0; i < n; ++i)
            {
                int j = picker.Next(0, n);
                updateLambda(Ker, i, j);
            }

            // compute loss when epoch complete
            loss = computeTrainLoss(Ker, X, y, lambdas!);

            Console.WriteLine($"epoach={t + 1}, TrainLoss = {loss}");
        }

        computeBias(Ker);
        checkLambda();

        return loss;
    }

    // check if lambda is in the specified constrain
    private void checkLambda()
    {
        for (int i=0; i < lambdas!.Length; ++i)
        {
            if (
                lambdas[i] < 0 && !Utility.IsClose(lambdas[i], 0.0, 1e-10, 1e-10) ||
                lambdas[i] > C && !Utility.IsClose(lambdas[i], C, 1e-10, 1e-10)
            )
                throw new Exception($"Constrain not valid: lambdas[{i}]={lambdas[i]} is out of range [0, {C}]");
        }

        double ylambdaSum = 0.0;
        for (int i = 0; i < lambdas.Length; ++i)
            ylambdaSum += yTrain![i] * lambdas[i];

        if (!Utility.IsClose(ylambdaSum, 0.0))
            throw new Exception($"Constrain not valid: (yTrain * lambdas).sum = {ylambdaSum} is not zero");
    }

    // solve bias from support vector
    // the support vector with lambda_i \in (0, C) shall satisfying
    // y_i [\sum_{k} y[k] * K(x_k, x_i) * \lambda[k] + b] = 1
    // we use b = mean(yi - f(x_i)) to estimate bias
    private void computeBias(double[,] Ker)
    {
        int n = Ker.GetLength(0);
        int[] y = yTrain!;

        List<double> biases = new List<double>();
        for (int i = 0; i < n; ++i)
        {
            if (lambdas![i] > 0 && lambdas[i] < C)
            {
                double fValue = 0.0;
                for (int j = 0; j < n; ++j)
                    fValue += Ker[i, j] * lambdas[j] * y[j];

                biases.Add(y[i] - fValue);
            }
        }

        if (biases.Count > 0) {
            bias = biases.Average();
            biasStddev = Math.Sqrt(biases.Select(x => (x-bias) * (x-bias)).Average());
        }

        Console.WriteLine($"{bias}, {biasStddev}");
    }

    private void updateLambda(double[,] Ker, int i, int j)
    {
        int n = Ker.GetLength(0);
        int[] y = yTrain!;

        // evaluate distance of X[i] and X[j] under the kernel
        // when dist(X[i], X[j]) = 0, skip update
        double dist = Ker[i, i] - 2 * Ker[i, j] + Ker[j, j];
        if (Utility.IsClose(dist, 0.0)) return;

        Debug.Assert(dist >= 0, $"dist={dist}");

        // compute f(X[j]) - y[j] - f(X[i]) + y[i] which equals to
        // \sum_{k} (K(x_k, x_j) - K(x_k, x_i)) \lambda(k) + y[i] - y[j]
        double fDiff = 0;
        for (int k = 0; k < n; ++k)
        {
            fDiff += y[k] * (Ker[k, j] - Ker[k, i]) * lambdas![k];
        }
        fDiff += y[i] - y[j];

        // compute new value for lambdas[i] and then shrinkage it into [0, C]
        double lambda = rangeCut(
            lambdas![i] + y[i] * fDiff / dist,
            lambdas[i], lambdas[j], y[i], y[j]
        );

        // update lambda[i] and lambda[j]
        lambdas[j] = fixOutOfRange(
            lambdas[j] + y[i] * y[j] * (lambdas[i] - lambda),
            0.0, C
        );
        lambdas[i] = lambda;
    }

    private void initModel(double[,] X, int[] y, int n)
    {
        XTrain = X;
        yTrain = y;
        lambdas = new double[n];
        bias = 0.0;
        biasStddev = double.NaN;
    }

    public double[] PredictLogProba(double[,] X)
    {
        int n1 = X.GetLength(0), n2 = XTrain!.GetLength(0);

        if (XTrain is null || lambdas is null || yTrain is null) throw new Exception("KernelSVM is not trained");

        // compute kernel value between train set and X
        double[,] Ker = kernel.Apply(X, XTrain);

        double[] pred = new double[n1];
        for (int i = 0; i < n1; ++i)
        {
            pred[i] = 0.0;
            for (int j=0; j < n2; ++j)
            {
                pred[i] += Ker[i,j] * lambdas[j] * yTrain[j];
            }
            pred[i] += bias;
        }

        return pred;
    }

    public int[] Predict(double[,] X)
    {
        var logProb = PredictLogProba(X);
        int[] label = new int[logProb.Length];

        for (int i = 0; i < logProb.Length; ++i)
        {
            if (logProb[i] >= 0)
            {
                label[i] = 1;
            }
            else
            {
                label[i] = -1;
            }
        }

        return label;
    }

    // Dual loss: 0.5 * \sum_{i, j} y_i y_j \lambda_i \lambda_j K_{i,j} - \sum_{\lamba_i}
    static double computeTrainLoss(double[,] Ker, double[,] X, int[] y, double[] lambdas)
    {
        int n = X.GetLength(0);
        double loss = 0.0;
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < n; ++j)
            {
                loss += y[i] * y[j] * lambdas[i] * lambdas[j] * Ker[i, j];
            }
        }

        loss = loss / 2 - lambdas.Sum();
        return loss;
    }

    double rangeCut(double x, double lambda_i, double lambda_j, int y_i, int y_j)
    {
        double L, H;
        if (y_i == y_j)
        {
            L = Math.Max(0, lambda_i + lambda_j - C);
            H = Math.Min(C, lambda_i + lambda_j);
        }
        else
        {
            L = Math.Max(0, lambda_i - lambda_j);
            H = Math.Min(C, C + lambda_i - lambda_j);
        }

        Debug.Assert(L <= H, $"L={L}, H={H}");

        if (x < L) return L;
        if (x > H) return H;
        return x;
    }

    static double fixOutOfRange(double x, double a, double b)
    {
        if (x < a && Utility.IsClose(x, a, 1e-10, 1e-10))
            x = a;

        if (x > b && Utility.IsClose(x, b, 1e-10, 1e-10))
            x = b;

        if (x < a || x > b)
            throw new InvalidDataException($"x={x} not in the range [{a}, {b}]");

        return x;
    }

    // data segment
    // kernel maps (array, array) -> double
    KernelType kernel;
    // weight for each sample
    double[]? lambdas;
    double bias;
    double biasStddev;
    // reference to train features
    double[,]? XTrain;
    int[]? yTrain;
    // tranin parameters
    int maxIter;
    double C;
    Random picker;
}


public class KernelType
{
    public KernelType(KernelFuncType kernel)
    {
        this.kernel = kernel;
    }

    public static double Linear(Span<double> x1, Span<double> x2)
    {
        return x1.VecDot(x2);
    }


    public double[,] Apply(double[,] X, double[,] Y)
    {
        int m = X.GetLength(0), n = Y.GetLength(0), d = X.GetLength(1);
        if (Y.GetLength(1) != d)
            throw new InvalidDataException("X and Y don't have the same number of columns");

        double[,] kernelValues = new double[m, n];
        unsafe
        {
            fixed (double* xptr = X, yptr = Y)
            {
                for (int i = 0; i < m; ++i)
                {
                    for (int j = 0; j < n; ++j)
                    {
                        var rowX = new Span<double>(xptr + i * d, d);
                        var rowY = new Span<double>(yptr + j * d, d);
                        double value = kernel(rowX, rowY);
                        kernelValues[i, j] = value;
                    }
                }
            }
        }

        return kernelValues;
    }

    public double[] Apply(double[,] X, double[] a)
    {
        int m = X.GetLength(0), d = X.GetLength(1);
        if (a.Length != d)
            throw new InvalidDataException("matrix X and vector a don't have the same number of columns");

        double[] kernelValues = new double[m];
        for (int i = 0; i < m; ++i)
        {
            double kernelValue;
            unsafe
            {
                fixed (double* Xptr = X, aptr = a)
                {
                    var rowX = new Span<double>(Xptr + i * d, d);
                    var aSpan = new Span<double>(aptr, d);
                    kernelValue = kernel(rowX, aSpan);
                }
            }

            kernelValues[i] = kernelValue;
        }

        return kernelValues;
    }

    public delegate double KernelFuncType(Span<double> arg1, Span<double> arg2);
    KernelFuncType kernel;
}

static class KernelTypeExample
{
    public static void Run()
    {
        double[,] X = new double[5, 3]{
        {-1.82396316, -0.84624514,  1.57433053},
        { 0.33095771,  1.34058535, -1.26536105},
        {-0.61651668,  0.24436356,  1.15568118},
        { 1.9827146 , -0.89182144, -0.3244714 },
        { 0.85588146, -0.72733899,  1.50528273}
        };

        var kernel = new KernelType(KernelType.Linear);
        double[,] K = kernel.Apply(X, X);

        for (int i = 0; i < K.GetLength(0); ++i)
        {
            for (int j = 0; j < K.GetLength(1); ++j)
            {
                Console.Write($"{K[i, j]}\t");
            }
            Console.WriteLine("");
        }
        Console.WriteLine("");

        double[] a = [-1.05527644, 0.80291977, -1.48295161];
        double[] k = kernel.Apply(X, a);
        for (int i = 0; i < k.GetLength(0); ++i)
        {
            Console.Write($"{k[i]}\t");
        }
        Console.WriteLine("");
    }
}

static class KernelSVMExample
{
    public static void Run()
    {
        var dataset = new Data.BinaryClassificationLinearSeperable(50, [-1, 1]);
        double[,] Xtrain, Xvalid;
        int[] ytrain, yvalid;

        dataset.Generate(400, out Xtrain, out ytrain);
        dataset.Generate(700, out Xvalid, out yvalid); // error for 300?

        runModel(Xtrain, ytrain, Xvalid, yvalid, 40);
    }

    private static void runModel(double[,] XTrain, int[] yTrain, double[,] XValid, int[] yValid, int maxIter=10)
    {
        var kernel = new KernelType(KernelType.Linear);

        var model = new KernelSVM(kernel, 10.0, maxIter);
        model.Fit(XTrain, yTrain);

        // report train loss and valid loss
        double errTrain = Metrics.ZeroOneLoss(yTrain, model.Predict(XTrain));
        double errValid = Metrics.ZeroOneLoss(yValid, model.Predict(XValid));

        Console.WriteLine($"train svm finish. Train error = {errTrain}, valid error = {errValid}");
    }
}