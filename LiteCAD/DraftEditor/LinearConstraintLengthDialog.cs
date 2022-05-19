using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiteCAD.DraftEditor
{
    public partial class LinearConstraintLengthDialog : Form
    {
        public LinearConstraintLengthDialog()
        {
            InitializeComponent();
        }

        public decimal Length;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Length = decimal.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                Close();
            }
            catch (Exception ex)
            {

            }
        }

        internal void Init(double length)
        {
            Length = (decimal)length;
            textBox1.Text = length.ToString();
        }
    }
}
