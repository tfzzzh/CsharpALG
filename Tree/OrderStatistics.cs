namespace CsharpAlG.Tree;

/// <summary>
/// Given a set of distinct elements {num_1, num_2 .. num_n}, find
/// the rank of any of its elements in O(log n). Given the rank,
/// return the number with that rank in O(log n)
/// </summary>
public class OrderStatistics<T> where T : IComparable<T>
{
    public OrderStatistics() {root = null;}

    public void Insert(T elem)
    {
        // new a node for elem
        var newNode = new NodeWithSize<T>(elem) {Color='R', Size=1};

        // special case: tree is empty
        if (root is null)
        {
            root = newNode;
            root.Color = 'B';
            return;
        }

        // find a position to insert
        NodeWithSize<T>? parent;
        NodeWithSize<T>? ptr;
        find(elem, out parent, out ptr);

        // when ptr is not null
        if (ptr is not null) throw new InvalidDataException($"duplicate key ${elem}");

        // insert elem as a child of parent
        if (elem.CompareTo(parent!.Val) < 0)
        {
            parent.Left = newNode;
        }
        // elem > parent.Val
        else
        {
            parent.Right = newNode;
        }
        newNode.Parent = parent;

        // propogate size to the root
        propogate(newNode);

        // fix colors
        insertFixUp(newNode);
    }

    public void Remove(T elem)
    {
        // sepcial case: when tree is empty  one shall not call remove
        if (root is null) throw new InvalidOperationException("remove value in an empty tree");

        // find the node contains elem
        NodeWithSize<T>? parent;
        NodeWithSize<T>? ptr;
        find(elem, out parent, out ptr);
        if (ptr is null) throw new KeyNotFoundException($"key {elem} not found in the tree");

        // remove the node
        remove(ptr);
    }

    public int Count {
        get
        {
            if (root is null) return 0;
            return root.Size;
        }
    }

    /// <summary>
    /// Given the rank of an element return the element
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    public T Select(int rank)
    {
        if (root is null) throw new InvalidDataException("tree is empty");
        if (rank <= 0 || rank >= root.Size+1) throw new IndexOutOfRangeException($"rank shall in [1,{root.Size}]");
        return select(root, rank);
    }

    /// <summary>
    /// Given an element in the set, return the rank of this element
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    public int GetRank(T elem)
    {
        // find node contains elem
        NodeWithSize<T>? parent;
        NodeWithSize<T>? ptr;
        find(elem, out parent, out ptr);
        if (ptr is null) throw new KeyNotFoundException($"key {elem} not found in the tree");

        return getRank(ptr);
    }

    private int getRank(NodeWithSize<T> node)
    {
        int rank = NodeWithSize<T>.GetSize(node.Left) + 1;
        NodeWithSize<T>?  parent = node.Parent;
        while (parent is not null)
        {
            if (parent.Val.CompareTo(node.Val) < 0)
                rank += NodeWithSize<T>.GetSize(parent.Left) + 1;
            parent = parent.Parent;
        }
        return rank;
    }

    private T select(NodeWithSize<T> node, int rank)
    {
        // get rank of current node
        int rank_node = NodeWithSize<T>.GetSize(node.Left) + 1;

        if (rank_node == rank) return node.Val;
        if (rank < rank_node)
        {
            return select(node.Left!, rank);
        }
        // rank > rank_node, one shall seek right tree's rank - rank_node element
        else
        {
            return select(node.Right!, rank - rank_node);
        }
    }

    void insertFixUp(NodeWithSize<T> x)
    {
        while (NodeWithSize<T>.GetColor(x.Parent) == 'R')
        {
            // case I: x.Parent is left child of its grand parent
            if (x.Parent!.Parent!.Left == x.Parent)
            {
                // a) x's uncle is red
                var uncle = x.Parent.Parent.Right;
                if (NodeWithSize<T>.GetColor(uncle) == 'R')
                {
                    x.Parent.Color = 'B';
                    uncle!.Color = 'B';
                    x.Parent.Parent.Color = 'R';
                    x = x.Parent.Parent;
                }
                else
                {
                    // b) x is its parent's right child
                    if (x.Parent.Right == x)
                    {
                        x = x.Parent;
                        leftRotate(x);
                    }

                    // c) x is it parent's left child
                    x.Parent!.Color = 'B';
                    x.Parent.Parent!.Color = 'R';
                    rightRotate(x.Parent.Parent);
                }
            }
            // case II: x.Parent is right child of its grand parent
            else
            {
                // a) x's uncle is red
                var uncle = x.Parent.Parent.Left;
                if (NodeWithSize<T>.GetColor(uncle) == 'R')
                {
                    x.Parent.Color = 'B';
                    uncle!.Color = 'B';
                    x.Parent.Parent.Color = 'R';
                    x = x.Parent.Parent;
                }
                else
                {
                    // b) x is it parent's left child
                    if (x.Parent.Left == x)
                    {
                        x = x.Parent;
                        rightRotate(x);
                    }
                    // c) x is it parent's left child
                    else
                    {
                        x.Parent.Color = 'B';
                        x.Parent.Parent.Color = 'R';
                        leftRotate(x.Parent.Parent);
                    }
                }
            }
        }
        root!.Color = 'B';
    }

    private void remove(NodeWithSize<T> z)
    {
        // node whose color is pushed to other node
        NodeWithSize<T> y;
        char yOriginalColor;
        // node who get y's original color
        NodeWithSize<T>? x;
        NodeWithSize<T>? parentX;

        // case I: z.Right is null
        if (z.Right is null)
        {
            y = z;
            yOriginalColor = NodeWithSize<T>.GetColor(y);
            x = y.Left;
            parentX = y.Parent;
            transplant(y, x);

            if (parentX is not null) recomputeAndPropogateTo(parentX, root!);
        }
        // case II: z.Left is null
        else if (z.Left is null)
        {
            y = z;
            yOriginalColor = NodeWithSize<T>.GetColor(y);
            x = y.Right;
            parentX = y.Parent;
            transplant(y, x);

            if (parentX is not null) recomputeAndPropogateTo(parentX, root!);
        }
        // case III: z's successor is exactly its right child
        else if (z.Right.Left is null)
        {
            y = z.Right;
            yOriginalColor = y.Color;
            x = y.Right;
            parentX = y;

            y.Color = z.Color;
            y.Left = z.Left;
            if (z.Left is not null) z.Left.Parent = y;

            transplant(z, y);

            recomputeAndPropogateTo(y, root!);
        }
        // case IV: z's successor is not its right child
        else
        {
            // set y to the successor of z
            y = z.Right;
            while (y.Left is not null) y = y.Left;

            yOriginalColor = y.Color;
            x = y.Right;
            parentX = y.Parent;
            transplant(y, x);
            recomputeAndPropogateTo(parentX!, z.Right);

            y.Left = z.Left;
            if (z.Left is not null) z.Left.Parent = y;
            y.Right = z.Right;
            z.Right.Parent = y;
            y.Color =  z.Color;

            transplant(z, y);
            recomputeAndPropogateTo(y, root!);
        }

        // fix color when y's original color is 'B'
        if (yOriginalColor == 'B')
            removeFixUp(parentX, x);
    }

    void removeFixUp(NodeWithSize<T>? parentX, NodeWithSize<T>? x)
    {
        while (NodeWithSize<T>.GetColor(x) ==  'B' && x != root)
        {
            // case I  x is its parent's left child
            if (x == parentX!.Left)
            {
                // a) x's sibling is red
                var w = parentX.Right;
                if (NodeWithSize<T>.GetColor(w) == 'R')
                {
                    w!.Color  =  'B';
                    parentX.Color = 'R';
                    leftRotate(parentX);
                    w = parentX.Right;
                }
                // b) x's sibling is black, and both child of the sibling is black
                if (NodeWithSize<T>.GetColor(w!.Left) == 'B' && NodeWithSize<T>.GetColor(w!.Right) == 'B')
                {
                    w.Color = 'R';
                    x = parentX;
                    parentX  = x.Parent;
                }
                // c) x's sibling's left child is red
                else if (NodeWithSize<T>.GetColor(w!.Left) == 'R')
                {
                    w.Left!.Color = 'B';
                    w.Color = 'R';
                    rightRotate(w);
                }
                // d) x's sibling's right child is red
                else
                {
                    w.Color = parentX.Color;
                    parentX.Color = 'B';
                    w.Right!.Color = 'B';
                    leftRotate(parentX);
                    x = root;
                }
            }
            // case II x is its parent's right child
            else
            {
                // a) x's sibling is red
                var w = parentX.Left;
                if (NodeWithSize<T>.GetColor(w) == 'R')
                {
                    w!.Color  =  'B';
                    parentX.Color = 'R';
                    rightRotate(parentX);
                    w = parentX.Left;
                }
                // b) x's sibling is black, and both child of the sibling is black
                if (NodeWithSize<T>.GetColor(w!.Left) == 'B' && NodeWithSize<T>.GetColor(w!.Right) == 'B')
                {
                    w.Color = 'R';
                    x = parentX;
                    parentX  = x.Parent;
                }
                // c) x's sibling's right child is red
                else if (NodeWithSize<T>.GetColor(w!.Right) == 'R')
                {
                    w.Right!.Color = 'B';
                    w.Color = 'R';
                    leftRotate(w);
                }
                // d) x's sibling's left child is red
                else
                {
                    w.Color = parentX.Color;
                    parentX.Color = 'B';
                    w.Left!.Color = 'B';
                    rightRotate(parentX);
                    x = root;
                }
            }
        }

        if (x is not null) x.Color = 'B';
    }

    private void leftRotate(NodeWithSize<T> x)
    {
        NodeWithSize<T>? parent = x.Parent;
        NodeWithSize<T> y = x.Right!;
        NodeWithSize<T>? beta = y.Left;

        y.Left = x;
        x.Parent = y;
        y.Size = x.Size;

        x.Right = beta;
        if (beta is not null)
            beta.Parent = x;
        x.Size = NodeWithSize<T>.GetSize(x.Left) +
            NodeWithSize<T>.GetSize(x.Right) + 1;

        if (parent is null)
        {
            root = y;
        }
        else if (parent.Left == x)
        {
            parent.Left = y;
        }
        else
        {
            parent.Right = y;
        }
        y.Parent = parent;
    }

    private void rightRotate(NodeWithSize<T> x)
    {
        NodeWithSize<T>? parent = x.Parent;
        NodeWithSize<T> y = x.Left!;
        NodeWithSize<T>? beta = y.Right;

        y.Right = x;
        x.Parent = y;
        y.Size = x.Size;

        x.Left = beta;
        if (beta is not null)
            beta.Parent = x;
        x.Size = NodeWithSize<T>.GetSize(x.Left) +
            NodeWithSize<T>.GetSize(x.Right) + 1;

        if (parent is null)
        {
            root = y;
        }
        else if (parent.Left == x)
        {
            parent.Left = y;
        }
        else
        {
            parent.Right = y;
        }

        y.Parent = parent;
    }

    // size in curr have been update
    // propogate the size change of curr up to the root
    private void propogate(NodeWithSize<T> curr)
    {
        NodeWithSize<T>? itr = curr.Parent;
        while (itr is not null)
        {
            itr.Size = NodeWithSize<T>.GetSize(itr.Left) +
                NodeWithSize<T>.GetSize(itr.Right) + 1;
            itr = itr.Parent;
        }
    }


    /// <summary>
    /// find a node whose key is elem. The node and its parents are returned
    /// in the out parameters
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="parent"></param>
    /// <param name="ptr">when elem exsit ptr is its node, else null is returned</param>
    private void find(T elem, out NodeWithSize<T>? parent, out NodeWithSize<T>? ptr)
    {
        NodeWithSize<T>? prev = null;
        NodeWithSize<T>? curr = root;

        while (curr is not null)
        {
            // found
            if (elem.CompareTo(curr.Val) == 0) break;

            // elem < curr.Val search left tree
            if (elem.CompareTo(curr.Val) < 0)
            {
                prev = curr;
                curr = curr.Left;
            }
            // elem > curr.Val search right tree
            else
            {
                prev = curr;
                curr = curr.Right;
            }
        }

        parent = prev;
        ptr = curr;
    }

    public bool IsRedBlackTree()
    {
        if (root is null) return true;
        (bool isValid, int height, T minVal, T maxVal) = getTreeState(root);
        return isValid && root.Color == 'B';
    }

    private (bool, int, T, T) getTreeState(NodeWithSize<T> node)
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

    private void transplant(NodeWithSize<T> u, NodeWithSize<T>? v)
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

    // recompute the size of start node and propogate the result to end
    void recomputeAndPropogateTo(NodeWithSize<T> start, NodeWithSize<T> end)
    {
        NodeWithSize<T>? ptr = start;
        while (ptr != end.Parent)
        {
            ptr!.Size = NodeWithSize<T>.GetSize(ptr.Left) + NodeWithSize<T>.GetSize(ptr.Right) + 1;
            ptr = ptr!.Parent;
        }
    }

    private NodeWithSize<T>? root;
}

class NodeWithSize<T> where T : IComparable<T>
{
    public NodeWithSize(T elem)
    {
        Val = elem;
    }

    public static char GetColor(NodeWithSize<T>? node)
    {
        if (node is null) return 'B';
        return node.Color;
    }

    public static int GetSize(NodeWithSize<T>? node)
    {
        if (node is null) return 0;
        return node.Size;
    }

    public char Color;
    public T Val;
    public int Size; // size of the subtree
    public NodeWithSize<T>? Left;
    public NodeWithSize<T>? Right;
    public NodeWithSize<T>? Parent;
}