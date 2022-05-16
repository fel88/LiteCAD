using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace LiteCAD.PropEditors
{
    public partial class Matrix4dPropEditor : UserControl, IPropEditor
    {
        public Matrix4dPropEditor()
        {
            InitializeComponent();
        }
        TransformationChain _matrix;
        public object ReturnValue => _matrix;

        public void Init(object o)
        {
            _matrix = (TransformationChain)o;
            updateList();
        }

        private void translateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _matrix.Items.Add(new TranslateTransformChainItem());
            updateList();

        }

        void updateList()
        {
            listView1.Items.Clear();
            foreach (var item in _matrix.Items)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.GetType().Name }) { Tag = item });
            }
        }
        private void scaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _matrix.Items.Add(new ScaleTransformChainItem());
            updateList();
        }
        TransformationChainItem current = null;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tci = listView1.SelectedItems[0].Tag as TransformationChainItem;
            current = tci;
            if (tci is TranslateTransformChainItem tr)
            {
                textBox1.Text = tr.Vector.X.ToString();
                textBox2.Text = tr.Vector.Y.ToString();
                textBox3.Text = tr.Vector.Z.ToString();
            }
            if (tci is ScaleTransformChainItem sc)
            {
                textBox1.Text = sc.Vector.X.ToString();
                textBox2.Text = sc.Vector.Y.ToString();
                textBox3.Text = sc.Vector.Z.ToString();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {

                if (current is TranslateTransformChainItem tr)
                {
                    tr.Vector.X = Helpers.ParseDouble(textBox1.Text);
                }
                if (current is ScaleTransformChainItem sc)
                {
                    sc.Vector.X = Helpers.ParseDouble(textBox1.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tci = listView1.SelectedItems[0].Tag as TransformationChainItem;
            _matrix.Items.Remove(tci);
            updateList();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _matrix.Items.Clear();
            updateList();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {

                if (current is TranslateTransformChainItem tr)
                {
                    tr.Vector.Y = Helpers.ParseDouble(textBox2.Text);
                }
                if (current is ScaleTransformChainItem sc)
                {
                    sc.Vector.Y = Helpers.ParseDouble(textBox2.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {

                if (current is TranslateTransformChainItem tr)
                {
                    tr.Vector.Z = Helpers.ParseDouble(textBox3.Text);
                }
                if (current is ScaleTransformChainItem sc)
                {
                    sc.Vector.Z = Helpers.ParseDouble(textBox3.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
