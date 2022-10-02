using OpenTK;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LiteCAD.Common
{
    public static class Extensions
    {       
        public static void SetErrorStyle(this TextBox c)
        {
            c.BackColor = Color.Red;
            c.ForeColor = Color.White;
        }
        public static void SetNormalStyle(this TextBox c)
        {
            c.BackColor = Color.White;
            c.ForeColor = Color.Black;
        }
    }
}