using CsharpALG.Numerical;
namespace CsharpALG.Optimization;

public class Adam : IGradientBasedMinimizer
{
    public Adam(
        NdArray<double>[] x, double lr=1e-3, double beta1=0.9, double beta2 = 0.999,
        double eps=1e-08
    )
    {
        this.x = x;
        eta = lr;
        (this.beta1, this.beta2, this.eps) = (beta1, beta2, eps);
        (prodb1, prodb2) = (1.0, 1.0);
        numStep = 0;

        mom = new NdArray<double>[x.Length];
        curv = new NdArray<double>[x.Length];
        initMomCurv();
    }

    public void Step() {
        prodb1 *= beta1;
        prodb2 *= beta2;

        for (int i=0; i < x.Length; ++i) {
            var m = mom[i].Data;
            var v = curv[i].Data;
            var grad = x[i].Grad!.Data;
            var param = x[i].Data;

            for (int j=0; j < m.Length; ++j) {
                // compute momumtum
                m[j] = beta1 * m[j] + (1 - beta1) * grad[j];
                // compute curvature
                v[j] = beta2 * v[j] + (1 - beta2) * grad[j] * grad[j];
                // compute search direction
                double dir = (m[j] / (1-prodb1)) / (Math.Sqrt(v[j] / (1-prodb2)) + eps);
                // update parameter
                param[j] = param[j] - eta * dir;
            }
        }

        numStep += 1;
    }

    private void initMomCurv() {
        int numParam = x.Length;
        for (int i=0; i < numParam; ++i) {
            mom[i] = new NdArray<double>(x[i].Shape);
            curv[i] = new NdArray<double>(x[i].Shape);

            Array.Fill(mom[i].Data, 0.0);
            Array.Fill(curv[i].Data, 0.0);
        }
    }

    public NdArray<double>[] Params {get => x;}

    private double beta1; // average parameter for momentum
    private double beta2; // average parameter for curvature
    private double prodb1; // (1-beta1) ^ t
    private double prodb2; // (1-beta2) ^ t
    private NdArray<double>[] x; // reference to model parameters
    private double eta; // learning rate
    private int numStep; // called step method numStep times
    private double eps; // smoother
    private NdArray<double>[] mom; // momumtum
    private NdArray<double>[] curv; // curvature
}