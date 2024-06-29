
using System.Diagnostics;
using CsharpALG.Tree;
test_binarysearchtree_insert();
test_binarysearchtree_find();
test_binarysearchtree_remove();
test_binarysearchtree_iterator();
test_rbtree_insert();
test_rbtree_remove();
test_order_statics_select();
test_order_statics_remove();
test_order_statics_get_rank();
test_fibonacciheap_extract_min();
test_fibonacciheap_decrease_key();

void test_binarysearchtree_insert()
{
    BinarySearchTree<int> tree = new BinarySearchTree<int>();
    int[] toInsert = new int[]{5,5,6,4,1,3,7,2};
    foreach(int num in toInsert)
    {
        tree.Insert(num);
    }
    tree.InorderTreeWalk(x => Console.Write($"{x.Val} "));
    Console.WriteLine();
}

void test_binarysearchtree_find()
{
    BinarySearchTree<int> tree = new BinarySearchTree<int>();
    int n = 100;
    int[] toInsert = new int[n];
    var random = new Random();
    for (int i=0; i < n; ++i) toInsert[i] = random.Next() % 47 - 23;
    foreach(int num in toInsert)
    {
        tree.Insert(num);
    }

    for (int i=0; i < n; ++i)
    {
        var ptr = tree.Find(toInsert[i]);
        Debug.Assert(ptr is not null);
        Debug.Assert(ptr.Val == toInsert[i]);
    }

    Debug.Assert(tree.Find(30) is null);
}

void test_binarysearchtree_remove()
{
    BinarySearchTree<int> tree = new BinarySearchTree<int>();
    int[] toInsert = new int[]{5,5,6,4,1,3,7,2};
    foreach(int num in toInsert)
    {
        tree.Insert(num);
    }

    foreach(int num in toInsert)
    {
        Console.WriteLine($"remove : {num}");
        tree.Remove(num);
        Console.Write("after remove, tree becomes: ");
        tree.InorderTreeWalk(x => Console.Write($"{x.Val} "));
        Console.WriteLine();
    }
}

void test_binarysearchtree_iterator()
{
    BinarySearchTree<int> tree = new BinarySearchTree<int>();
    int[] toInsert = new int[]{5,5,6,4,1,3,7,2};
    foreach(int num in toInsert)
    {
        tree.Insert(num);
    }

    Console.WriteLine("tree elements are:");
    foreach(int num in tree)
    {
        Console.Write($"{num} ");
    }
    Console.WriteLine();
}

void test_rbtree_insert()
{
    Console.WriteLine("test rb tree insert");
    RedBlackTree<int> tree = new RedBlackTree<int>();
    int[] toInsert = new int[]{5,-5,6,4,1,3,7,2};

    int k = 0;
    foreach(int num in toInsert)
    {
        tree.Insert(num);
        Console.WriteLine($"k={k}, val={num}");
        Debug.Assert(tree.IsRedBlackTree());
        k += 1;
    }

    Console.WriteLine("tree elements are:");
    foreach(int num in tree)
    {
        Console.Write($"{num} ");
    }
    Console.WriteLine();
}


void test_rbtree_remove()
{
    int n = 10;
    var tree = new RedBlackTree<int>();
    for (int i=0; i < n; ++i)
    {
        tree.Insert(i);
    }

    for (int i=0; i < n; i+=2)
    {
        tree.Remove(i);
        Debug.Assert(tree.IsRedBlackTree());
        Console.Write($"after remove {i} tree becomes: ");
        foreach(int elem in tree)
        {
            Console.Write($"{elem} ");
        }
        Console.WriteLine();
    }

    for (int i=n-1; i >=1; i-=2)
    {
        tree.Remove(i);
        Debug.Assert(tree.IsRedBlackTree());
        Console.Write($"after remove {i} tree becomes: ");
        foreach(int elem in tree)
        {
            Console.Write($"{elem} ");
        }
        Console.WriteLine();
    }
}

void test_order_statics_select()
{
    Console.Write("test_order_statics_select");
    int[] toInsert = [19, -26, 23, 41, 35, -49, -77, -75, -18, 119, 65, -10, 56, 102, 7, 117, 12, -130, -123, 48, 95, -65, -24, 4, 108, 81, -70, 38, 104, -61, 31, -87, 115, 33, -82, -5, -128, -113, 30, -98, -114, -1, -124, -41, 86, 78, -4, -25, -69, -126, -104, 73, -44, 69];
    var selector = new OrderStatistics<int>();
    foreach(int num in toInsert)
    {
        selector.Insert(num);
        // Console.Write($"{num} ");
        Debug.Assert(selector.IsRedBlackTree());
    }
    // Console.WriteLine();

    Array.Sort(toInsert);
    for (int i=0; i < toInsert.Length; ++i)
    {
        int num = selector.Select(i+1);
        Debug.Assert(num == toInsert[i]);
        Console.Write($"rank={i+1}:value={num} ");
    }
    Console.WriteLine();
}

void test_order_statics_remove()
{
    Console.WriteLine("test_order_statics_remove");
    int[] toInsert = [19, -26, 23, 41, 35, -49, -77, -75, -18, 119, 65, -10, 56, 102, 7, 117, 12, -130, -123, 48, 95, -65, -24, 4, 108, 81, -70, 38, 104, -61, 31, -87, 115, 33, -82, -5, -128, -113, 30, -98, -114, -1, -124, -41, 86, 78, -4, -25, -69, -126, -104, 73, -44, 69];
    var selector = new OrderStatistics<int>();
    var set = new SortedSet<int>();
    foreach(int num in toInsert)
    {
        selector.Insert(num);
        set.Add(num);
        // Console.Write($"{num} ");
        // Debug.Assert(selector.IsRedBlackTree());
    }

    int sz = toInsert.Length;
    foreach(int num in toInsert)
    {
        selector.Remove(num);
        set.Remove(num);
        int r = 1;
        foreach(var item in set)
        {
            Debug.Assert(item == selector.Select(r));
            r += 1;
        }
        Debug.Assert(selector.IsRedBlackTree());
        sz  -= 1;
        Debug.Assert(selector.Count == sz);
        // Console.Write($"{num} ");
        // Debug.Assert(selector.IsRedBlackTree());
    }
}

void test_order_statics_get_rank()
{
    Console.WriteLine("test_order_statics_remove");
    int[] toInsert = [19, -26, 23, 41, 35, -49, -77, -75, -18, 119, 65, -10, 56, 102, 7, 117, 12, -130, -123, 48, 95, -65, -24, 4, 108, 81, -70, 38, 104, -61, 31, -87, 115, 33, -82, -5, -128, -113, 30, -98, -114, -1, -124, -41, 86, 78, -4, -25, -69, -126, -104, 73, -44, 69];
    var selector = new OrderStatistics<int>();
    var set = new SortedSet<int>();
    foreach(int num in toInsert)
    {
        selector.Insert(num);
        set.Add(num);
        // Console.Write($"{num} ");
        // Debug.Assert(selector.IsRedBlackTree());
    }

    Array.Sort(toInsert);
    for (int i=0; i < toInsert.Length; ++i)
    {
        Debug.Assert(selector.GetRank(toInsert[i]) == i+1);
    }
}


void test_fibonacciheap_extract_min()
{
    Console.WriteLine("test_fibonacciheap_extract_min");
    int[] arr = [37, 19, 4, 55, 38, -41, 66, 51];
    var heap = new FibonacciHeap<int>();

    foreach(int elem in arr) heap.Insert(elem);
    List<int> v = new List<int>();
    while (heap.Count > 0)
    {
        v.Add(heap.ExtractMin());
    }

    Debug.Assert(v.Count == arr.Length);
    Array.Sort(arr);
    for (int i=0; i < arr.Length; ++i)
        Debug.Assert(arr[i] == v[i]);
}

void test_fibonacciheap_decrease_key()
{
    Console.WriteLine("test_fibonacciheap_decrease_key");
    int[] arr = [-1,-2,-3,5,4,3,2,1];
    var heap = new FibonacciHeap<int>();

    var nodes = new List<FibNode<int>>();
    foreach(int elem in arr) nodes.Add(heap.Insert(elem));
    int delta = -2;
    foreach(var x in nodes)
    {
        heap.DecreaseKey(x, x.Key + delta);
    }

    List<int> v = new List<int>();
    while (heap.Count > 0)
    {
        v.Add(heap.ExtractMin());
    }

    Debug.Assert(v.Count == arr.Length);
    Array.Sort(arr);
    for (int i=0; i < arr.Length; ++i)
        Debug.Assert(arr[i] == v[i] - delta);
}