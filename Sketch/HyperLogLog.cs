/* Hyperloglog Sketching for counting numbers of dictinct elements
 * reference: https://oertl.github.io/hyperloglog-sketch-estimation-paper/paper/paper.pdf
 */
using System.Security.Cryptography;

namespace CsharpALG.Sketch;

public class HyperLogLog
{

    /// <summary>
    /// initalize the hyperloglog algorithm.
    /// </summary>
    /// <param name="p">2**p is the size of register. Namely the parameter m in the paper</param>
    /// <exception cref="InvalidDataException">p shall in range[1, 32)</exception>
    public HyperLogLog(int p)
    {
        if (p < 1 || p >= 32) throw new InvalidDataException($"p shall in the range [1,32)");
        logRegister = p;
        registers = new int[NumRegisters];
    }

    public void Insert(long elem)
    {
        UInt32 hashValue = hash(elem);

        // split has value into binary form
        // 31...        ContentSize-1 ... 0
        // xxxxx        xxxxx
        // idx bits     contentbits
        int idx = (int)(hashValue >> ContentSize);
        // Console.WriteLine(idx);

        // get smallest bit_id s.t. hashValue(ContentSize-bit_id) is not 0
        int k = 1;
        while (k <= ContentSize && (hashValue & (1U << (ContentSize - k))) == 0)
        {
            k += 1;
        }

        if (k > registers[idx])
            registers[idx] = k;
    }

    public int CountDistinct(string method="raw")
    {
        ICardinalityEstimator estimator;
        if (method == "raw")
        {
            estimator = new RawEstimator();
        }
        else if (method == "ertl")
        {
            estimator = new ErtlEstimator(ContentSize);
        }
        else
        {
            throw new NotImplementedException($"estimate method {method} is not implemented");
        }

        double cnt = estimator.estimate(registers);
        return (int) cnt;
    }

    public int NumRegisters { get => 1 << logRegister; }
    public int ContentSize { get => 32 - logRegister; }
    private int logRegister;
    private int[] registers;

    // hash function for number x
    private static UInt32 hash(long x)
    {
        byte[] hashed = HashAlgorithm.ComputeHash(BitConverter.GetBytes(x));
        return BitConverter.ToUInt32(hashed);
    }

    private static SHA256 HashAlgorithm = SHA256.Create();
}

interface ICardinalityEstimator
{
    public double estimate(int[] registers);
    public static double Alpha0 { get => 1.0 / (2.0 * Math.Log(2)); }
}

// estimator proposed in
class RawEstimator : ICardinalityEstimator
{
    public double estimate(int[] registers)
    {
        double nRaw = 0;
        int m = registers.Length;
        // \sum 2^{-k_i}
        foreach (int val in registers)
        {
            nRaw += Math.Pow(2.0, -val);
        }
        nRaw = ICardinalityEstimator.Alpha0 * m * m / nRaw;

        if (nRaw <= 2.5 * m)
        {
            int c0 = 0;
            foreach(int val in registers) c0 += (val == 0) ? 1 : 0;

            if (c0 != 0)
            {
                return m * Math.Log(((double) m) / c0);
            }
            else
            {
                return nRaw;
            }
        }
        else if (nRaw <= int.MaxValue / 30)
        {
            return nRaw;
        }
        else
        {
            return int.MinValue * Math.Log(1.0 + nRaw / int.MinValue);
        }
    }
}


class ErtlEstimator: ICardinalityEstimator
{
    public ErtlEstimator(int contentSize)
    {
        this.contentSize = contentSize;
    }

    public double estimate(int[] registers)
    {
        int[] mutiples = new int[contentSize+2];
        foreach(var val in registers) mutiples[val] += 1;

        int m = registers.Length;
        double z = m * tao(1.0 - mutiples[contentSize+1] / m);
        for (int i=contentSize; i >= 1; --i)
            z = 0.5 * (z + mutiples[i]);
        z += m * sigma(((double)mutiples[0]) / m);

        return ICardinalityEstimator.Alpha0 * m * (m / z);
    }

    static double sigma(double x, double tol = 1e-8)
    {
        if (Math.Abs(x - 1.0) <= double.Epsilon) return double.PositiveInfinity;

        double y =  1.0;
        double z = x;

        while(true)
        {
            x = x * x;
            double zPrev = z;
            z = z + x * y;
            y = 2.0 * y;

            if (
                Math.Abs(z - zPrev) < tol ||
                Math.Abs(zPrev) > 1e-2 && Math.Abs(z - zPrev) / Math.Abs(zPrev) < tol
            ) break;
        }

        return z;
    }

    static double tao(double x, double tol = 1e-8)
    {
        if (Math.Abs(x) <= double.Epsilon || Math.Abs(x - 1.0) <= double.Epsilon)
            return 0.0;

        double y  = 1.0;
        double z = 1.0 - x;
        while  (true)
        {
            x = Math.Sqrt(x);
            var zPrev = z;
            y = y / 2.0;
            z = z - (1.0 - x) * (1.0-x) * y;

            if (
                Math.Abs(z - zPrev) < tol ||
                Math.Abs(zPrev) > 1e-2 && Math.Abs(z - zPrev) / Math.Abs(zPrev) < tol
            ) break;
        }
        return z / 3.0;
    }

    private int contentSize;
}