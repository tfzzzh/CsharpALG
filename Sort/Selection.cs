using System.Linq.Expressions;

namespace CsharpALG.Sort;

// select k-th smallest element from an array
// this algorithm has linear expected complexity
public class RandomSelection
{
// return the pivot position of array[p .. q]
static private int partition<T>(T[] arr, int p, int q) where T: IComparable<T>
{
    if (p == q) return p;

    // randomly pick and index in p to q as pivot
    int idx = p + rand.Next() % (q-p+1);
    (arr[idx], arr[q]) = (arr[q], arr[idx]);

    // keep [p, i) <= pivot < [i, j)
    int i=p, j=p;
    T pivot = arr[q];
    while (j < q)
    {
        // arr[j] <= pivot
        if (arr[j].CompareTo(pivot) <= 0)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
            i += 1;
        }
        j += 1;
    }

    // arr[q] shall take position i
    (arr[i], arr[q]) = (arr[q], arr[i]);
    return i;
}

// select r-th largest element from arr[p to q] inclusive, 0 <= r < (q-p+1)
public static T Select<T>(T[] arr, int p, int q, int r) where T: IComparable<T>
{
    if (p == q) return arr[p];

    // partition arr via a random pivot
    int pivot = partition(arr, p, q);
    int left = pivot - p;

    // found
    if (left == r) return arr[pivot];

    // when there exist more than r elements in arr[p to piovt - 1]
    if (left > r)
    {
        return Select(arr, p, pivot-1, r);
    }
    // left < r -> all number in arr[p to pivot] can be removed
    else
    {
        return Select(arr, pivot+1, q, r-left-1);
    }
}

private static Random rand = new Random();
}


// selection with linear complexity in worst case
public class LinearSelection
{
    // select r-th largest element from arr[p to q]. 0 <= r < (q-p+1)
    public static T Select<T>(T[] arr, int p, int q, int r) where T: IComparable<T>
    {
        // special case: p == q and r == 0
        if (p == q) return arr[p];

        // special case: when arr[p to q] contains <= 5 elements
        if (p+4 <= q)
        {
            insertSort(arr, p, q);
            return arr[p+r];
        }

        // partition T into ceil(n/5) chunks, sort each chunk by insertion sort
        // then take the median of each chunk
        int numEntries = q - p + 1;
        int numChunk = (numEntries+4) / 5;
        T[] medians = new T[numChunk];
        for (int chunk = 0; chunk < numChunk; ++chunk)
        {
            int start = p + chunk * 5;
            int end = Math.Min(q, p + (chunk+1) * 5);
            insertSort(arr, start, end);
            medians[chunk] = arr[start + (end-start) / 2];
        }

        // recusively find median of the medians
        T pivot = Select(medians, 0, numChunk-1, (numChunk-1)/2);

        // use pivot to partition the array
        int pos = partition(arr, p, q, pivot);

        int left = pos - p;

        // found
        if (left == r) return pivot;

        // when there exist more than r elements in arr[p to pos - 1]
        if (left > r)
        {
            return Select(arr, p, pos-1, r);
        }
        // left < r -> all number in arr[p to pos] can be removed
        else
        {
            return Select(arr, pos+1, q, r-left-1);
        }
    }

    static private void insertSort<T>(T[] arr, int p, int q) where T: IComparable<T>
    {
        if (p >= q) return;

        // insert arr[i] into arr[p to i-1]
        for (int i=p+1; i <= q; ++i)
        {
            T toInsert = arr[i];
            int j = i-1;
            while (j >= p && arr[j].CompareTo(toInsert) > 0)
            {
                arr[j+1] = arr[j];
                j -= 1;
            }
            arr[j+1] = toInsert;
        }
    }

    static private int partition<T>(T[] arr, int p, int q, T pivot) where T: IComparable<T>
    {
        if (p == q) return p;

        // keep [p, i) <= pivot < [i, j)
        int i=p, j=p;
        while (j <= q)
        {
            // arr[j] <= pivot
            if (arr[j].CompareTo(pivot) <= 0)
            {
                (arr[i], arr[j]) = (arr[j], arr[i]);
                i += 1;
            }
            j += 1;
        }

        // [p, i-1] <= pivot < [i, q]
        // suppose [start to end] == pivot, one shall
        int end = i-1;
        int start = i-1;
        while (start >= p && arr[start].CompareTo(pivot) == 0)
            start -= 1;
        start += 1;

        return start + (end-start) / 2;
    }
}