using OpenTK;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LiteCAD.Common
{
    public static class Extensions
    {
        public static Vector3 ToVector3(this Vector3d v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }
        
        public static Vector2d ToVector2d(this Vector2 v)
        {
            return new Vector2d(v.X, v.Y);
        }
        public static StringBuilder ToXml(this PolyBoolCS.Polygon p)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("<?xml version=\"1.0\"?>");
            s.AppendLine("<root>");
            foreach (var item in p.regions)
            {
                s.AppendLine("<region>");

                foreach (var pn in item)
                {
                    s.AppendLine($"<point x=\"{pn.x}\" y=\"{pn.y}\"/>");
                }
                s.AppendLine("</region>");
            }
            s.AppendLine("</root>");
            return s;

        }
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