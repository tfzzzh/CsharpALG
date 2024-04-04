using System.Diagnostics;

namespace CsharpALG.Sort;

public static class RadixSort
{
    public static int[] Sort(int[] arr, int basis=10)
    {
        if (basis <= 1) throw new InvalidDataException("basis shall >= 2");

        // find miniumn value of the arr
        int minVal = int.MaxValue;
        foreach(int elem in arr)
            minVal = Math.Min(minVal, elem);

        // substract minVal to make each value of arr nonnegative
        Entry[] entries = new Entry[arr.Length];
        int round = 0;
        for (int i=0; i < arr.Length; ++i)
        {
            entries[i] = new Entry(arr[i] - minVal, basis);
            round = Math.Max(round, entries[i].Digits.Count);
        }

        Entry[] entriesNext = new Entry[arr.Length];

        // perform count sort on each digits
        int[] bins = new int[basis];
        for (int r=0; r < round; ++r)
        {
            foreach(var entry in entries)
            {
                int digit = r < entry.Digits.Count ? entry.Digits[r] : 0;
                bins[digit] += 1;
            }

            for (int i=1; i < basis; ++i)
                bins[i] += bins[i-1];

            // permute entries according to its count number
            for (int i = entries.Length-1; i >= 0; --i)
            {
                var entry = entries[i];
                int digit = r < entry.Digits.Count ? entry.Digits[r] : 0;
                entriesNext[bins[digit]-1] = entry;
                bins[digit] -= 1;
            }

            for (int i=0; i < basis; ++i)
                bins[i] = 0;

            (entries, entriesNext) = (entriesNext, entries);
        }

        // add minVal to return array
        int[] result = new int[arr.Length];
        for (int i=0; i < arr.Length; ++i)
        {
            result[i] = entries[i].Value + minVal;
        }

        return result;
    }
}

struct Entry
{
    public Entry(int num, int basis=10)
    {
        Value = num;
        Digits = new List<int>();

        if (num == 0)
        {
            Digits.Add(0);
        } else {
            while (num > 0)
            {
                Digits.Add(num % basis);
                num /= basis;
            }
        }
    }

    public int Value;
    public List<int> Digits; // digits from least significant bits to the most
}