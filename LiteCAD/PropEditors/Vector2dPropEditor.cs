using System;
using System.Windows.Forms;
using OpenTK;

namespace LiteCAD.PropEditors
{
    public partial class Vector2dPropEditor : UserControl, IPropEditor
    {
        public Vector2dPropEditor()
        {
            InitializeComponent();
        }
        Vector2d vector;
        public object ReturnValue { get => vector; }

                

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                vector.Y = double.Parse(textBox2.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                vector.X = double.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {

            }
        }

        public void Init(object o)
        {
            var v = (Vector3d)o;
            textBox1.Text = v.X.ToString();
            textBox2.Text = v.Y.ToString();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParentForm.Close();
        }
    }
}
