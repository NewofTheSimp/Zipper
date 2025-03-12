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
            byte[] writer = new byte[2];
            writer[0] = (byte)treeBytes.Length;
            writer[1] = (byte)encodedBytes.Length;


            using (FileStream fs = new FileStream(outputFile, FileMode.Create))
            {
                fs.Write(writer, 0, writer.Length);
                fs.Write(treeBytes, 0, treeBytes.Length);
                fs.Write(encodedBytes, 0, encodedBytes.Length);
            }
        }

        public static void Decompress(string compressedFile, string outputFile)
        {
            byte[] compressedData = File.ReadAllBytes(compressedFile);
            int treeDataLength = getTreeSize(compressedData);
            Node root = rebuildTree(compressedData, treeDataLength);
            byte[] decompressedData = decode(root, compressedData.Skip(treeDataLength).ToArray());
            File.WriteAllBytes(outputFile, decompressedData);
        }

        public static int getTreeSize(byte[] compressedData)
        {
            // Extracts the size of the Huffman tree from the compressed data
            return BitConverter.ToInt32(compressedData, 0);
        }

        public static Node rebuildTree(byte[] compressedData, int treeDataLength)
        {
            // Reconstructs the Huffman tree from the stored tree bytes
            string treeStructure = string.Join("", compressedData.Skip(4).Take(treeDataLength).Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            int index = 0;
            return rebuildTreeRecursive(treeStructure, ref index);
        }

        private static Node rebuildTreeRecursive(string treeStructure, ref int index)
        {
            if (index >= treeStructure.Length)
                return null;

            if (treeStructure[index] == '1')
            {
                index++;
                byte value = Convert.ToByte(treeStructure.Substring(index, 8), 2);
                index += 8;
                return new Node(value, 0);
            }

            index++;
            Node left = rebuildTreeRecursive(treeStructure, ref index);
            Node right = rebuildTreeRecursive(treeStructure, ref index);
            return new Node(0, 0) { Left = left, Right = right };
        }

        public static byte[] decode(Node root, byte[] encodedData)
        {
            List<byte> decodedBytes = new List<byte>();
            Node current = root;
            string bitString = string.Join("", encodedData.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            foreach (char bit in bitString)
            {
                current = (bit == '0') ? current.Left : current.Right;

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

