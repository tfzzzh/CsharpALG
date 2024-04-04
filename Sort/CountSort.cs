namespace CsharpALG.Sort;

public static class CountSort
{
    public static int[] Sort(int[] arr, int max_value)
    {
        int[] frequency = Enumerable.Repeat(0, max_value + 1).ToArray();
        int n = arr.Length;
        for (int i = 0; i < n; ++i)
        {
            if (arr[i] < 0 || arr[i] > max_value)
                throw new InvalidDataException($"element in arr shall in range [0,{max_value}]");

            frequency[arr[i]] += 1;
        }

        // compute cumulative sum of frequency inplace
        for (int i = 1; i <= max_value; ++i)
            frequency[i] += frequency[i - 1];

        // assign data according to its cumulative index
        int[] arr_sorted = new int[n];
        for (int i = n - 1; i >= 0; --i)
        {
            int elem = arr[i];
            arr_sorted[frequency[elem] - 1] = elem;
            frequency[elem] -= 1;
        }
        return arr_sorted;
    }
}