// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using CsharpALG.Sort;

// var rand = new Random();
// for (int i=0; i < 10; ++i)
//     Console.WriteLine(rand.Next());

int[] arr = { 5, 4, 2, 3, 1, 1, 2, 4 };
// QuickSort.Sort(arr, 0, arr.Length - 1);
HeapSort.Sort(arr);
foreach (int i in arr)
    Console.WriteLine(i);

string[] arr_str = { "ahke", "aabh", "aa", "aa", "a", "bcd", "bc", "bce", "bcdefg" };
// QuickSort.Sort(arr_str, 0, arr_str.Length-1);
HeapSort.Sort(arr_str);
foreach (var i in arr_str)
    Console.WriteLine(i);

// test counted sort
arr = [1, 5, 4, 2, 2, 1, 1, 3, 2, 1, 5, 5, 4, 2, 6, 10];
arr = CountSort.Sort(arr, 10);
Array.Reverse(arr);
foreach (var i in arr)
    Console.WriteLine(i);

testRadixSort();
testRandomSelection();
testLinearSelection();

// test Radix Sort
void testRadixSort()
{
    int[] arr = [36, 41, 56, -9, -86, 33, 77, -49, -54, 6, -44, 70, -58, -38, 95, 74, 29, -41, -19, -94, 28, -75, 83, 92, -15, 13, 80, 72, -16, -84, 46, -84, 69, -83, -76, -91, 67, 84, 86, -88, 0, 24, 71, -78, -42, 32, -77, 77, 99, 59, -63, 17, -71, 30, 48, -61, -80, 2, 42, 30, 95, -70, -15, -9, -79, -70, -6, 29, 47, 16, 29, 11, 79, 24, 3, 33, -47, -21, -3, 83, 33, -44, 65, -8, -59, 78, 46, 99, -8, 60, -48, -41, -16, -70, -54, 89, -72, -64, -17, 6, -22, -92, 36, -99, 77, -63, -70, -64, 93, 51, -11, -53, 64, 83, -62, 75, 53, -30, -80, 16, -9, 93, 5, 81, -88, -25, -43, 21, -9, 54, -76, -60, -80, -78, -19, -65, -3, -25, 36, -88, -49, -98, 10, -24, 74, -16, -66, 16, 29, 73, -55, 30, -60, 29, 64, -52, -89, -86, 82, 98, -12, 55, 42, 73, -31, -95, 82, 14, 52, -88, 34, -41, -69, -7, -4, 43, -30, -12, 67, 42, 54, 4, 81, 8, 61, -5, -42, 22, 91, 33, 65, 63, 2, -83, -33, -19, -17, 93, 0, -72, -94, -42, 78, 53, 76, 38, 83, 31, -52, 65, -74, -61, 26, -88, 25, 44, 16, -26, -9, -35, -97, 52, 66, 62, 61, 42, -23, -48, -94, -12, 24, 63, 34, 45, -14, -11, -89, -2, -27, -85, -52, 73, -88, 70, -64, 92, 47, 14, -36, -13, 33, 36, -63, -44, -17, -61, -4, -35, 92, -96, -41, -62, -83, 94, 97, 99, -32, 26, -27, -68, 61, 94, 76, 48, -49, -75, 90, 82, -53, 53, 11, 64, -6, 31, 2, -62, -76, -77, 2, 18, 45, -81, -2, -1, -29, 15, 1, 23, 23, 55];
    var arr_sorted = RadixSort.Sort(arr);
    Debug.Assert(arr.Length == arr_sorted.Length);
    Array.Sort(arr);
    int k = 0;
    foreach (int i in arr_sorted)
    {
        Console.Write($"{i} ");
        Debug.Assert(i == arr_sorted[k++]);
    }
    Console.WriteLine();
}

void testRandomSelection()
{
    int[] arr = new int[]{1,2,3,4,5,6};
    for (int i=0; i < arr.Length; ++i)
    {
        int[] arr_clone = (int[]) arr.Clone();
        Debug.Assert(RandomSelection.Select(arr_clone, 0, arr.Length-1, i) == (i+1));
    }

    arr = new int[]{4,4,2,2,3,3,1,1};
    for (int i=0; i < arr.Length; ++i)
    {
        int[] arr_clone = (int[]) arr.Clone();
        int num = RandomSelection.Select(arr_clone, 0, arr.Length-1, i);
        // Console.WriteLine(num);
        Debug.Assert(num == (i/2+1));
    }

}

void testLinearSelection()
{
    int[] arr = new int[]{11,10,9,8,7,6,5,4,3,2,1};
    for (int i=0; i < arr.Length; ++i)
    {
        int[] arr_clone = (int[]) arr.Clone();
        Debug.Assert(LinearSelection.Select(arr_clone, 0, arr.Length-1, i) == (i+1));
    }

    arr = new int[]{4,4,2,2,3,3,1,1};
    for (int i=0; i < arr.Length; ++i)
    {
        int[] arr_clone = (int[]) arr.Clone();
        int num = LinearSelection.Select(arr_clone, 0, arr.Length-1, i);
        // Console.WriteLine(num);
        Debug.Assert(num == (i/2+1));
    }
}