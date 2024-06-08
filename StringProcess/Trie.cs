using System.Diagnostics;
using System.Text;

namespace CsharpALG.StringProcess;
public class Trie
{
    public  Trie() {root = null;}

    public void Add(string word)
    {
        if (root is null)
            root = new TrieNode();

        if (word.Length == 0)
        {
            root.IsWord = true;
            return;
        }

        TrieNode curr = root;
        foreach(char c in word)
        {
            if (!(c >= 'a' && c <= 'z'))
                throw new InvalidDataException("only lower case char is supported");

            TrieNode? next = curr.Next[c-'a'];
            if (next is null)
            {
                next = new TrieNode();
                curr.Next[c-'a'] = next;
            }
            curr = next;
        }
        curr.IsWord = true;
    }

    public bool Contains(string word)
    {
        if (root is null) return false;

        TrieNode curr = root;
        foreach(char c in word)
        {
            if (!(c >= 'a' && c <= 'z'))
                throw new InvalidDataException("only lower case char is supported");

            TrieNode? next = curr.Next[c-'a'];
            if (next is null) return false;

            curr = next;
        }

        return curr.IsWord;
    }

    public bool Remove(string word)
    {
        if (!Contains(word)) return false;
        remove(root!, word, 0);
        return true;
    }

    public List<string> Keys
    {
        get
        {
            var keys = new List<string>();
            if (root is not null)
            {
                StringBuilder prefix = new StringBuilder();
                getKeys(root, prefix, keys);

            }
            return keys;
        }
    }

    void getKeys(TrieNode curr, StringBuilder prefix, List<string> keys)
    {
        if (curr.IsWord)
        {
            keys.Add(prefix.ToString());
        }

        for(int cid = 0; cid < 26; ++cid)
        {
            if (curr.Next[cid] is not null)
            {
                prefix.Append((char) ('a' + cid));
                getKeys(curr.Next[cid]!, prefix, keys);
                prefix.Remove(prefix.Length - 1, 1);
            }
        }
    }

    TrieNode? remove(TrieNode curr, string word, int i)
    {
        bool hasChild;
        if (i == word.Length)
        {
            Debug.Assert(curr.IsWord);
            curr.IsWord = false;
            hasChild = false;
            for (int cid=0; cid < 26 && !hasChild; ++cid)
            {
                hasChild = curr.Next[cid] is not null;
            }
            if (hasChild)
            {
                return curr;
            }
            else
            {
                return null;
            }
        }

        TrieNode? next = curr.Next[word[i] - 'a'];
        Debug.Assert(next is not null);
        curr.Next[word[i] - 'a'] = remove(next, word, i+1);

        hasChild = false;
        for (int cid=0; cid < 26 && !hasChild; ++cid)
        {
            hasChild = curr.Next[cid] is not null;
        }

        if (hasChild) return curr;
        return null;
    }

    private TrieNode? root;
}


class TrieNode
{
    public TrieNode()
    {
        IsWord = false;
        Next = new TrieNode?[26];
        Array.Fill(Next, null);
    }

    public bool IsWord;
    public TrieNode?[] Next;
}