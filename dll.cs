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
        public byte Key;          // The byte value this node represents
        public uint Frequency;    // Frequency of this byte in the input
        public Node Next;        // Next node in doubly linked list
        public Node Previous;    // Previous node in doubly linked list
        public Node Left;        // Left child in Huffman tree
        public Node Right;       // Right child in Huffman tree

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
        public Node Head;  // First node in the list
        public Node Tail;  // Last node in the list

        public DoublyLinkedList()
        {
            Head = null;
            Tail = null;
        }

        /// <summary>
        /// Adds a node to the list while maintaining frequency order (ascending)
        /// </summary>
        public void AddInSequence(Node n)
        {
            if (Head == null)
            {
                // Empty list case
                Head = n;
                Tail = n;
            }
            else if (n.Frequency <= Head.Frequency)
            {
                // Insert at beginning
                n.Next = Head;
                Head.Previous = n;
                Head = n;
            }
            else if (n.Frequency > Tail.Frequency)
            {
                // Insert at end
                n.Previous = Tail;
                Tail.Next = n;
                Tail = n;
            }
            else
            {
                // Insert in middle - find correct position
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
            // Calculate total bits needed for all Huffman codes
            int totalBits = 0;
            foreach (byte b in f)
            {
                totalBits += table[b].Length;
            }

            // Calculate padding needed to make total bits a multiple of 8
            int paddingBits = (8 - (totalBits % 8)) % 8;
            totalBits += paddingBits;
            int byteCount = totalBits / 8;

            // Store all bits in a list for efficient appending
            List<bool> bitList = new List<bool>(totalBits);
            foreach (byte b in f)
            {
                bitList.AddRange(table[b]);
            }

            // Add padding bits (0s)
            for (int i = 0; i < paddingBits; i++)
            {
                bitList.Add(false);
            }

            // Convert bit list to byte array
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

            // Store padding count in last byte
            bArr[byteCount] = (byte)paddingBits;
            return bArr;
        }

        /// <summary>
        /// Converts a bit array to a byte array with padding information
        /// </summary>
        internal static byte[] translate(bool[] bitArray)
        {
            // Calculate padding needed
            int paddingBits = (8 - (bitArray.Length % 8)) % 8;
            int totalBits = bitArray.Length + paddingBits;
            int byteCount = totalBits / 8;

            // Create padded bit array
            bool[] paddedBits = new bool[totalBits];
            Array.Copy(bitArray, paddedBits, bitArray.Length);

            // Convert to bytes
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

            // Store padding count
            bArr[byteCount] = (byte)paddingBits;
            return bArr;
        }

        /// <summary>
        /// Counts frequency of each byte in the input file
        /// </summary>
        internal static uint[] countEachByte(byte[] file)
        {
            uint[] f = new uint[256];
            for (int i = 0; i < file.Length; i++) f[file[i]]++;
            return f;
        }

        /// <summary>
        /// Creates doubly linked list from frequency counts
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

        /// <summary>
        /// Recursively builds Huffman code table by traversing the tree
        /// </summary>
        internal static void BuildTableRecursive(Node tail, bool[] code, List<bool> currentCode, Dictionary<byte, bool[]> huffmanTable)
        {
            if (tail == null) return;

            // Leaf node - store its code
            if (tail.Left == null && tail.Right == null)
            {
                huffmanTable[tail.Key] = currentCode.ToArray();
            }

            // Traverse left with '1' added to code
            if (tail.Left != null)
            {
                var leftCode = new List<bool>(currentCode);
                leftCode.Add(true);
                BuildTableRecursive(tail.Left, code, leftCode, huffmanTable);
            }

            // Traverse right with '0' added to code
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
        public static void rsaveTree(Node n, List<bool> treeBits)
        {
            if (n.Left == null)
            {
                // Leaf node - mark with 1 followed by 8-bit byte value
                treeBits.Add(true);
                treeBits.AddRange(ByteToBits(n.Key));
            }
            else
            {
                // Internal node - mark with 0 then serialize children
                treeBits.Add(false);
                rsaveTree(n.Left, treeBits);
                rsaveTree(n.Right, treeBits);
            }
        }

        /// <summary>
        /// Converts byte to bit array (MSB first)
        /// </summary>
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
        public static Dictionary<byte, bool[]> BuildHuffmanTable(Node root)
        {
            var huffmanTable = new Dictionary<byte, bool[]>();
            BuildTableRecursive(root, new bool[0], new List<bool>(), huffmanTable);
            return huffmanTable;
        }

        /// <summary>
        /// Constructs Huffman tree from frequency-sorted list
        /// </summary>
        internal static DoublyLinkedList makeDLLHuffman(DoublyLinkedList L)
        {
            // Combine nodes until only one remains (the root)
            while (L.Head != L.Tail)
            {
                // Take two lowest frequency nodes
                Node first = L.Head;
                Node second = L.Head.Next;

                // Create new combined node
                Node newNode = new Node(55, first.Frequency + second.Frequency);
                newNode.Left = first;
                newNode.Right = second;

                // Remove the two nodes
                L.Head = second.Next;
                if (L.Head != null)
                {
                    L.Head.Previous = null;
                }

                // Insert new combined node in proper position
                L.AddInSequence(newNode);
            }

            return L;
        }

        /// <summary>
        /// Opens file dialog and reads selected file
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
        /// Serializes Huffman tree to compact bit representation
        /// </summary>
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
        public static void Compress(string inputFile, string outputFile)
        {
            // Read file and calculate frequencies
            byte[] fileBytes = File.ReadAllBytes(inputFile);
            uint[] freq = countEachByte(fileBytes);

            // Build Huffman tree
            DoublyLinkedList list = makeDLL(freq);
            list = makeDLLHuffman(list);
            Node root = list.Tail;

            // Generate codes and compress data
            var huffmanTable = BuildHuffmanTable(root);
            byte[] encodedBytes = translate(fileBytes, huffmanTable);
            byte[] treeBytes = saveTree(root);

            // Write compressed file
            write2file(outputFile, encodedBytes, treeBytes);
        }

        /// <summary>
        /// Writes compressed data to file with header information
        /// </summary>
        public static void write2file(string outputFile, byte[] encodedBytes, byte[] treeBytes)
        {
            using (FileStream fs = new FileStream(outputFile, FileMode.Create))
            {
                // Write tree length (2 bytes)
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

                // Write tree and compressed data
                fs.Write(treeBytes, 0, treeBytes.Length);
                fs.Write(encodedBytes, 0, encodedBytes.Length);
            }
        }

        /// <summary>
        /// Decompresses Huffman-encoded file
        /// </summary>
        public static void Decompress(string inputFile, string outputFile)
        {
            // Read compressed file
            byte[] fileBytes = File.ReadAllBytes(inputFile);

            // Parse header to get tree and data lengths
            int treeLength = fileBytes[0] == 0 ? fileBytes[1] : (fileBytes[0] * 256) + fileBytes[1];
            int encodedLength = fileBytes.Length - treeLength - 2;

            // Extract tree and compressed data
            byte[] treeBytes = new byte[treeLength];
            Array.Copy(fileBytes, 2, treeBytes, 0, treeLength);

            byte[] encodedBytes = new byte[encodedLength];
            Array.Copy(fileBytes, 2 + treeLength, encodedBytes, 0, encodedLength);

            // Rebuild tree and decode data
            Node root = rebuildTree(treeBytes);
            byte[] decodedBytes = decode(encodedBytes, root);

            // Write decompressed file
            File.WriteAllBytes(outputFile, decodedBytes);
        }

        /// <summary>
        /// Rebuilds Huffman tree from serialized bit representation
        /// </summary>
        private static Node rebuildTree(byte[] treeBytes)
        {
            // Convert tree bytes to bits
            List<bool> treeBits = new List<bool>();
            for (int i = 0; i < treeBytes.Length - 1; i++)
            {
                treeBits.AddRange(ByteToBits(treeBytes[i]));
            }

            // Remove padding bits
            int paddingBits = treeBytes[treeBytes.Length - 1];
            treeBits.RemoveRange(treeBits.Count - paddingBits, paddingBits);

            // Rebuild tree recursively
            int index = 0;
            return rebuildTreeRecursive(treeBits.ToArray(), ref index);
        }

        /// <summary>
        /// Recursively rebuilds Huffman tree from bit array
        /// </summary>
        private static Node rebuildTreeRecursive(bool[] treeBits, ref int index)
        {
            if (index >= treeBits.Length) return null;

            if (treeBits[index++]) // Leaf node
            {
                // Read next 8 bits as byte value
                bool[] byteBits = new bool[8];
                Array.Copy(treeBits, index, byteBits, 0, 8);
                index += 8;
                byte key = BitsToByte(byteBits);
                return new Node(key, 0);
            }
            else // Internal node
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
        private static byte[] decode(byte[] encodedBytes, Node root)
        {
            // Convert encoded bytes to bits
            List<bool> bitList = new List<bool>();
            for (int i = 0; i < encodedBytes.Length - 1; i++)
            {
                bitList.AddRange(ByteToBits(encodedBytes[i]));
            }

            // Remove padding bits
            int paddingBits = encodedBytes[encodedBytes.Length - 1];
            bitList.RemoveRange(bitList.Count - paddingBits, paddingBits);

            // Traverse tree using bits to decode bytes
            List<byte> decodedBytes = new List<byte>();
            Node current = root;
            foreach (bool bit in bitList)
            {
                current = bit ? current.Left : current.Right;

                // When leaf is reached, record byte and reset to root
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