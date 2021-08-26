using System;
using System.Windows.Forms;
using System.Globalization;
using OpenTK;
using LiteCAD.Common;

namespace LiteCAD.Tools
{
    public partial class CylinderProjectionPointEditorToolPanel : UserControl
    {
        public CylinderProjectionPointEditorToolPanel()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            try
            {
                var ang = double.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture) / 180 * Math.PI;
                p.X = ang * 20;
                textBox1.Text = p.X.ToString("N5");
                textBox2.SetNormalStyle();
            }
            catch
            {
                textBox2.SetErrorStyle();
            }
        }
        internal void SetPoint(Vector2d vector2d)
        {
            p = vector2d;
            allowFire = false;
            textBox1.Text = vector2d.X.ToString("N5");
            textBox3.Text = vector2d.Y.ToString("N5");
            var ang = (vector2d.X / 20) * 180 / Math.PI;
            textBox2.Text = ang.ToString("N5");
            allowFire = true;
        }
        bool allowFire = false;
        Vector2d p;
        public event Action<Vector2d> PointChanged;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            try
            {
                p.X = double.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                textBox1.SetNormalStyle();
            }
            catch
            {
                textBox1.SetErrorStyle();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            try
            {
                p.Y = double.Parse(textBox3.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                textBox3.SetNormalStyle();
            }
            catch
            {
                textBox3.SetErrorStyle();
            }
        }
        public event Action Set;

        private void button1_Click(object sender, EventArgs e)
        {
            PointChanged?.Invoke(p);
            Set?.Invoke();
        }
    }
}
