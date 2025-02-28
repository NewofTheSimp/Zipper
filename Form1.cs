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

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    stats.Compress(openFileDialog.FileName, saveFileDialog.FileName);
                    MessageBox.Show("File successfully compressed!");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    stats.Decompress(openFileDialog.FileName, saveFileDialog.FileName);
                    MessageBox.Show("File successfully decompressed!");
                }
            }
        }
    }
}

