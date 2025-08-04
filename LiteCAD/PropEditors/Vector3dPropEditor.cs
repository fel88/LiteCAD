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
using OpenTK.Mathematics;

namespace LiteCAD.PropEditors
{
    public partial class Vector3dPropEditor : UserControl, IPropEditor
    {
        public Vector3dPropEditor()
        {
            InitializeComponent();
        }
        Vector3d vector;
        public object ReturnValue { get => vector; }


        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                vector.Z = double.Parse(textBox3.Text);
            }
            catch (Exception ex)
            {

            }
        }

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
            textBox3.Text = v.Z.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParentForm.Close();
        }
    }
}
