namespace CsharpALG.Sort;

public static class QuickSort
{
    private static int partition<T>(T[] arr, int start, int end)
        where T: IComparable<T>
    {
        // keep [0, i) < pivot and pivot <= [i, j)
        int i = start, j = start;
        T pivot = arr[end];

        while (j < end)
        {
            // arr[j] < pivot
            if (arr[j].CompareTo(pivot) < 0)
            {
                // swap arr[i] with arr[j]
                T tmp = arr[i];
                arr[i] = arr[j];
                arr[j] = tmp;

                i += 1;
            }

            j += 1;
        }

        // now one shall move pivot to arr[i]
        arr[end] = arr[i];
        arr[i] = pivot;

        return i;
    }

    public static void Sort<T>(T[] arr, int start, int end)
        where T: IComparable<T>
    {
        if (start >= end) return;

        int pivotPos = partition(arr, start, end);
        // arr[start to pivotPos) < pivot && pivot <= arr[pivotPos+1...]
        Sort(arr, start, pivotPos-1);
        Sort(arr, pivotPos+1, end);
    }
}
