using System.Collections;
using System.Diagnostics;

namespace CsharpAlG.Tree;

/*
Property of a RB tree
1. every node is either black or red
2. the root and leaves (NIL) are black
3. every red node have two black children
4. for every node, all simple pathes from the node to all its descendant leaves
    contain the same number of black nodes
*/
class RedBlackTree<T> : IEnumerable<T>
    where T : IComparable<T>
{
    public RedBlackTree() { root = null; }

    public IEnumerator<T> GetEnumerator() => new RBTreeEnumerator<T>(root);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Insert(T elem)
    {
        // allocate node for the inserted element
        var x = new RBNode<T>(elem, 'R');

        // special case: tree is empty
        if (root is null)
        {
            root = x;
        }
        else
        {
            RBNode<T>? prev = null;
            RBNode<T>? curr = root;

            while (curr is not null)
            {
                prev = curr;
                // elem <= curr.Val
                if (elem.CompareTo(curr.Val) <= 0)
                {
                    // when duplicate found throw exception
                    if (elem.CompareTo(curr.Val) == 0)
                        throw new InvalidDataException($"Keys of RB tree must be distinct. The insertion value {elem} is already in the three");
                    curr = curr.Left;
                }
                else
                {
                    curr = curr.Right;
                }
            }

            if (elem.CompareTo(prev!.Val) <= 0)
            {
                prev.Left = x;
            }
            else
            {
                prev.Right = x;
            }
            x.Parent = prev;
        }

        // fix tree from x
        insertFixUp(x);
    }

    public void Remove(T elem)
    {
        var curr = root;
        while (curr is not null && curr.Val.CompareTo(elem) != 0)
        {
            if (elem.CompareTo(curr.Val) < 0)
            {
                curr = curr.Left;
            }
            else
            {
                curr = curr.Right;
            }
        }

        if (curr is null)
            throw new KeyNotFoundException($"key {elem} is not in the rb tree");

        remove(curr);
    }

    private void insertFixUp(RBNode<T> x)
    {
        // handle conflict that both x and its parents are red
        while (RBNode<T>.GetColor(x.Parent) == 'R')
        {
            // case1: parent is the grand parent's left son
            if (x.Parent == x.Parent!.Parent!.Left)
            {
                // case 1.1 the uncle node is red
                RBNode<T>? uncle = x.Parent!.Parent!.Right;
                if (RBNode<T>.GetColor(uncle) == 'R')
                {
                    x.Parent.Color = 'B';
                    uncle!.Color = 'B';
                    x.Parent!.Parent!.Color = 'R';
                    x = x.Parent!.Parent!;
                }
                else
                {
                    // case 1.2 x is the right child
                    if (x.Parent!.Right == x)
                    {
                        leftRotate(x.Parent!);
                        x = x.Left!;
                    }

                    // case 1.3 x is the left child
                    rightRotate(x.Parent!.Parent!);
                    x.Parent!.Color = 'B';
                    x.Parent!.Right!.Color = 'R';
                }
            }
            // case2: parent is the grand parent's right son
            else
            {
                var uncle = x.Parent!.Parent!.Left;

                // case2.1 uncle is red node
                if (RBNode<T>.GetColor(uncle) == 'R')
                {
                    x.Parent!.Color = 'B';
                    uncle!.Color = 'B';
                    x.Parent!.Parent!.Color = 'R';
                    x = x.Parent!.Parent!;
                }
                else
                {
                    // case2.2 x is a left child of its parent
                    if (x == x.Parent!.Left)
                    {
                        rightRotate(x.Parent!);
                        x = x.Right!;
                    }

                    // case 2.3 x is a right child of its parent
                    leftRotate(x.Parent!.Parent!);
                    x.Parent!.Color = 'B';
                    x.Parent!.Left!.Color = 'R';
                }
            }
        }

        root!.Color = 'B';
    }

    public bool IsRedBlackTree()
    {
        if (root is null) return true;
        (bool isValid, int height, T minVal, T maxVal) = getTreeState(root);
        return isValid && root.Color == 'B';
    }

    private void transplant(RBNode<T> u, RBNode<T>? v)
    {
        var parent = u.Parent;
        if (parent is null)
        {
            root = v;
        }
        else if (parent.Left == u)
        {
            parent.Left = v;
        }
        else
        {
            parent.Right = v;
        }

        if (v is not null)
            v.Parent = parent;
    }

    private void remove(RBNode<T> z)
    {
        RBNode<T> y = z;
        RBNode<T>? x = null;
        RBNode<T>? p = null;
        char originalYColor = 'U'; //unknown

        // case 1: right tree of z is null
        if (z.Right is null)
        {
            p = z.Parent;
            x = z.Left;
            originalYColor = z.Color;
            transplant(z, z.Left);

        }
        // case 2: left tree of z is null
        else if (z.Left is null)
        {
            p = z.Parent;
            x = z.Right;
            originalYColor = z.Color;
            transplant(z, z.Right);
        }
        // case3: z's successor is its right child
        else if (z.Right.Left is null)
        {
            y = z.Right;
            p = y;
            x = y.Right;
            originalYColor = y.Color;
            y.Color = z.Color;
            y.Left = z.Left;
            z.Left.Parent = y;
            transplant(z, y);
        }
        // case4: z's successor is not its right child
        else
        {
            // find successor of z
            y = z.Right;
            while (y.Left is not null)
                y = y.Left;

            originalYColor = y.Color;
            x = y.Right;
            p = y.Parent;
            transplant(y, x);
            y.Right = z.Right;
            z.Right.Parent = y;
            y.Color = z.Color;

            y.Left = z.Left;
            z.Left.Parent = y;
            transplant(z, y);
        }

        // when y's orignal color is black, its color is
        // inherit to x one must fix the tree
        if (originalYColor == 'B')
            removeFixUp(p, x);
    }

    void removeFixUp(RBNode<T>? p, RBNode<T>? x)
    {
        while (RBNode<T>.GetColor(x) == 'B' && root != x)
        {
            // case I: x is the left child of its parent
            if (p!.Left == x)
            {
                // a) p' right child is red (p must be black in this case)
                var w = p.Right;
                if (RBNode<T>.GetColor(w) == 'R')
                {
                    // change color of w and p
                    w!.Color = 'B';
                    p.Color = 'R';
                    leftRotate(p);
                    w = p.Right;
                }

                //now w must be black
                // b) both child of w is black
                if (RBNode<T>.GetColor(w!.Left) == 'B' && RBNode<T>.GetColor(w.Right) == 'B')
                {
                    w.Color = 'R';
                    x = p;
                    p = p.Parent;
                }
                else
                {
                    // c) w' left child if red
                    if (RBNode<T>.GetColor(w.Left) == 'R')
                    {
                        w.Color = 'R';
                        w.Left!.Color = 'B';
                        rightRotate(w);
                        w = p.Right;
                    }

                    // d) w's right child is red
                    w!.Color = p.Color;
                    w.Right!.Color = 'B';
                    p.Color = 'B';
                    leftRotate(p);
                    x = root;
                }

            }
            // case II: x is right child of its parent
            else
            {
                // case a): x's sibling is 'r'
                var w = p.Left;
                if (RBNode<T>.GetColor(w) == 'R')
                {
                    w!.Color = 'B';
                    p.Color = 'R';
                    rightRotate(p);
                    w = p.Left;
                }

                // case b): both child of w is black
                if (RBNode<T>.GetColor(w!.Left) == 'B' && RBNode<T>.GetColor(w.Right) == 'B')
                {
                    w.Color = 'R';
                    x = p;
                    p = p.Parent;
                }
                else
                {
                    // case c): right child of w is red
                    if (RBNode<T>.GetColor(w.Right) == 'R')
                    {
                        w.Color = 'R';
                        w.Right!.Color = 'B';
                        leftRotate(w);
                        w = p.Left;
                    }

                    // case 3): left child of w is red
                    w!.Color = p.Color;
                    p.Color = 'B';
                    w.Left!.Color = 'B';
                    rightRotate(p);
                    x = root;
                }
            }
        }

        if (x is not null)
            x.Color = 'B';
    }

    // tuple: (isvalid, black height, minval, maxval)
    // root color is not checked
    private (bool, int, T, T) getTreeState(RBNode<T> node)
    {
        if (node.Left is null && node.Right is null)
        {
            return (true, node.Color == 'B' ? 1 : 0, node.Val, node.Val);
        }
        else if (node.Right is null)
        {
            (bool isLeftValid, int lHeight, T lMin, T lMax) = getTreeState(node.Left!);
            bool isValid = isLeftValid && lHeight == 0 && lMax.CompareTo(node.Val) <= 0;
            int height = lHeight + (node.Color == 'B' ? 1 : 0);
            T minVal = node.Val;
            if (minVal.CompareTo(lMin) > 0) minVal = lMin;

            T maxVal = node.Val;
            if (maxVal.CompareTo(lMax) < 0) maxVal = lMax;
            return (isValid, height, minVal, maxVal);

        }
        else if (node.Left is null)
        {
            (bool isRightValid, int rHeight, T rMin, T rMax) = getTreeState(node.Right!);
            bool isValid = isRightValid && rHeight == 0 && rMin.CompareTo(node.Val) > 0;
            int height = rHeight + (node.Color == 'B' ? 1 : 0);
            T minVal = node.Val;
            if (minVal.CompareTo(rMin) > 0) minVal = rMin;

            T maxVal = node.Val;
            if (maxVal.CompareTo(rMax) < 0) maxVal = rMax;
            return (isValid, height, minVal, maxVal);
        }
        else
        {
            (bool isLeftValid, int lHeight, T lMin, T lMax) = getTreeState(node.Left!);
            (bool isRightValid, int rHeight, T rMin, T rMax) = getTreeState(node.Right!);

            // both child is valid
            bool isValid = isLeftValid && isRightValid;

            // lheight == rheight
            isValid = isValid && lHeight == rHeight;

            // left max <= val < right min
            isValid = isValid && lMax.CompareTo(node.Val) <= 0 && node.Val.CompareTo(rMin) < 0;

            // height of current tree
            int height = Math.Max(lHeight, rHeight) + (node.Color == 'B' ? 1 : 0);

            T minVal = node.Val;
            if (minVal.CompareTo(lMin) > 0) minVal = lMin;
            if (minVal.CompareTo(rMin) > 0) minVal = rMin;

            T maxVal = node.Val;
            if (maxVal.CompareTo(lMax) < 0) maxVal = lMax;
            if (maxVal.CompareTo(rMax) < 0) maxVal = rMax;

            return (isValid, height, minVal, maxVal);
        }
    }

    private void leftRotate(RBNode<T> x)
    {
        var parent = x.Parent;
        RBNode<T> y = x.Right!;

        var beta = y.Left;
        x.Right = beta;
        if (beta is not null)
            beta.Parent = x;

        y.Left = x;
        x.Parent = y;

        // link parent to y
        if (parent is null)
            root = y;
        else if (parent.Left == x)
            parent.Left = y;
        else
            parent.Right = y;
        y.Parent = parent;
    }

    private void rightRotate(RBNode<T> x)
    {
        var parent = x.Parent;
        RBNode<T> y = x.Left!;

        var beta = y.Right;
        x.Left = beta;
        if (beta is not null) beta.Parent = x;

        y.Right = x;
        x.Parent = y;

        // link parent to y
        if (parent is null)
            root = y;
        else if (parent.Left == x)
            parent.Left = y;
        else
            parent.Right = y;
        y.Parent = parent;
    }

    private RBNode<T>? root;
}


// node of read black tree
class RBNode<T> where T : IComparable<T>
{
    public RBNode(T elem, char color)
    {
        Val = elem;
        Color = color;
    }

    public static char GetColor(RBNode<T>? node)
    {
        if (node is null) return 'B';
        return node.Color;
    }

    public char Color;
    public T Val;
    public RBNode<T>? Left;
    public RBNode<T>? Right;
    public RBNode<T>? Parent;
}

class RBTreeEnumerator<T> : IEnumerator<T> where T : IComparable<T>
{
    public RBTreeEnumerator(RBNode<T>? root)
    {
        this.root = root;
        curr = null;
    }

    public T Current
    {
        get
        {
            if (curr is null) throw new InvalidOperationException("deference null pointer");
            return curr.Val;
        }
    }

    object IEnumerator.Current
    {
        get => this.Current;
    }

    public bool MoveNext()
    {
        // special case: root is empty
        if (root is null) return false;

        // special case: when curr not initalized
        if (curr is null)
        {
            curr = root;
            while (curr.Left is not null)
            {
                curr = curr.Left;
            }
        }
        else
        {
            // when curr's right tree is not empty
            if (curr.Right is not null)
            {
                curr = curr.Right;
                while (curr.Left is not null)
                    curr = curr.Left;
            }
            // curr's right tree is empty
            else
            {
                RBNode<T>? parent = curr.Parent;
                while (parent is not null && parent.Right == curr)
                {
                    curr = parent;
                    parent = parent.Parent;
                }
                curr = parent;
            }
        }

        return curr is not null;
    }

    public void Reset()
    {
        curr = null;
    }

    void IDisposable.Dispose() { }

    private RBNode<T>? root;
    private RBNode<T>? curr;
}