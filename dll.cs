using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Xml.Linq;
using System.DirectoryServices.ActiveDirectory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Zipper
{
    // Node class for Doubly Linked List storing byte Key and Frequency
    public class Node
    {
        public byte Key;          // Stores the byte key (e.g., the actual character or data byte)
        public uint Frequency;     // Stores the frequency of the byte (how many times this byte appears)
        public Node Next;         // Points to the next node in the doubly linked list
        public Node Previous;     // Points to the previous node in the doubly linked list
        public Node Left;         // Points to the left node, typically used for the tree structure (Huffman tree)
        public Node Right;        // Points to the right node, typically used for the tree structure (Huffman tree)

        // Constructor to initialize a Node with a given key and frequency
        public Node(byte key, uint frequency)
        {
            Key = key;  // Set the byte key
            Frequency = frequency;  // Set the frequency of the byte
            Next = null;  // Initially, no next node
            Previous = null;  // Initially, no previous node
            Left = null;  // Initially, no left child (used in Huffman tree)
            Right = null;  // Initially, no right child (used in Huffman tree)
        }
    }

    // Doubly Linked List class for byte Key-Frequency pairs
    public class DoublyLinkedList
    {
        //props****************************************************************
        public Node Head;  // Points to the head (first node) of the list
        public Node Tail;  // Points to the tail (last node) of the list

        //constructor**********************************************************
        public DoublyLinkedList()
        {
            Head = null;  // Initially, no head node
            Tail = null;  // Initially, no tail node
        }

        //methods**************************************************************


        /// <summary>
        /// Add a new node into the doubly linked list asc based on frequency
        /// </summary>
        /// <algo>
        /// put a node with a certain freq before a node with the same freq
        /// </algo>
        public void AddInSequence(Node n)
        {
            if (Head == null)
            {
                Head = n;
                Tail = n;
            }
            else if (n.Frequency <= Head.Frequency)
            {
                n.Next = Head;
                Head.Previous = n;
                Head = n;
            }
            else if (n.Frequency > Tail.Frequency)
            {
                n.Previous = Tail;
                Tail.Next = n;
                Tail = n;
            }
            else
            {
                Node c = Head;
                while (n.Frequency > c.Frequency) c = c.Next;
                c.Previous.Next = n;
                n.Previous = c.Previous;
                n.Next = c;
                c.Previous = n;
            }
        }
    }

    /// <summary>
    ///     return byte[] by translating the file (converting each byte by it bitpattern in the table
    /// </summary>
    /// <algo>
    /// Iterate bytewise throug the file adding the new bitvalue from the table to a string.
    /// determine extra bits such that we get a stringlength which is a multiple of 8
    /// convert string to byte[]
    /// </algo>
    public class stats
    {
        internal static byte[] translate(byte[] f, Dictionary<byte, string> table)
        {
            int x = 0;
            string t = "";
            for (int i = 0; i < f.Length; i++)  t += table[f[i]];
            x = 8 - t.Length % 8;
            for (int i = 0; i < x; i++) t += "0";
            

            int byteCount = t.Length / 8;
            byte[] bArr = new byte[byteCount+1];
            for (int i = 0; i < byteCount; i++)
            {
                bArr[i] =convert8bB(t.Substring(i * 8, 8));
            }
            bArr[byteCount] = (byte)x;
            return bArr;
        }

        private static byte convert8bB(string s)
        {
            int cnt = 0, w = 128;
            for (int b = 0; b < s.Length; b++)
            {
                if (s[b] == '1') cnt +=w;
                w/= 2;
            }
            return (byte)cnt;
        }


        /// <summary>
        ///     return how often each byte occurs in file
        /// </summary>
        internal static uint[] countEachByte(byte[] file)
        {
            uint[] f = new uint[256];
            for (int i = 0; i < file.Length; i++) f[file[i]]++;
            return f;
        }

        /// <summary>
        ///     return the head of a doubly linked list asc on freq
        /// </summary>
        internal static DoublyLinkedList makeDLL(uint[] freq)
        {
            DoublyLinkedList L = new DoublyLinkedList();
            for (int i = 0; i < freq.Length; i++)
            {
                if (freq[i] != 0)
                {
                    Node n = new Node((byte)i, freq[i]);
                    L.AddInSequence(n);
                }
            }
            return L;
        }

  


        // The Huffman table is generated by traversing through the dll nodes from the tail.
        // The table it self is a dictionary with the byte value and a bitcode.
        // First we decent the left nodes until the nodes left and right node are null.
        // We decent the right node until it's left and right node are null.
        // We've reached a leaf node put the byte value and the string in a dictionary.
        // If the tail is null return to the previous node.
        // For every left decent we add a 1 to a string.
        // For every rigth decent we add 0 to a string.
        internal static void BuildTableRecursive(Node tail, string code, Dictionary<byte, string> huffmanTable)
        {
            if (tail == null) return;

            // Leaf node
            if (tail.Left == null && tail.Right == null)
            {
                huffmanTable[tail.Key] = code;
            }

            BuildTableRecursive(tail.Left, code + "1", huffmanTable);
            BuildTableRecursive(tail.Right, code + "0", huffmanTable);

        }
        /// <summary>
        ///     return string that saves the tree structure with the byte values (not the freq)
        /// </summary>
        /// <algo>
        ///     step through the tree from the top down.
        ///     assign a 0 for a non-leaf and then go down in the order left right
        ///     and a 1 for a leaf node + the normal bitcode of the byte of that node
        /// </algo>
        public static void rsaveTree(Node n, string s)
        {
            // Leaf node
            if (n.Left == null)
            {
                s += '1';
                s += convertBt8b(n.Key);
            }
            else    //non-leaf
            {
                s += '0';
                rsaveTree(n.Left, s);
                rsaveTree(n.Right, s);
            }
        }

        /// <summary>
        /// convert byte to 8 bits
        /// </summary>
        private static string convertBt8b(byte key)
        {
            return "x";
        }

        public static Dictionary<byte, string> BuildHuffmanTable(Node root)
        {
            var huffmanTable = new Dictionary<byte, string>();
            BuildTableRecursive(root, "", huffmanTable);
            return huffmanTable;
        }

        /// <summary>
        ///     Assign lower nodes from the next and previous current node. 
        ///     Add the new node in sequence traverse nodes.
        /// </summary>
        internal static DoublyLinkedList makeDLLHuffman(DoublyLinkedList L)
        {
            Node current = L.Head;
            var newNode = new Node(55, current.Frequency + current.Next.Frequency);
            while (current.Next != null)
            {

                newNode = new Node(55, current.Frequency + current.Next.Frequency);
                newNode.Left = current;
                newNode.Right = current.Next;
                L.AddInSequence(newNode);
                current = current.Next.Next;

            }
            return L;

        }
        /// <summary>
        /// This method creates a binary tree (typically Huffman tree)
        /// by combining nodes based on their frequencies
        /// </summary>
        /*internal static Node CreateTree(Node Head)
        {
            DoublyLinkedList L = new DoublyLinkedList();
            // Traverse the list, merging nodes to form the Huffman tree
            Node current = Head;
            while (current != null && current.Next != null)
            {
                // Create a new node that combines the current node and the next node's frequencies
                Node newNode = new Node(55, current.Frequency + current.Next.Frequency);
                newNode.Left = current;  // The left child of the new node is the current node
                newNode.Right = current.Next;  // The right child of the new node is the next node

                // Insert the new combined node back into the list (sorted by frequency)
                L.AddInSequence(newNode);

                // Move the pointer 'current' forward by two steps (skip the next node because it's already merged)
                current = current.Next.Next;  // Move two steps to continue combining the next two nodes
            }
            return L.Tail;
        }*/

        /// <summary>
        /// read a file as byte[]
        /// </summary>
        internal static byte[] readFileAsBytes()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return File.ReadAllBytes(ofd.FileName);
            }
            return null;
        }
        /// <summary>
        /// return string that saves the tree structure with the byte values (not the freq)
        /// </summary>
        internal static string saveTree(Node tail)
        {
            string s = "";
            rsaveTree(tail, s);
            return s;
        }
    }

}

