using System.Numerics;

namespace CsharpALG.Numerical;

public class FastFourierTransform
{
    public static Complex[] FFT(Complex[] x)
    {
        x = Padding(x);
        fft(x, inverse: false);
        return x;
    }

    public static Complex[] IFFT(Complex[] x)
    {
        x = Padding(x);
        fft(x, inverse: true);

        int n = x.Length;
        for (int i=0; i < n; ++i) x[i] /= n;

        return x;
    }

    static void fft(Complex[] x, bool inverse = false)
    {
        int n = x.Length;
        if (n == 1) return;

        double theta = 2 * Math.PI / n;
        Complex omega = !inverse ? new Complex(Math.Cos(theta), -Math.Sin(theta)) :
            new Complex(Math.Cos(theta), Math.Sin(theta));

        // collect x[::2] and x[1::2]
        Complex[] odds = new Complex[n / 2];
        Complex[] evens = new Complex[n / 2];
        for (int i = 0; i < n; ++i)
        {
            if (i % 2 == 1)
            {
                odds[i / 2] = x[i];
            }
            else
            {
                evens[i / 2] = x[i];
            }
        }

        // divide step: compute fft over odds and evens
        fft(odds, inverse);
        fft(evens, inverse);

        // combine step
        Complex w = new Complex(1.0, 0.0);
        for (int i = 0; i < n / 2; ++i)
        {
            x[i] = w * odds[i] + evens[i];
            x[n / 2 + i] = -w * odds[i] + evens[i];
            w *= omega;
        }
    }

    public static Complex[] Padding(Complex[] x)
    {
        int n = getNbar(x.Length);
        // if (n == x.Length) return x;

        var xBar = new Complex[n];
        for (int i = 0; i < x.Length; ++i)
            xBar[i] = x[i];

        for (int i = x.Length; i < n; ++i)
            xBar[i] = new Complex(0.0, 0.0);
        return xBar;
    }

    static int getNbar(int n)
    {
        int pow = 1;
        while (pow < n)
        {
            pow *= 2;
        }
        return pow;
    }
}


static class FFTExample
{
    public static void Run() {
        // fft result: array([10.+10.j, -4. +0.j, -2. -2.j,  0. -4.j])
        var arr = new Complex[]{
            new Complex(1.0, 1.0), new Complex(2.0, 2.0),
            new Complex(3.0, 3.0), new Complex(4.0, 4.0)
        };

        run(arr);

        // fft: [ 13.8       +14.2j       ,   8.02878426-15.39325035j,
        // -2.5       +11.3j       ,  -2.48248558-11.52878426j,
        //  9.4        +9.2j       , -11.62878426 -2.80674965j,
        // 10.9        -2.7j       , -15.91751442 +8.12878426j]
        arr = [
            new Complex(1.2, 1.3), new Complex(2.2, 2.5),
            new Complex(3.7, 3.7), new Complex(4.6, 4.6),
            new Complex(6.7, 6.7), new Complex(-4.6, -4.6)
        ];

        run(arr);
    }

    static void run(Complex[] arr) {
        Console.WriteLine("value of arr:");
        foreach(var elem in arr)
            Console.Write($"{elem}");
        Console.WriteLine();

        var fftValues = FastFourierTransform.FFT(arr);
        Console.WriteLine("fft values of arr:");
        foreach(var elem in fftValues)
            Console.Write($"{elem}");
        Console.WriteLine();

        var ifftValues = FastFourierTransform.IFFT(fftValues);
        Console.WriteLine("apply fft then ifft:");
        foreach(var elem in ifftValues)
            Console.Write($"{elem}");
        Console.WriteLine();
    }
}