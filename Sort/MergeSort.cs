namespace Sort;

public static class MergeSort
{
    public static void Sort<T>(T[] arr, int start, int end, T[] buffer)
        where T : IComparable<T>
    {
        if (start >= end) return;
        int mid = start + (end - start) / 2;

        Sort(arr, start, mid, buffer);
        Sort(arr, mid + 1, end, buffer);

        // copy arr[start to end] to buffer
        int i, j, k;
        for (i = start; i <= end; ++i)
            buffer[i] = arr[i];

        i = start; j = mid + 1; k = start;
        while (i <= mid && j <= end)
        {
            if (buffer[i].CompareTo(buffer[j]) <= 0)
            {
                arr[k++] = buffer[i++];
            }
            else
            {
                arr[k++] = buffer[j++];
            }
        }

        while (i <= mid)
        {
            arr[k++] = buffer[i++];
        }

        while (j <= end)
        {
            arr[k++] = buffer[j++];
        }
    }

    public static void Sort<T>(T[] arr)
        where T : IComparable<T>
    {
        int n = arr.Length;
        T[] buffer = new T[n];
        Sort(arr, 0, n - 1, buffer);
    }
}
