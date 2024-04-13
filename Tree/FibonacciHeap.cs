namespace CsharpAlG.Tree;

/// <summary>
/// A Fibonacci heap is a data structure for priority queue operations, consisting of
/// a collection of heap-ordered trees. It has a better amortized running time than
/// many other priority queue data structures including the binary heap and binomial heap.
/// </summary>
public class FibonacciHeap<T> where T: IComparable<T>
{
    public FibonacciHeap() {}

    public int Count {get => n_;}

    public T Minimum {
        get
        {
            if (head_ is null) throw new NullReferenceException("get min from an empty heap");
            return head_.Key;
        }
    }

    public FibNode<T> Insert(T elem)
    {
        var x = new FibNode<T>(elem);

        // insert x to the root list
        CircularLinkList.HeadInsert(ref head_, x);

        // if x.Key is smaller than the header change header of the list
        if (x.Key.CompareTo(head_!.Key) < 0)
            head_ = x;

        n_ += 1;
        return x;
    }

    public T ExtractMin()
    {
        // if heap not empty remove head from root list
        if (head_ is null) throw new NullReferenceException("extract min from empty heap");
        T minVal = head_.Key;
        var x = head_;
        CircularLinkList.Remove(ref head_, x);

        // if the old head have child insert them into the root list
        while (x.Child is not null)
        {
            var child = x.Child;
            CircularLinkList.Remove(ref x.Child, child);
            CircularLinkList.HeadInsert(ref head_, child);
        }

        // merge root, make them distinct
        double phi = (Math.Sqrt(5.0) + 1.0) / 2;
        int degreeMax = (int) Math.Ceiling(Math.Log(n_) / Math.Log(phi)) + 5;
        FibNode<T>?[] roots = new FibNode<T>[degreeMax];

        // when root list not null extract one node from the list
        // and merge it into roots
        while (head_ is not null)
        {
            var node = head_;
            CircularLinkList.Remove(ref head_, node);

            while (roots[node.Degree] is not null)
            {
                // when roots[node.Degree] is not null
                int degree = node.Degree;
                var nodeY = roots[degree];

                // make node.Key <= node.Y.Key through swap
                if (node.Key.CompareTo(nodeY!.Key) > 0)
                    (node, nodeY) = (nodeY, node);

                // link node.Y.Key into node.Key
                nodeY.Parent = node;
                CircularLinkList.HeadInsert(ref node.Child, nodeY);

                // update degree and mark
                node.Degree += 1;
                nodeY.Mark = false;

                roots[degree] = null;
            }

            roots[node.Degree] = node;
        }

        // insert node in roots into root list
        FibNode<T>? minNode = null;
        foreach(var node in roots)
        {
            if (node is not null)
            {
                CircularLinkList.HeadInsert(ref head_, node);

                if (minNode is null || minNode.Key.CompareTo(node.Key) > 0)
                    minNode = node;
            }
        }

        // set head to the min val
        head_ = minNode;
        n_ -= 1;
        return minVal;
    }

    public void DecreaseKey(FibNode<T> x, T newKey)
    {
        // check if newKey is less than the old
        if (x.Key.CompareTo(newKey) < 0)
            throw new ArgumentException($"new Key value {newKey} shall less or equal than the old key {x.Key}");

        // check if x and its parent invalid heap condition
        x.Key = newKey;
        var y = x.Parent;
        if (y is not null && x.Key.CompareTo(y.Key) < 0)
        {
            // the condition not valid perform node cut and possible
            // remove x from y'child and move it to root list
            CircularLinkList.Remove(ref y.Child, x);
            y.Degree -= 1;
            x.Parent = null;
            x.Mark = false;
            CircularLinkList.HeadInsert(ref head_, x);

            // move y to root list
            // until its parent is null, or it is not marked
            while (y.Parent is not null && y.Mark)
            {
                var parent = y.Parent;
                // remove y from its parent's child
                CircularLinkList.Remove(ref parent.Child, y);
                parent.Degree -= 1;

                // insert y to root list and set its parent to null
                y.Parent = null;
                y.Mark = false;
                CircularLinkList.HeadInsert(ref head_, y);

                y = parent;
            }

            // when y's parent is not null mark it with true
            if (y.Parent is not null) y.Mark = true;
        }

        // head update
        if (newKey.CompareTo(head_!.Key) < 0)
            head_ = x;
    }

    private int n_; // number of nodes
    private FibNode<T>? head_; // header of root list, one shall keep it minimal
}

public static class CircularLinkList
{
    public static void HeadInsert<T>(ref FibNode<T>? head, FibNode<T> x)
        where T: IComparable<T>
    {
        if (head is null)
        {
            head = x;
            head.Prev = head;
            head.Next = head;
            return;
        }

        // insert x after head
        // fields shall update: head.Next, x.prev, x.Next, head.next.prev
        // special case head == head.Next
        if (head == head.Next)
        {
            // link list into head -> x ->
            head.Next = x;
            x.Prev = head;
            x.Next = head;
            head.Prev = x;
        }
        else
        {
            var Second = head.Next;
            head.Next = x;
            x.Prev = head;
            x.Next = Second;
            Second!.Prev = x;
        }
    }

    public static void Remove<T>(ref FibNode<T>? head, FibNode<T> x)
        where T: IComparable<T>
    {
        // case1: the removed node is head
        if (x == head)
        {
            // case 1.1 the list only contains one node
            if (head.Next == head)
            {
                head = null;
            }
            else
            {
                // change head,
                // Second.Prev
                // tail.Next,
                var tail = head.Prev;
                var second = head.Next;
                tail!.Next = second;
                second!.Prev = tail;
                head = second;
            }
        }
        // case2: the removed node is not head
        else
        {
            // prev -> node -> next => prev->next
            // change prev.Next, next.Prev
            var prev = x.Prev;
            var next = x.Next;
            prev!.Next = next;
            next!.Prev = prev;
        }

        x.Prev = null;
        x.Next = null;
    }
}

public class FibNode<T> where T: IComparable<T>
{
    public FibNode(T key)
    {
        Key = key;
    }

    public T Key;
    public bool Mark;
    public int Degree;
    public FibNode<T>? Prev;
    public FibNode<T>? Next;
    public FibNode<T>? Parent;
    public FibNode<T>? Child;
}