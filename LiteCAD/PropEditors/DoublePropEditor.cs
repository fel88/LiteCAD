using System;
using System.Windows.Forms;

namespace LiteCAD.PropEditors
{
    public partial class DoublePropEditor : UserControl, IPropEditor
    {
        public DoublePropEditor()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParentForm.Close();
        }

        public double Result;

        public object ReturnValue { get; set; }        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var res = RPNCalc.Solve(textBox1.Text);
                textBox2.Text = res.ToString();
                //solver formula here
                Result = res;
                ReturnValue = Result;
            }
            catch (Exception ex)
            {

            }
        }

        public void Init(object o)
        {
            textBox1.Text = ((double)o).ToString();
        }
    }   
}