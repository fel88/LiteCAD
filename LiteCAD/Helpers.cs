using OpenTK;
using System.Drawing;
using System.Windows.Forms;

namespace LiteCAD
{
    public static class Helpers
    {
        public static DialogResult ShowQuestion(string text, string caption, MessageBoxButtons btns = MessageBoxButtons.YesNo)
        {
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Question);
        }
        public static DialogResult Warning(string text, string caption, MessageBoxButtons btns = MessageBoxButtons.OK)
        {
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Warning);
        }
        public static Vector2 ToVector2f(this Vector2d v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }
        public static PointF ToPointF(this Vector2d v)
        {
            return new PointF((float)v.X, (float)v.Y);
        }
    }
}