
using System.Diagnostics;
using CsharpAlG.Tree;
test_binarysearchtree_insert();
test_binarysearchtree_find();
test_binarysearchtree_remove();
test_binarysearchtree_iterator();
test_rbtree_insert();
test_rbtree_remove();

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