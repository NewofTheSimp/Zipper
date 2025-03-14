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
        public uint Frequency;    // Stores the frequency of the byte (how many times this byte appears)
        public Node Next;         // Points to the next node in the doubly linked list
        public Node Previous;     // Points to the previous node in the doubly linked list
        public Node Left;         // Points to the left node, typically used for the tree structure (Huffman tree)
        public Node Right;        // Points to the right node, typically used for the tree structure (Huffman tree)

        // Constructor to initialize a Node with a given key and frequency
        public Node(byte key, uint frequency)
        {
            Key = key;            // Set the byte key
            Frequency = frequency;// Set the frequency of the byte
            Next = null;         // Initially, no next node
            Previous = null;      // Initially, no previous node
            Left = null;         // Initially, no left child (used in Huffman tree)
            Right = null;         // Initially, no right child (used in Huffman tree)
        }
    }

    // Doubly Linked List class for byte Key-Frequency pairs
    public class DoublyLinkedList
    {
        // Properties
        public Node Head;  // Points to the head (first node) of the list
        public Node Tail;  // Points to the tail (last node) of the list

        // Constructor
        public DoublyLinkedList()
        {
            Head = null;  // Initially, no head node
            Tail = null;  // Initially, no tail node
        }

        // Methods

        /// <summary>
        /// Add a new node into the doubly linked list in ascending order based on frequency
        /// </summary>
        /// <param name="n">The node to add</param>
        public void AddInSequence(Node n)
        {
            // If the list is empty, set the new node as both head and tail
            if (Head == null)
            {
                Head = n;
                Tail = n;
            }
            // If the new node's frequency is less than or equal to the head's frequency, insert at the beginning
            else if (n.Frequency <= Head.Frequency)
            {
                n.Next = Head;
                Head.Previous = n;
                Head = n;
            }
            // If the new node's frequency is greater than the tail's frequency, insert at the end
            else if (n.Frequency > Tail.Frequency)
            {
                n.Previous = Tail;
                Tail.Next = n;
                Tail = n;
            }
            // Otherwise, find the correct position in the middle of the list
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

    // Main class for Huffman compression and decompression
    public class stats
    {
        /// <summary>
        /// Translates a byte array into a compressed bit string using the Huffman table
        /// </summary>
        /// <param name="f">The input byte array</param>
        /// <param name="table">The Huffman table mapping bytes to bit strings</param>
        /// <returns>The compressed byte array</returns>
        internal static byte[] translate(byte[] f, Dictionary<byte, string> table)
        {
            string bitString = "";
            // Iterate through each byte in the input array and append its Huffman code to the bit string
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
        /// Converts a bit string into a byte array
        /// </summary>
        /// <param name="bitString">The input bit string</param>
        /// <returns>The resulting byte array</returns>
        internal static byte[] translate(string bitString)
        {
            // Add padding bits to make the length a multiple of 8
            int x = 0;
            x = 8 - bitString.Length % 8;
            for (int i = 0; i < x; i++) bitString += "0";

            // Convert the bit string to a byte array
            int byteCount = bitString.Length / 8;
            byte[] bArr = new byte[byteCount + 1];
            for (int i = 0; i < byteCount; i++)
            {
                bArr[i] = convert8bB(bitString.Substring(i * 8, 8));
            }
            // Store the number of padding bits in the last byte
            bArr[byteCount] = (byte)x;
            return bArr;
        }

        /// <summary>
        /// Converts an 8-bit binary string to a byte
        /// </summary>
        /// <param name="s">The 8-bit binary string</param>
        /// <returns>The resulting byte</returns>
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
        /// Counts the frequency of each byte in the input file
        /// </summary>
        /// <param name="file">The input byte array</param>
        /// <returns>An array of frequencies indexed by byte value</returns>
        internal static uint[] countEachByte(byte[] file)
        {
            uint[] f = new uint[256];
            for (int i = 0; i < file.Length; i++) f[file[i]]++;
            return f;
        }

        /// <summary>
        /// Creates a doubly linked list from the frequency array
        /// </summary>
        /// <param name="freq">The frequency array</param>
        /// <returns>The doubly linked list</returns>
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

        /// <summary>
        /// Recursively builds the Huffman table by traversing the Huffman tree
        /// </summary>
        /// <param name="tail">The current node in the tree</param>
        /// <param name="code">The current Huffman code</param>
        /// <param name="huffmanTable">The Huffman table to populate</param>
        internal static void BuildTableRecursive(Node tail, string code, Dictionary<byte, string> huffmanTable)
        {
            if (tail == null) return;

            // If the current node is a leaf, add its byte and Huffman code to the table
            if (tail.Left == null && tail.Right == null)
            {
                huffmanTable[tail.Key] = code;
            }

            // Traverse the left and right subtrees
            BuildTableRecursive(tail.Left, code + "1", huffmanTable);
            BuildTableRecursive(tail.Right, code + "0", huffmanTable);
        }

        /// <summary>
        /// Recursively saves the Huffman tree structure as a bit string
        /// </summary>
        /// <param name="n">The current node in the tree</param>
        public static void rsaveTree(Node n)
        {
            // If the current node is a leaf, append '1' followed by its byte value
            if (n.Left == null)
            {
                s += "1";
                s += convertBt8b(n.Key);
            }
            // If the current node is not a leaf, append '0' and traverse its children
            else
            {
                s += "0";
                rsaveTree(n.Left);
                rsaveTree(n.Right);
            }
        }

        /// <summary>
        /// Converts a byte to an 8-bit binary string
        /// </summary>
        /// <param name="key">The byte to convert</param>
        /// <returns>The 8-bit binary string</returns>
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

        /// <summary>
        /// Builds the Huffman table from the Huffman tree
        /// </summary>
        /// <param name="root">The root of the Huffman tree</param>
        /// <returns>The Huffman table</returns>
        public static Dictionary<byte, string> BuildHuffmanTable(Node root)
        {
            var huffmanTable = new Dictionary<byte, string>();
            BuildTableRecursive(root, "", huffmanTable);
            return huffmanTable;
        }

        /// <summary>
        /// Constructs the Huffman tree from the doubly linked list
        /// </summary>
        /// <param name="L">The doubly linked list</param>
        /// <returns>The modified doubly linked list with the Huffman tree</returns>
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
        /// Reads a file as a byte array
        /// </summary>
        /// <returns>The byte array representing the file</returns>
        internal static byte[] readFileAsBytes()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return File.ReadAllBytes(ofd.FileName);
            }
            return null;
        }

        // Static variable to store the tree structure as a bit string
        static string s = "";

        /// <summary>
        /// Saves the Huffman tree structure as a byte array
        /// </summary>
        /// <param name="tail">The root of the Huffman tree</param>
        /// <returns>The byte array representing the tree</returns>
        internal static byte[] saveTree(Node tail)
        {
            s = "";
            rsaveTree(tail);
            byte[] btArr = translate(s);
            return btArr;
        }

        /// <summary>
        /// Compresses a file using Huffman encoding
        /// </summary>
        /// <param name="inputFile">The path to the input file</param>
        /// <param name="outputFile">The path to the output file</param>
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

        /// <summary>
        /// Decompresses a file using Huffman decoding
        /// </summary>
        /// <param name="inputFile">The path to the input file</param>
        /// <param name="outputFile">The path to the output file</param>
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

        /// <summary>
        /// Rebuilds the Huffman tree from a byte array
        /// </summary>
        /// <param name="treeBytes">The byte array representing the tree</param>
        /// <returns>The root of the Huffman tree</returns>
        private static Node rebuildTree(byte[] treeBytes)
        {
            string treeBits = "";
            foreach (byte b in treeBytes)
            {
                treeBits += convertBt8b(b);
            }


            // Remove padding bits
            int paddingBits = treeBytes[treeBytes.Length - 1];
            treeBits = treeBits.Substring(0, treeBits.Length - paddingBits);

            int index = 0;
            return rebuildTreeRecursive(treeBits, ref index);
        }

        /// <summary>
        /// Recursively rebuilds the Huffman tree from a bit string
        /// </summary>
        /// <param name="treeBits">The bit string representing the tree</param>
        /// <param name="index">The current position in the bit string</param>
        /// <returns>The current node in the tree</returns>
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

        /// <summary>
        /// Decodes the compressed data using the Huffman tree
        /// </summary>
        /// <param name="encodedBytes">The compressed byte array</param>
        /// <param name="root">The root of the Huffman tree</param>
        /// <returns>The decoded byte array</returns>
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