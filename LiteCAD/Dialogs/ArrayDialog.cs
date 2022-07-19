using LiteCAD.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiteCAD.Dialogs
{
    public partial class ArrayDialog : Form
    {
        public ArrayDialog()
        {
            InitializeComponent();
        }

        public int QtyX = 1;
        public int QtyY = 1;
        public double OffsetX = 0;
        public double OffsetY = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                QtyX = int.Parse(textBox1.Text);
                textBox1.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox1.SetErrorStyle();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                QtyY = int.Parse(textBox2.Text);
                textBox2.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox2.SetErrorStyle();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OffsetX = Helpers.ParseDouble(textBox4.Text);
                textBox4.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox4.SetErrorStyle();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OffsetY = Helpers.ParseDouble(textBox3.Text);
                textBox3.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox3.SetErrorStyle();
            }
        }
    }
}
