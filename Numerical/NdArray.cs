using System.Diagnostics;
using System.Text;

namespace CsharpALG.Numerical;

public class NdArray<T>
{
    public NdArray(int[] shape)
    {
        Shape = shape;
        NDim = shape.Length;
        Offset = 0;

        // init stride
        initContinuousStride();
        Length = Stride![0] * Shape[0];

        // init buffer fo
        buffer_ = new T[Length];

        Grad = null;
    }

    public NdArray(T[] data, int[] shape)
    {
        Shape = shape;
        NDim = shape.Length;
        Offset = 0;

        // init stride
        initContinuousStride();
        Length = Stride![0] * Shape[0];

        // init buffer fo
        buffer_ = data;

        Grad = null;
    }

    // From T[] to NdArray
    public static NdArray<T> From(T[] arr)
    {
        T[] data = (T[]) arr.Clone();
        return new NdArray<T>(data, [arr.Length]);
    }

    // From T[,] to NdArray
    public static NdArray<T> From(T[,] arr)
    {
        int[] shape = [arr.GetLength(0), arr.GetLength(1)];

        T[] data = new T[((long) shape[0]) * shape[1]];
        long k = 0;
        for (int i=0; i < shape[0]; ++i)
            for (int j=0; j < shape[1]; ++j) {
                data[k] = arr[i, j];
                k += 1;
            }

        return new NdArray<T>(data, shape);
    }

    // From T[,,] to NdArray
    public static NdArray<T> From(T[,,] arr)
    {
        int[] shape = [arr.GetLength(0), arr.GetLength(1), arr.GetLength(2)];

        T[] data = new T[((long) shape[0]) * shape[1] * shape[2]];
        long l = 0;
        for (int i=0; i < shape[0]; ++i)
            for (int j=0; j < shape[1]; ++j) {
                for (int k=0; k < shape[2]; ++k) {
                    data[l] = arr[i, j, k];
                    l += 1;
                }
            }

        return new NdArray<T>(data, shape);
    }

    public NdArray<T> Clone()
    {
        T[] data_cloned = (T[]) buffer_.Clone();
        NdArray<T> clone = new NdArray<T>(data_cloned, Shape);
        return clone;
    }

    // use subscripts to access data
    public T this[params int[] indices]
    {
        get => buffer_[Sub2Ind(indices)];
        set {buffer_[Sub2Ind(indices)] = value;}
    }

    // print ND array

    public T[] Data
    {
        get => buffer_;
    }

    // subscription A[i1, i2 ..] to linear index A[pos]
    public long Sub2Ind(int[] subs)
    {
        if (subs.Length != NDim)
        {
            throw new InvalidDataException("subs does not have same dims with tensor");
        }

        long pos = Offset;
        for (int i = 0; i < NDim; ++i)
        {
            if(subs[i] < -1 || subs[i] >= Shape[i]){
                throw new InvalidDataException($"{i}th index exceeds the bound of the shape.");
            }
            pos += subs[i] * Stride[i];
        }
        return pos;
    }

    private void initContinuousStride()
    {
        Stride = new long[NDim];
        Stride[NDim - 1] = 1;
        for (int i = NDim - 2; i >= 0; --i)
            Stride[i] = Stride[i + 1] * Shape[i + 1];
    }

    public override string ToString()
    {
        // Somtimes the tensor is not contiguous, so we need to convert the index calculated
        // by shape to the real index calculated by sride.
        Func<long, long> getRealPos = idx => {
            long res = 0;
            long mod = 1;
            for (int i = NDim - 1; i >= 1; i--) mod *= Shape[i];
            for (int i = 0; i < NDim; i++) {
                long shape = idx / mod;
                idx -= shape * mod;
                res += shape * Stride[i];
                if(i < NDim - 1 ) mod /= Shape[i + 1];
            }
            return res + Offset;
        };

        StringBuilder r = new StringBuilder($"Tensor(type:{typeof(T).Name}):\n");
        for (long i = 0; i < Length; i++) {
            long mod = 1;
            for (int j = NDim - 1; j >= 0; j--) {
                mod *= Shape[j];
                if (i % mod == 0) {
                    r.Append("[");
                } else {
                    break;
                }
            }
            r.Append(" ").Append(buffer_[getRealPos(i)]);

            if ((i + 1) % Shape[NDim - 1] != 0) r.Append(",");

            r.Append(" ");
            mod = 1;
            int hit_times = 0;
            for (int j = NDim - 1; j >= 0; j--) {
                mod *= Shape[j];
                if ((i + 1) % mod == 0) {
                    r.Append("]");
                    hit_times++;
                } else {
                    break;
                }
            }
            if (hit_times > 0 && hit_times < NDim) {
                r.Append(",\n");
                for (int j = 0; j < NDim - hit_times; j++) {
                    r.Append(" ");
                }
            }
        }
        // r.Append("\n");
        return r.ToString();
    }

    private T[] buffer_;
    public int[] Shape { get; internal set; }
    public long[] Stride { get; internal set; }
    public long Length { get; internal set; }
    public int NDim { get; internal set; }
    public long Offset { get; internal set; }
    // when set must same layout
    public NdArray<T>? Grad {get; set;}
}


static class NdArrayExample
{
    public static void Run()
    {
        Console.WriteLine("NdArray from C# array");
        double[,] A = new double[,]{
            {1.0, 2.0, 3.0, 4.0},
            {5.0, 6.0, 7.0, 8.0},
            {9.0, 10.0, 11.0, 12.0}
        };

        run(A);

        double[,,] B = new double[,,] {
            {{1.0, 2.0}, {3.0, 4.0}, {5.0, 6.0}},
            {{2.2, 4.4}, {6.6, 8.8}, {10.0, 12.0}}
        };

        run(B);

        // test for set value
        var arr = new NdArray<double>([3, 6, 3]);
        arr[1, 5, 2] = 12.6;
        Console.WriteLine($"arr[1, 5, 2] = {arr[1, 5, 2]}");
    }

    static void run(double[,] A)
    {
        var tensor = NdArray<double>.From(A);
        Debug.Assert(tensor.NDim == 2);
        Console.WriteLine($"tensor is of shape {tensor.Shape[0]}, {tensor.Shape[1]}");
        // for (int i=0; i < tensor.Shape[0]; ++i)
        // {
        //     for (int j=0; j < tensor.Shape[1]; ++j)
        //     {
        //         Console.Write($"{tensor[i,j]}\t");
        //     }
        //     Console.WriteLine();
        // }
        Console.WriteLine(tensor.ToString());
    }

    static void run(double[,,] A)
    {
        var tensor = NdArray<double>.From(A);
        Debug.Assert(tensor.NDim == 3);
        Console.WriteLine($"tensor is of shape {tensor.Shape[0]}, {tensor.Shape[1]}, {tensor.Shape[2]}");
        // for (int i=0; i < tensor.Shape[0]; ++i)
        // {
        //     for (int j=0; j < tensor.Shape[1]; ++j)
        //     {
        //         Console.Write('[');
        //         for (int k=0; k < tensor.Shape[2]; ++k) {
        //             Console.Write($"{tensor[i,j, k]}\t");
        //         }
        //         Console.Write(']');
        //     }
        //     Console.WriteLine();
        // }
        Console.WriteLine(tensor.ToString());
    }
}