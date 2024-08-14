using System.ComponentModel;
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

    private NdArray(T[] data, int[] shape, long[] stride, long offset, long length)
    {
        Shape = shape;
        NDim = shape.Length;
        Length = length;
        Offset = offset;
        Stride = stride;
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

    // get X[start : end] with start <= end
    public NdArray<T> Slice(int start, int end)
    {
        if (start > end)
            throw new InvalidDataException($"slice start shall not greater than end: start={start}, end={end}");

        if (end > Shape[0])
        {
            Console.WriteLine("end is truncate to shape[0]");
            end = Shape[0];
        }

        // offset of X[start] is start * stride[0]
        // long offsetSlice = start * Stride[0] + Offset;
        int[] subStart = new int[NDim];
        subStart[0] = start;
        long offsetSlice = Sub2Ind(subStart);

        int[] shapeSlice = (int[]) Shape.Clone();
        shapeSlice[0] = end - start;

        long lengthSlice = 1;
        for (int i=0; i < NDim; ++i) lengthSlice *= shapeSlice[i];

        // NdArray(T[] data, int[] shape, long[] stride, long offset, long length)
        return new NdArray<T>(buffer_, shapeSlice, Stride, offsetSlice, lengthSlice);
    }

    // Slice out range
    public NdArray<T> Slice(params (int, int)[] ranges)
    {
        // check if range is valid
        if (ranges.Length != NDim)
            throw new InvalidDataException("Length of range is not equal to dim of array");

        for (int i=0; i < NDim; ++i)
        {
            if (ranges[i].Item1 > ranges[i].Item2)
                throw new InvalidDataException("In i-th dimension, range start > range end");

            if (ranges[i].Item2 > Shape[i])
                throw new InvalidDataException("range out of bound");
        }

        // compute offset
        int[] subStart = new int[NDim];
        for (int i=0; i < NDim; ++i) subStart[i] = ranges[i].Item1;
        long offsetSlice = Sub2Ind(subStart);

        // compute shape and length
        int[] shapeSlice = new int[NDim];
        long lengthSlice = 1;
        for (int i=0; i < NDim; ++i)
        {
            shapeSlice[i] = ranges[i].Item2 - ranges[i].Item1;
            lengthSlice *= shapeSlice[i];
        }

        // return NdArray
        return new NdArray<T>(buffer_, shapeSlice, Stride, offsetSlice, lengthSlice);
    }

    public NdArray<T> TakeRow(int[] indices)
    {
        // check indices[i] >= 0 && < Shape[0]
        var shapeTake = (int[]) Shape.Clone();
        shapeTake[0] = indices.Length;

        long lengthTake = shapeTake.Select(x => (long) x).Aggregate(1L, (acc, val) => acc * val);
        // Console.WriteLine($"length of take is {lengthTake}");
        var dataTake = new T[lengthTake];

        int k = 0; // index to dataTake
        int[] subStart = new int[NDim];
        int[] subEnd = new int[NDim];
        for (int i=1; i < NDim; ++i) subEnd[i] = Shape[i] - 1;
        foreach(int idx in indices)
        {
            subStart[0] = idx;
            subEnd[0] = idx;

            for (long i=Sub2Ind(subStart); i <= Sub2Ind(subEnd); ++i)
            {
                dataTake[k] = buffer_[i];
                k += 1;
            }
        }

        return new NdArray<T>(dataTake, shapeTake);
    }

    public T[] Data
    {
        get => buffer_;
    }

    internal bool nextSub(int[] sub)
    {
        sub[NDim-1] += 1;
        for (int i=NDim-1; i >= 0; --i)
        {
            if (sub[i] < Shape[i]) return true;

            sub[i] = 0;
            if (i > 0)
                sub[i-1] += 1;
        }

        return false;
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
            {{1.0, 1.0}, {2.0, 2.0}, {3.0, 3.0}},
            {{4.2, 4.4}, {5.6, 5.8}, {6.0, 6.0}}
        };

        run(B);
        displayNextSub(NdArray<double>.From(B));

        // test for set value
        var arr = new NdArray<double>([3, 6, 3]);
        arr[1, 5, 2] = 12.6;
        Console.WriteLine($"arr[1, 5, 2] = {arr[1, 5, 2]}");

        Console.WriteLine("test for slice");
        B = new double[,,] {
            {{1.0, 1.0}, {2.0, 2.0}},
            {{3.0, 3.0}, {4.0, 4.0}},
            {{5.0, 5.0}, {6.0, 6.0}},
            {{7.0, 7.0}, {8.0, 8.0}}
        };
        arr = NdArray<double>.From(B);
        Console.WriteLine($"{arr.Slice(1, 3)}");

        B = new double[,,] {
            {{1.0, 2.0}, {3.0, 4}, {5, 6}, {7, 8}},
            {{9, 10}, {10, 11}, {11, 12}, {12, 13}},
            {{14, 15}, {15, 16}, {17, 18}, {18, 19}},
            {{20, 21}, {22, 23}, {23, 24}, {24, 25}},
            {{26, 27}, {27, 28}, {29, 30}, {30, 31}}
        };
        arr = NdArray<double>.From(B);
        arr = arr.Slice((1, 4), (1, 4), (0, 2));
        Console.WriteLine($"{arr}");
        Console.WriteLine("permute arr via (2, 1, 0)");
        arr = arr.TakeRow([2, 1, 0]);
        Console.WriteLine($"{arr}");
        arr = arr.Slice((1, 2), (1,2), (1,2));
        Console.WriteLine($"{arr}");
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

    static void displayNextSub(NdArray<double> arr)
    {
        Console.WriteLine("display subs");
        int[] subs = new int[arr.NDim];
        do
        {
            foreach(int idx in subs)
                Console.Write($"{idx} ");
            Console.WriteLine();
        } while (arr.nextSub(subs));
    }
}