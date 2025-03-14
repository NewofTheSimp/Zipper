using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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

    public class stats
    {
        /// <summary>
        ///     return byte[] by translating the file (converting each byte by it bitpattern in the table
        /// </summary>
        /// <algo>
        /// Iterate bytewise throug the file adding the new bitvalue from the table to a string.
        /// determine extra bits such that we get a stringlength which is a multiple of 8
        /// convert string to byte[]
        /// </algo>
        internal static byte[] translate(byte[] f, Dictionary<byte, string> table)
        {
            string bitString = "";
            for (int i = 0; i < f.Length; i++)
            {
                bitString += table[f[i]];
            }

            // Add padding bits to make the length a multiple of 8
            int paddingBits = 8 - bitString.Length % 8;
            if (paddingBits != 8)
            {
                bitString += new string('0', paddingBits);
            }

            // Convert the bit string to a byte array
            int byteCount = bitString.Length / 8;
            byte[] bArr = new byte[byteCount + 1];
            for (int i = 0; i < byteCount; i++)
            {
                bArr[i] = convert8bB(bitString.Substring(i * 8, 8));
            }

            // Store the number of padding bits in the last byte
            bArr[byteCount] = (byte)paddingBits;
            return bArr;
        }

        /// <summary>
        ///     return byte[] by translating the file (converting each byte by it bitpattern in the table
        /// </summary>
        /// <algo>
        /// Iterate bytewise throug the file adding the new bitvalue from the table to a string.
        /// determine extra bits such that we get a stringlength which is a multiple of 8
        /// convert string to byte[]
        /// </algo>
        internal static byte[] translate(string bitString)
        {
            int x = 0;
            x = 8 - bitString.Length % 8;
            for (int i = 0; i < x; i++) bitString += "0";

            int byteCount = bitString.Length / 8;
            byte[] bArr = new byte[byteCount + 1];
            for (int i = 0; i < byteCount; i++)
            {
                bArr[i] = convert8bB(bitString.Substring(i * 8, 8));
            }
            bArr[byteCount] = (byte)x;
            return bArr;
        }

        private static byte convert8bB(string s)
        {
            int cnt = 0, w = 128;
            for (int b = 0; b < s.Length; b++)
            {
                if (s[b] == '1') cnt += w;
                w /= 2;
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
        public static void rsaveTree(Node n)
        {
            // Leaf node
            if (n.Left == null)
            {
                s += "1";
                s += convertBt8b(n.Key);
            }
            else    //non-leaf
            {
                s += "0";
                rsaveTree(n.Left);
                rsaveTree(n.Right);
            }
        }

        /// <summary>
        /// convert byte to 8 bits
        /// </summary>
        public static string convertBt8b(byte key)
        {
            string str = "";
            byte value = key;
            for (int i = 7; i >= 0; i--)
            {
                int bit = (value >> i) & 1;
                str += bit;
            }
            return str;
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
            while (L.Head != L.Tail)
            {
                // Take the two nodes with the smallest frequencies
                Node first = L.Head;
                Node second = L.Head.Next;

                // Create a new node with the combined frequency
                Node newNode = new Node(55, first.Frequency + second.Frequency);
                newNode.Left = first;
                newNode.Right = second;

                // Remove the two nodes from the list
                L.Head = second.Next;
                if (L.Head != null)
                {
                    L.Head.Previous = null;
                }

                // Add the new node back into the list in the correct position
                L.AddInSequence(newNode);
            }

            return L;
        }

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

        static string s = "";
        /// <summary>
        /// return string that saves the tree structure with the byte values (not the freq)
        /// </summary>
        internal static byte[] saveTree(Node tail)
        {
            s = "";
            rsaveTree(tail);
            byte[] btArr = translate(s);
            return btArr;
        }

        public static void Compress(string inputFile, string outputFile)
        {
            byte[] fileBytes = File.ReadAllBytes(inputFile);
            uint[] freq = countEachByte(fileBytes);
            DoublyLinkedList list = makeDLL(freq);
            list = makeDLLHuffman(list);
            Node root = list.Tail;
            var huffmanTable = BuildHuffmanTable(root);
            byte[] encodedBytes = translate(fileBytes, huffmanTable);
            byte[] treeBytes = saveTree(root);

            // Write the tree length, encoded length, tree, and encoded data to the output file
            using (FileStream fs = new FileStream(outputFile, FileMode.Create))
            {
                fs.WriteByte((byte)treeBytes.Length);
                fs.WriteByte((byte)encodedBytes.Length);
                fs.Write(treeBytes, 0, treeBytes.Length);
                fs.Write(encodedBytes, 0, encodedBytes.Length);
            }
        }

        public static void Decompress(string inputFile, string outputFile)
        {
            byte[] fileBytes = File.ReadAllBytes(inputFile);
            int treeLength = fileBytes[0];
            int encodedLength = fileBytes[1];

            byte[] treeBytes = new byte[treeLength];
            Array.Copy(fileBytes, 2, treeBytes, 0, treeLength);

            byte[] encodedBytes = new byte[encodedLength];
            Array.Copy(fileBytes, 2 + treeLength, encodedBytes, 0, encodedLength);

            Node root = rebuildTree(treeBytes);
            byte[] decodedBytes = decode(encodedBytes, root);

            File.WriteAllBytes(outputFile, decodedBytes);
        }

        private static Node rebuildTree(byte[] treeBytes)
        {
            string treeBits = "";
            foreach (byte b in treeBytes)
            {
                treeBits += convertBt8b(b);
            }

            int index = 0;
            return rebuildTreeRecursive(treeBits, ref index);
        }

        private static Node rebuildTreeRecursive(string treeBits, ref int index)
        {
            if (index >= treeBits.Length) return null;

            if (treeBits[index] == '1')
            {
                index++;
                string byteBits = treeBits.Substring(index, 8);
                index += 8;
                byte key = convert8bB(byteBits);
                return new Node(key, 0);
            }
            else
            {
                index++;
                Node node = new Node(0, 0);
                node.Left = rebuildTreeRecursive(treeBits, ref index);
                node.Right = rebuildTreeRecursive(treeBits, ref index);
                return node;
            }
        }

        private static byte[] decode(byte[] encodedBytes, Node root)
        {
            string bitString = "";
            foreach (byte b in encodedBytes)
            {
                bitString += convertBt8b(b);
            }

            // Remove padding bits
            int paddingBits = encodedBytes[encodedBytes.Length - 1];
            bitString = bitString.Substring(0, bitString.Length - paddingBits);

            List<byte> decodedBytes = new List<byte>();
            Node current = root;
            foreach (char bit in bitString)
            {
                if (bit == '0')
                {
                    current = current.Right;
                }
                else
                {
                    current = current.Left;
                }

                if (current.Left == null && current.Right == null)
                {
                    decodedBytes.Add(current.Key);
                    current = root;
                }
            }

            return decodedBytes.ToArray();
        }
    }
}