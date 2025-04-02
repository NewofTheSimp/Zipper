using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Zipper
{
    /// <summary>
    /// Represents a node in the Huffman tree and doubly linked list
    /// </summary>
    public class Node
    {
        /// <summary>The byte value this node represents</summary>
        public byte Key;

        /// <summary>Frequency of this byte in the input</summary>
        public uint Frequency;

        /// <summary>Next node in doubly linked list</summary>
        public Node Next;

        /// <summary>Previous node in doubly linked list</summary>
        public Node Previous;

        /// <summary>Left child in Huffman tree</summary>
        public Node Left;

        /// <summary>Right child in Huffman tree</summary>
        public Node Right;

        /// <summary>
        /// Initializes a new node with the specified key and frequency
        /// </summary>
        /// <param name="key">The byte value</param>
        /// <param name="frequency">Frequency count</param>
        public Node(byte key, uint frequency)
        {
            Key = key;
            Frequency = frequency;
            Next = null;
            Previous = null;
            Left = null;
            Right = null;
        }
    }

    /// <summary>
    /// Doubly linked list implementation for building Huffman tree
    /// </summary>
    public class DoublyLinkedList
    {
        /// <summary>First node in the list</summary>
        public Node Head;

        /// <summary>Last node in the list</summary>
        public Node Tail;

        /// <summary>
        /// Initializes an empty doubly linked list
        /// </summary>
        public DoublyLinkedList()
        {
            Head = null;
            Tail = null;
        }

        /// <summary>
        /// Adds a node to the list while maintaining frequency order (ascending)
        /// </summary>
        /// <param name="n">Node to insert</param>
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
    /// Main class for Huffman compression and decompression operations
    /// </summary>
    public class stats
    {
        /// <summary>
        /// Compresses input bytes using Huffman codes
        /// </summary>
        /// <param name="f">Input byte array</param>
        /// <param name="table">Huffman code lookup table</param>
        /// <returns>Compressed byte array with padding info</returns>
        internal static byte[] translate(byte[] f, Dictionary<byte, bool[]> table)
        {
            int totalBits = 0;
            foreach (byte b in f)
            {
                totalBits += table[b].Length;
            }

            int paddingBits = (8 - (totalBits % 8)) % 8;
            totalBits += paddingBits;
            int byteCount = totalBits / 8;

            List<bool> bitList = new List<bool>(totalBits);
            foreach (byte b in f)
            {
                bitList.AddRange(table[b]);
            }

            for (int i = 0; i < paddingBits; i++)
            {
                bitList.Add(false);
            }

            byte[] bArr = new byte[byteCount + 1];
            for (int i = 0; i < byteCount; i++)
            {
                byte val = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (bitList[i * 8 + j])
                    {
                        val |= (byte)(1 << (7 - j));
                    }
                }
                bArr[i] = val;
            }

            bArr[byteCount] = (byte)paddingBits;
            return bArr;
        }

        /// <summary>
        /// Converts a bit array to a byte array with padding information
        /// </summary>
        /// <param name="bitArray">Array of bits to convert</param>
        /// <returns>Byte array with padding info in last byte</returns>
        internal static byte[] translate(bool[] bitArray)
        {
            int paddingBits = (8 - (bitArray.Length % 8)) % 8;
            int totalBits = bitArray.Length + paddingBits;
            int byteCount = totalBits / 8;

            bool[] paddedBits = new bool[totalBits];
            Array.Copy(bitArray, paddedBits, bitArray.Length);

            byte[] bArr = new byte[byteCount + 1];
            for (int i = 0; i < byteCount; i++)
            {
                byte val = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (paddedBits[i * 8 + j])
                    {
                        val |= (byte)(1 << (7 - j));
                    }
                }
                bArr[i] = val;
            }

            bArr[byteCount] = (byte)paddingBits;
            return bArr;
        }

        /// <summary>
        /// Counts frequency of each byte in the input file
        /// </summary>
        /// <param name="file">Input byte array</param>
        /// <returns>Array of frequencies indexed by byte value</returns>
        internal static uint[] countEachByte(byte[] file)
        {
            uint[] f = new uint[256];
            for (int i = 0; i < file.Length; i++) f[file[i]]++;
            return f;
        }

        /// <summary>
        /// Creates doubly linked list from frequency counts
        /// </summary>
        /// <param name="freq">Array of byte frequencies</param>
        /// <returns>Constructed doubly linked list</returns>
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
        /// Recursively builds Huffman code table by traversing the tree
        /// </summary>
        /// <param name="tail">Current node being processed</param>
        /// <param name="code">Current Huffman code being built</param>
        /// <param name="currentCode">List storing current code bits</param>
        /// <param name="huffmanTable">Dictionary to store completed codes</param>
        internal static void BuildTableRecursive(Node tail, bool[] code, List<bool> currentCode, Dictionary<byte, bool[]> huffmanTable)
        {
            if (tail == null) return;

            if (tail.Left == null && tail.Right == null)
            {
                huffmanTable[tail.Key] = currentCode.ToArray();
            }

            if (tail.Left != null)
            {
                var leftCode = new List<bool>(currentCode);
                leftCode.Add(true);
                BuildTableRecursive(tail.Left, code, leftCode, huffmanTable);
            }

            if (tail.Right != null)
            {
                var rightCode = new List<bool>(currentCode);
                rightCode.Add(false);
                BuildTableRecursive(tail.Right, code, rightCode, huffmanTable);
            }
        }

        /// <summary>
        /// Recursively serializes Huffman tree structure to bit list
        /// </summary>
        /// <param name="n">Current node being processed</param>
        /// <param name="treeBits">List storing the serialized tree bits</param>
        public static void rsaveTree(Node n, List<bool> treeBits)
        {
            if (n.Left == null)
            {
                treeBits.Add(true);
                treeBits.AddRange(ByteToBits(n.Key));
            }
            else
            {
                treeBits.Add(false);
                rsaveTree(n.Left, treeBits);
                rsaveTree(n.Right, treeBits);
            }
        }

        /// <summary>
        /// Converts byte to bit array (MSB first)
        /// </summary>
        /// <param name="b">Byte to convert</param>
        /// <returns>8-element boolean array representing bits</returns>
        public static bool[] ByteToBits(byte b)
        {
            bool[] bits = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                bits[i] = (b & (1 << (7 - i))) != 0;
            }
            return bits;
        }

        /// <summary>
        /// Converts bit array to byte (MSB first)
        /// </summary>
        /// <param name="bits">Boolean array representing bits</param>
        /// <returns>Reconstructed byte</returns>
        public static byte BitsToByte(bool[] bits)
        {
            byte val = 0;
            for (int i = 0; i < 8 && i < bits.Length; i++)
            {
                if (bits[i])
                {
                    val |= (byte)(1 << (7 - i));
                }
            }
            return val;
        }

        /// <summary>
        /// Builds Huffman code table from tree root
        /// </summary>
        /// <param name="root">Root node of Huffman tree</param>
        /// <returns>Dictionary mapping bytes to their Huffman codes</returns>
        public static Dictionary<byte, bool[]> BuildHuffmanTable(Node root)
        {
            var huffmanTable = new Dictionary<byte, bool[]>();
            BuildTableRecursive(root, new bool[0], new List<bool>(), huffmanTable);
            return huffmanTable;
        }

        /// <summary>
        /// Constructs Huffman tree from frequency-sorted list
        /// </summary>
        /// <param name="L">Doubly linked list of nodes</param>
        /// <returns>List containing only the root node</returns>
        internal static DoublyLinkedList makeDLLHuffman(DoublyLinkedList L)
        {
            while (L.Head != L.Tail)
            {
                Node first = L.Head;
                Node second = L.Head.Next;

                Node newNode = new Node(55, first.Frequency + second.Frequency);
                newNode.Left = first;
                newNode.Right = second;

                L.Head = second.Next;
                if (L.Head != null)
                {
                    L.Head.Previous = null;
                }

                L.AddInSequence(newNode);
            }

            return L;
        }

        /// <summary>
        /// Opens file dialog and reads selected file
        /// </summary>
        /// <returns>Byte array of file contents or null if canceled</returns>
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
        /// Serializes Huffman tree to compact bit representation
        /// </summary>
        /// <param name="tail">Root node of Huffman tree</param>
        /// <returns>Byte array representing the serialized tree</returns>
        internal static byte[] saveTree(Node tail)
        {
            List<bool> treeBits = new List<bool>();
            rsaveTree(tail, treeBits);
            byte[] btArr = translate(treeBits.ToArray());
            return btArr;
        }

        /// <summary>
        /// Compresses input file using Huffman coding
        /// </summary>
        /// <param name="inputFile">Path to input file</param>
        /// <param name="outputFile">Path for compressed output</param>
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

            write2file(outputFile, encodedBytes, treeBytes);
        }

        /// <summary>
        /// Writes compressed data to file with header information
        /// </summary>
        /// <param name="outputFile">Output file path</param>
        /// <param name="encodedBytes">Compressed data bytes</param>
        /// <param name="treeBytes">Serialized Huffman tree</param>
        public static void write2file(string outputFile, byte[] encodedBytes, byte[] treeBytes)
        {
            using (FileStream fs = new FileStream(outputFile, FileMode.Create))
            {
                if (treeBytes.Length < 256)
                {
                    fs.WriteByte(0);
                    fs.WriteByte((byte)treeBytes.Length);
                }
                else
                {
                    int x = treeBytes.Length / 256;
                    int z = treeBytes.Length % 256;

                    fs.WriteByte((byte)x);
                    fs.WriteByte((byte)z);
                }

                fs.Write(treeBytes, 0, treeBytes.Length);
                fs.Write(encodedBytes, 0, encodedBytes.Length);
            }
        }

        /// <summary>
        /// Decompresses Huffman-encoded file
        /// </summary>
        /// <param name="inputFile">Path to compressed file</param>
        /// <param name="outputFile">Path for decompressed output</param>
        public static void Decompress(string inputFile, string outputFile)
        {
            byte[] fileBytes = File.ReadAllBytes(inputFile);

            int treeLength = fileBytes[0] == 0 ? fileBytes[1] : (fileBytes[0] * 256) + fileBytes[1];
            int encodedLength = fileBytes.Length - treeLength - 2;

            byte[] treeBytes = new byte[treeLength];
            Array.Copy(fileBytes, 2, treeBytes, 0, treeLength);

            byte[] encodedBytes = new byte[encodedLength];
            Array.Copy(fileBytes, 2 + treeLength, encodedBytes, 0, encodedLength);

            Node root = rebuildTree(treeBytes);
            byte[] decodedBytes = decode(encodedBytes, root);

            File.WriteAllBytes(outputFile, decodedBytes);
        }

        /// <summary>
        /// Rebuilds Huffman tree from serialized bit representation
        /// </summary>
        /// <param name="treeBytes">Serialized tree data</param>
        /// <returns>Root node of reconstructed Huffman tree</returns>
        private static Node rebuildTree(byte[] treeBytes)
        {
            List<bool> treeBits = new List<bool>();
            for (int i = 0; i < treeBytes.Length - 1; i++)
            {
                treeBits.AddRange(ByteToBits(treeBytes[i]));
            }

            int paddingBits = treeBytes[treeBytes.Length - 1];
            treeBits.RemoveRange(treeBits.Count - paddingBits, paddingBits);

            int index = 0;
            return rebuildTreeRecursive(treeBits.ToArray(), ref index);
        }

        /// <summary>
        /// Recursively rebuilds Huffman tree from bit array
        /// </summary>
        /// <param name="treeBits">Bit array representing the tree</param>
        /// <param name="index">Current position in bit array (passed by reference)</param>
        /// <returns>Reconstructed node</returns>
        private static Node rebuildTreeRecursive(bool[] treeBits, ref int index)
        {
            if (index >= treeBits.Length) return null;

            if (treeBits[index++])
            {
                bool[] byteBits = new bool[8];
                Array.Copy(treeBits, index, byteBits, 0, 8);
                index += 8;
                byte key = BitsToByte(byteBits);
                return new Node(key, 0);
            }
            else
            {
                Node node = new Node(0, 0);
                node.Left = rebuildTreeRecursive(treeBits, ref index);
                node.Right = rebuildTreeRecursive(treeBits, ref index);
                return node;
            }
        }

        /// <summary>
        /// Decodes compressed data using Huffman tree
        /// </summary>
        /// <param name="encodedBytes">Compressed data bytes</param>
        /// <param name="root">Root node of Huffman tree</param>
        /// <returns>Decompressed byte array</returns>
        private static byte[] decode(byte[] encodedBytes, Node root)
        {
            List<bool> bitList = new List<bool>();
            for (int i = 0; i < encodedBytes.Length - 1; i++)
            {
                bitList.AddRange(ByteToBits(encodedBytes[i]));
            }

            int paddingBits = encodedBytes[encodedBytes.Length - 1];
            bitList.RemoveRange(bitList.Count - paddingBits, paddingBits);

            List<byte> decodedBytes = new List<byte>();
            Node current = root;
            foreach (bool bit in bitList)
            {
                current = bit ? current.Left : current.Right;

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