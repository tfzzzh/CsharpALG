using CsharpALG.Optimization;

namespace CsharpALG.Numerical;

public static class Minimizer {
    public static void Minimize(
        Func<NdArray<double>[], double> computeloss,
        Func<NdArray<double>[], NdArray<double>[]> computeGrad,
        IGradientBasedMinimizer method,
        int maxIter,
        double tol = 1e-4
    )
    {
        Action<NdArray<double>[] > backward = (x) => {
            var grads = computeGrad(x);
            foreach((var param, var grad) in x.Zip(grads))
            {
                param.Grad = grad;
            }
        };

        var x = method.Params;
        for (int i = 0; i < maxIter; ++i)
        {
            backward(x);
            double gradNorm = 0.0;
            gradNorm = x.Select(x => {var norm = x.Grad!.L2Norm(); return norm * norm;}).Sum();
            gradNorm = Math.Sqrt(gradNorm);

            method.Step(computeloss, backward);

            double loss = computeloss(x);
            Console.WriteLine($"epoch = {i}, loss = {loss}, gradNorm = {gradNorm}");
            if (gradNorm < tol) break;
        }
    }
}

static class ExampleGradientDescent
{
    public static void Run() {
        int n = 5, d = 10;
        var xs = new NdArray<double>[n];
        for (int i=0; i < n; ++i)
        {
            double[] arr = (double[]) LinearAlg.RandomUniform(-5.0, 5.0, [d]);
            xs[i] = new NdArray<double>(arr, [d]);
        }

        run(xs, "GradientDescent");
        run(xs, "GradientDescentWithLineSearch");
        run(xs, "Adam");
    }

    public static void run(NdArray<double>[] xs, string method="GradientDescent")
    {
        // int n = 5, d = 10;
        // var xs = new NdArray<double>[n];
        // for (int i=0; i < n; ++i)
        // {
        //     double[] arr = (double[]) LinearAlg.RandomUniform(-5.0, 5.0, [d]);
        //     xs[i] = new NdArray<double>(arr, [d]);
        // }
        Console.WriteLine($"Run {method}");
        xs = xs.Select(x => x.Clone()).ToArray();

        IGradientBasedMinimizer opt;
        if (method == "GradientDescent") {
            opt = new GradientDescent(xs, 2e-3);
        }
        else if (method == "GradientDescentWithLineSearch")
        {
            var ls = new WeakWolf(1e-1, 0.1, 0.1);
            opt = new GradientDescent(xs,  learningRate:1e-1, ls:ls);
        }
        else {
            opt = new Adam(xs, 0.5e-1, 0.8, 0.4);
        }
        Minimizer.Minimize(computeloss, computeGrad, opt, 100);

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