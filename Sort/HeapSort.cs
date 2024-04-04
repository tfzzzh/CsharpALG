namespace CsharpALG.Sort;

public static class HeapSort
{
    public static void Sort<T>(T[] arr) where T : IComparable<T>
    {
        int n = arr.Length;
        // special case:
        if (n == 0) return;

        var heap = new Heap<T>(arr);
        for (int i = 0; i < n; ++i)
            heap.Pop();
    }
}

// Max Heap with root is the maximum
class Heap<T> where T : IComparable<T>
{
    public Heap(T[] arr)
    {
        data = arr; Size = arr.Length;
        heapfy();
    }

    // get Top element of the heap
    public T Top()
    {
        return data[0];
    }

    public void Pop()
    {
        if (Size == 1)
        {
            Size -= 1;
            return;
        }

        (data[0], data[Size - 1]) = (data[Size - 1], data[0]);
        Size -= 1;
        adjust(0);
    }

    private static int leftChild(int i)
    {
        return 2 * i + 1;
    }

    private static int rightChild(int i)
    {
        return 2 * i + 2;
    }

    private void heapfy()
    {
        if (Size <= 1) return;
        // for a internal node, it must have
        // 2 * i + 1 <= Size - 1 => i <= size/2 - 1
        for (int i = Size / 2 - 1; i >= 0; --i)
        {
            adjust(i);
        }
    }

    private void adjust(int i)
    {
        int left = leftChild(i);
        int right = rightChild(i);
        while (left < Size)
        {
            int max_id = i;
            if (data[max_id].CompareTo(data[left]) < 0)
                max_id = left;
            if (right < Size && data[max_id].CompareTo(data[right]) < 0)
                max_id = right;

            if (max_id == i) break;
            (data[i], data[max_id]) = (data[max_id], data[i]);

            i = max_id;
            right = rightChild(i);
            left = leftChild(i);
        }
    }

    private T[] data;
    public int Size;
}