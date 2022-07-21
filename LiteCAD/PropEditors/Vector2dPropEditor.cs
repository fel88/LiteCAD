using System;
using System.Windows.Forms;
using LiteCAD.Common;
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
                vector.Y = Helpers.ParseDouble(textBox2.Text);
                textBox2.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox2.SetErrorStyle();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                vector.X = Helpers.ParseDouble(textBox1.Text);
                textBox1.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox1.SetErrorStyle();
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
