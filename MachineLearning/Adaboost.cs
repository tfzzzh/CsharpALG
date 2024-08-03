using System.Diagnostics;
namespace CsharpALG.MachineLearning;

public class AdaBoost
{
    public AdaBoost(int maxIter)
    {
        stumps = new List<Stump>();
        etas = new List<double>();
        this.maxIter = maxIter;
    }

    public double Fit(double[,] X, int[] y)
    {
        int n = X.GetLength(0), d = X.GetLength(1);

        // check if train data not empty and size of x, y compatible
        if (n == 0 || d == 0)
            throw new InvalidDataException("input matrix X is empty");

        if (n != y.Length)
            throw new InvalidDataException("row of input matrix X is not equal to length of y");

        // clear stump and etas
        stumps.Clear();
        etas.Clear();

        // initalize training algorithm
        var f = new double[n]; // ensemble score
        var losses = new double[n]; // exponential losses of f
        Array.Fill(losses, 1.0); // initally exp(-yf) = exp(0) = 1.0
        var weight = new double[n]; // weight to train weak classifier
        Array.Fill(weight, 1.0 / n);
        double totalLoss = losses.Sum();

        // foreach epoach perform frank-wolf like update
        for (int t = 0; t < maxIter; ++t)
        {
            // train weight classfier using weight
            Stump stump = new Stump();
            var errWeak = stump.Fit(X, y, weight);

            // update learning rate ToDo: handle the special case errWeak is small
            double eta = 0.5 * Math.Log((1.0 - errWeak) / errWeak);
            int[] h = stump.Predict(X);

            // update ensemble score and losses
            for (int i = 0; i < n; ++i)
            {
                f[i] += eta * h[i];
                losses[i] = Math.Exp(-y[i] * f[i]);
            }

            // update weight
            totalLoss = losses.Sum();
            for (int i = 0; i < n; ++i)
            {
                weight[i] = losses[i] / totalLoss;
            }

            stumps.Add(stump);
            etas.Add(eta);

            // display train error
            Console.WriteLine($"epoach={t + 1}, TrainLoss = {totalLoss}");
        }

        // return loss of the model
        return totalLoss;
    }

    public int[] Predict(double[,] X)
    {
        var logProb = PredictLogProba(X);
        int[] y = new int[logProb.Length];

        for (int i = 0; i < logProb.Length; ++i)
        {
            if (logProb[i] >= 0)
            {
                y[i] = 1;
            }
            else
            {
                y[i] = -1;
            }
        }

        return y;
    }

    public double[] PredictLogProba(double[,] X)
    {
        int n = X.GetLength(0);
        double[] y = new double[n];

        for (int i = 0; i < stumps.Count; ++i)
        {
            int[] yStump = stumps[i].Predict(X);
            for (int j = 0; j < n; ++j)
            {
                y[j] += yStump[j] * etas[i];
            }
        }

        return y;
    }

    List<Stump> stumps;
    List<double> etas;
    int maxIter;
}


class Stump
{

    public Stump()
    {
        dim = -1;
    }

    public int[] Predict(double[,] X)
    {
        int n = X.GetLength(0);
        int[] ys = new int[n];

        for (int i = 0; i < n; ++i)
        {
            if (X[i, dim] <= theta)
            {
                ys[i] = h;
            }
            else
            {
                ys[i] = -h;
            }
        }

        return ys;
    }

    public double Fit(double[,] X, int[] y, double[] weight)
    {
        double loss = double.MaxValue;

        int m = X.GetLength(0), n = X.GetLength(1);
        for (int d = 0; d < n; ++d)
        {
            double lossCurr, thetaCurr;
            int hCurr;
            fitAtOneDim(X, y, weight, d, out hCurr, out lossCurr, out thetaCurr);

            if (loss > lossCurr)
            {
                loss = lossCurr;
                theta = thetaCurr;
                h = hCurr;
                dim = d;
            }
        }

        return loss;
    }

    public override string? ToString()
    {
        return $"Stump(theta={theta}, h={h}, dim={dim})";
    }

    private static void fitAtOneDim(
        double[,] X, int[] y, double[] weight, int dim, out int h,
        out double lossOpt, out double thetaOpt
    )
    {
        // get loss when h set -1
        double lossN, thetaN;
        fitAtOneDimWhenHFixed(X, y, weight, dim, -1, out lossN, out thetaN);

        // get loss when h set to 1
        double lossP, thetaP;
        fitAtOneDimWhenHFixed(X, y, weight, dim, 1, out lossP, out thetaP);

        // compare the two and return optimal
        if (lossN <= lossP)
        {
            lossOpt = lossN;
            thetaOpt = thetaN;
            h = -1;
        }
        else
        {
            lossOpt = lossP;
            thetaOpt = thetaP;
            h = 1;
        }
    }


    // do not change X, y, weight
    private static void fitAtOneDimWhenHFixed(
        double[,] X, int[] y, double[] weight, int dim, int h,
        out double lossOpt, out double thetaOpt
    )
    {
        int n = X.GetLength(0);
        Debug.Assert(n == y.Length && n == weight.Length);

        // sort x in ascending and change y, weight accordingly
        // implement this via using a permutation indices s.t.
        // X[indices[0], dim] <= X[indices[1], dim] ...
        int[] indices = Enumerable.Range(0, n).ToArray();
        Array.Sort(indices, (a, b) => X[a, dim].CompareTo(X[b, dim]));

        // compute error when theta set to x[0] - 1
        // now all X[:, dim] > theta_curr, then X[:, dim] will be labeled
        // with -h, if -h y[i] < 0 -> prediction is wrong suffer loss
        double theta_curr = X[indices[0], dim] - 1;
        double loss_curr = 0.0;
        for (int i = 0; i < n; ++i)
        {
            if (-h * y[i] < 0)
            {
                loss_curr += weight[i];
            }
        }

        // init optimal error and theta
        lossOpt = loss_curr;
        thetaOpt = theta_curr;

        // update error when theta set from x[0] to x[n-1]
        int start = 0;
        while (start < n)
        {
            int end = start + 1;
            theta_curr = X[indices[start], dim];
            while (end < n && X[indices[end], dim] <= theta_curr)
                end += 1;

            // when update theta_curr to X[indices[start], dim] labels
            // from [indices[start] to indices[end-1]] are changed from -h to h
            // if h * y[indices[i]] > 0 -> weight[indices[i]] shall substract from the loss
            // if h * y[indices[i]] < 0 -> weight[indices[i]] shall add to the loss
            for (int i = start; i < end; ++i)
            {
                loss_curr -= h * y[indices[i]] * weight[indices[i]];
            }

            // update lossOpt
            if (lossOpt > loss_curr)
            {
                lossOpt = loss_curr;
                thetaOpt = theta_curr;
            }

            start = end;
        }
    }

    double theta;
    int h; // h \in {-1, 1}
    int dim; // take which dimension to perform classification
}

static class AdaBoostExample
{
    public static void Run()
    {
        var dataset = new Data.BinaryClassificationLinearSeperable(10, [-1, 1]);
        double[,] Xtrain, Xvalid;
        int[] ytrain, yvalid;

        dataset.Generate(400, out Xtrain, out ytrain);
        dataset.Generate(400, out Xvalid, out yvalid);

        runModel(Xtrain, ytrain, Xvalid, yvalid, 40);
    }

    private static void runModel(double[,] XTrain, int[] yTrain, double[,] XValid, int[] yValid, int maxIter=10)
    {
        var model = new AdaBoost(maxIter);
        model.Fit(XTrain, yTrain);

        // report train loss and valid loss
        double errTrain = Metrics.ZeroOneLoss(yTrain, model.Predict(XTrain));
        double errValid = Metrics.ZeroOneLoss(yValid, model.Predict(XValid));

        Console.WriteLine($"train adaboost finish. Train error = {errTrain}, valid error = {errValid}");
    }
}

static class ExampleStump
{
    private static void runModel(double[,] X, int[] y, double[] weight)
    {
        Stump stump = new Stump();
        double loss = stump.Fit(X, y, weight);

        Console.WriteLine($"loss = {loss}, stump = {stump}");
    }

    public static void Run()
    {
        // 3 dim the second is minimal
        double[,] X = new double[6, 3]
        {
            {5.0, -1.0, 0.3},
            {4.0, 1.0, 0.6},
            {9.5, 1.0, 2.6},
            {7.7, 1.0, 6.9},
            {5.8, 0.0, 6.6},
            {-10.2, -2.0, 3.7}
        };

        int[] y = [-1, 1, 1, 1, -1, -1];
        double[] weight = [1.0 / 6, 1.0 / 6, 1.0 / 6, 1.0 / 6, 1.0 / 6, 1.0 / 6];

        runModel(X, y, weight);

        y = [1, -1, -1, -1, 1, 1];
        runModel(X, y, weight);

        // not classfied case
        X = new double[6, 3]
        {
            {5.0, -1.0, 0.3},
            {4.0, 1.0, 0.6},
            {9.5, 1.0, 2.6},
            {7.7, 1.0, 6.9},
            {5.8, 0.0, 6.6},
            {-10.2, -2.0, 3.7}
        };

        y = [-1, 1, -1, -1, -1, -1];
        weight = [1.0 / 6, 1.0 / 6, 1.0 / 6, 1.0 / 6, 1.0 / 6, 1.0 / 6];
        runModel(X, y, weight);
    }
}