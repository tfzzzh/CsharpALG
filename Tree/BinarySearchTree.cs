using System.Collections;
using System.Collections.Generic;

namespace CsharpAlG.Tree;

public class BinarySearchTree<T> : IEnumerable<T>
    where T : IComparable<T>
{
    public BinarySearchTree() { root = null; }

    public void Insert(T elem)
    {
        // special case: when tree is empty use elem as root
        if (root is null)
        {
            root = new Node(elem);
            return;
        }

        // find a position to insert elem
        Node? prev = null;
        Node? curr = root;
        while (curr is not null)
        {
            prev = curr;
            if (elem.CompareTo(curr.Val) <= 0)
            {
                curr = curr.Left;
            }
            else
            {
                curr = curr.Right;
            }
        }

        // insert it into the tree
        var newNode = new Node(elem) { Parent = prev };
        if (elem.CompareTo(prev!.Val) <= 0)
        {
            prev.Left = newNode;
        }
        else
        {
            prev.Right = newNode;
        }
    }

    // find node with value equal to elem. Return null
    // when elem not find
    public Node? Find(T elem)
    {
        Node? curr = root;
        while (curr is not null)
        {
            if (elem.CompareTo(curr.Val) < 0)
                curr = curr.Left;

            else if (elem.CompareTo(curr.Val) > 0)
                curr = curr.Right;

            else
                break;
        }

        return curr;
    }

    public bool Remove(T elem)
    {
        Node? node = Find(elem);
        if (node is null) return false;
        remove(node);
        return true;
    }

    public void InorderTreeWalk(Action<Node> action)
    {
        inorderTreeWalk(root, action);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new TreeEnumerator<T>(this.root);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    private static void inorderTreeWalk(Node? root, Action<Node> action)
    {
        if (root is null) return;
        inorderTreeWalk(root.Left, action);
        action(root);
        inorderTreeWalk(root.Right, action);
    }

    private void remove(Node u)
    {
        // node u does not have left subtree
        if (u.Left is null)
            transplant(u, u.Right);

        // node u does not have right subtree
        else if (u.Right is null)
            transplant(u, u.Left);

        // both left and right child of u is not null
        else
        {
            Node right = u.Right;
            // if right has precessor
            if (right.Left is not null)
            {
                // find precessor of right
                Node prev = right.Left;
                while (prev.Right is not null)
                {
                    prev = prev.Right;
                }

                // now prev.right is null use its
                // left tree to occupy its position
                transplant(prev, prev.Left);

                prev.Right = right;
                right.Parent = prev;
                right = prev;
            }

            // at this point we make right.Left invalid
            right.Left = u.Left;
            u.Left.Parent = right;

            // make right occupy u's position
            transplant(u, right);
        }

        // make u unreachable to and from
        u.Left = null;
        u.Right = null;
    }

    // transplant subtree rooted at v to position u
    // after transplant u becomes unreachable
    private void transplant(Node u, Node? v)
    {
        Node? parent = u.Parent;

        // u is the root
        if (parent is null)
            root = v;

        // u's the left child of its parent
        else if (ReferenceEquals(parent.Left, u))
            parent.Left = v;

        else
            parent.Right = v;

        if (v is not null)
            v.Parent = parent;
    }

    // root of the tree
    private Node? root;

    // Node class of the tree
    public class Node
    {
        public Node(T val) { Val = val; }
        public T Val;
        public Node? Left;
        public Node? Right;
        public Node? Parent;
    }
    // iterator class of the tree
}


public class TreeEnumerator<T> : IEnumerator<T> where T: IComparable<T>
{
    private BinarySearchTree<T>.Node? current;
    private BinarySearchTree<T>.Node? root;

    public TreeEnumerator(BinarySearchTree<T>.Node? ptr)
    {
        root = ptr;
    }

    public T Current
    {
        get
        {
            if (current is null)
                throw new InvalidOperationException("iterator not initialized correctly");
            return current.Val;
        }
    }

    object IEnumerator.Current
    {
        get { return this.Current; }
    }

    public bool MoveNext()
    {
        if (current is null)
        {
            current = root;
            if (current is not null)
                while (current.Left is not null)
                    current = current.Left;
        }
        else
        {
            if (current.Right is not null)
            {
                current = current.Right;
                while (current.Left is not null)
                    current = current.Left;
            }
            else
            {
                BinarySearchTree<T>.Node? parent = current.Parent;
                while (parent is not null && parent.Right == current)
                {
                    current = parent;
                    parent = current.Parent;
                }
                current = parent;
            }
        }
        return current is not null;
    }

    public void Reset()
    {
        current = null;
    }


    void IDisposable.Dispose() {}
}