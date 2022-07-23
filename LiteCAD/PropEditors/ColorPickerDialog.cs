using System;
using System.Drawing;
using System.Windows.Forms;

namespace LiteCAD.PropEditors
{
    public partial class ColorPickerDialog : UserControl, IPropEditor
    {
        public ColorPickerDialog()
        {
            InitializeComponent();
        }

        public object ReturnValue { get; set; }

        public void Init(object o)
        {
            colorDialog1.Color = (Color)o;
            pictureBox1.BackColor = (Color)o;
            ReturnValue = o;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            pictureBox1.BackColor = colorDialog1.Color;
            ReturnValue = colorDialog1.Color;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ParentForm.Close();
        }
    }
}
