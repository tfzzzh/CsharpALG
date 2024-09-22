using System.Diagnostics;
using System.Numerics;
using System.Text;
using CsharpALG.Numerical;

namespace CsharpALG.Miscellaneous;

public class UnSignedBigInteger
{
    // except for 0 itself, highest digit of num shall not be 0
    public UnSignedBigInteger(byte[] num)
    {
        n = num.Length;
        data = new(n);
        for (int i = n - 1; i >= 0; --i) data.Add(num[i]);
    }

    public UnSignedBigInteger(List<byte> data)
    {
        this.data = data;
        n = data.Count;
    }

    public override string? ToString()
    {
        if (n == 0) return null;

        StringBuilder builder = new();
        for (int i = n - 1; i >= 0; --i)
        {
            builder.Append(data[i].ToString("X2"));
        }
        return builder.ToString();
    }

    public static UnSignedBigInteger MultNaive(UnSignedBigInteger a, UnSignedBigInteger b)
    {
        int m = a.NumBytes, n = b.NumBytes;
        List<byte> prod = new(m + n);

        UInt64 carry = 0;
        for (int k = 0; k < m + n - 1; ++k)
        {
            UInt64 digit = carry;
            // convolve: with
            // 0 <= i <= m-1 && 0 <= k-i <= n-1 that is
            for (int i = Math.Max(k - n + 1, 0); i <= Math.Min(m - 1, k); ++i)
            {
                int term = a.Data[i] * b.Data[k - i];
                digit = checked(digit + (UInt64)term);
            }

            // Console.WriteLine($"{digit.ToString("X")}");
            carry = digit / BASE;
            prod.Add((byte)(digit % BASE));
        }
        if (carry != 0)
        {
            Debug.Assert(carry < BASE);
            prod.Add((byte)carry);
        }
        return new UnSignedBigInteger(prod);
    }

    public static UnSignedBigInteger MultByFFT(UnSignedBigInteger a, UnSignedBigInteger b)
    {
        int m = a.NumBytes, n = b.NumBytes;

        // perform fft on a and b
        // Complex[] ca = a.Data.Select(x => new Complex(x, 0)).ToArray();
        // Complex[] cb = b.Data.Select(x => new Complex(x, 0)).ToArray();
        Complex[] ca = new Complex[m+n-1];
        Complex[] cb = new Complex[m+n-1];
        for (int i=0; i < m; ++i) ca[i] = new Complex(a.Data[i], 0.0);
        for (int i=0; i < n; ++i) cb[i] = new Complex(b.Data[i], 0.0);

        var aft = FastFourierTransform.FFT(ca);
        var bft = FastFourierTransform.FFT(cb);

        // perform vector muliply in fft coefficient
        var cft = new Complex[Math.Max(aft.Length, bft.Length)];
        for (int i = 0; i < cft.Length; ++i)
        {
            Complex aft_i = i < aft.Length ? aft[i] : 0;
            Complex bft_i = i < bft.Length ? bft[i] : 0;
            cft[i] = aft_i * bft_i;
        }

        // perform ifft to get polynominal representation
        Complex[] cc = FastFourierTransform.IFFT(cft);

        // make each term in range [0, BASE)
        List<byte> dataC = new(m + n);
        UInt64 carry = 0;
        for (int i = 0; i < m + n - 1; ++i)
        {
            UInt64 term = (UInt64) Math.Round(cc[i].Real);
            Debug.Assert(Math.Abs(cc[i].Imaginary) < 1e-8);
            term += carry;

            dataC.Add((byte)(term % BASE));
            carry = term / BASE;
        }
        // Debug.Assert(dataC.Last() != 0);

        if (carry != 0)
        {
            Debug.Assert(carry < BASE);
            dataC.Add((byte)carry);
        }

        // return the result
        return new UnSignedBigInteger(dataC);
    }

    // getter of data
    public List<byte> Data { get => data; }
    public int NumBytes { get => n; }
    public static UInt16 BASE { get => 1 << 8; }

    private List<byte> data;
    int n;
}


static class UnSignedBigIntegerExample
{
    static public void PrintBigInterger()
    {
        var num = new UnSignedBigInteger(new byte[] { 0xaf, 0xff, 0xab, 0x03 });
        Console.WriteLine($"num={num}");
    }

    static void run(byte[] a, byte[] b)
    {
        var num1 = new UnSignedBigInteger(a);
        var num2 = new UnSignedBigInteger(b);
        var num3 = UnSignedBigInteger.MultNaive(num1, num2);
        var num4 = UnSignedBigInteger.MultByFFT(num1, num2);
        Console.WriteLine($"{num1} * {num2} = {num3}");

        Debug.Assert($"{num3}" == $"{num4}");
    }

    public static void Run() {
        run([0xff, 0xff, 0xff, 0xff, 0xff], [0xff, 0xff, 0xff, 0xff, 0xff]);
        run([0xff, 0xff, 0xff, 0xff, 0xff, 0xab, 0xac, 0xbb], [0xff, 0xff, 0xff, 0xff, 0xff]);
        run([0xff, 0xff, 0xff, 0xff, 0xff, 0xab, 0xac, 0xbb], [0xff, 0xff, 0xff, 0xff, 0xff,0xff, 0xff, 0xff, 0xff, 0xff,0xff, 0xff, 0xff, 0xff, 0xff]);
    }
}