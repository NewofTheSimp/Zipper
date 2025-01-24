using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Zipper
{


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            byte[] file = stats.readFileAsBytes();
            uint[] freq = stats.countEachByte(file);
            DoublyLinkedList DLL = stats.makeDLL(freq);
            DLL = stats.makeDLLHuffman(DLL);
            //Node tail = stats.CreateTree(DLL.Head);
            Dictionary<byte, string> table = stats.BuildHuffmanTable(DLL.Tail);
            byte[] byArr = stats.translate(file, table);

        }

    }
}

