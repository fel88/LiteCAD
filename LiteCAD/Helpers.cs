using OpenTK;
using System.Windows.Forms;

namespace LiteCAD
{
    public static class Helpers
    {
        public static DialogResult ShowQuestion(string text, string caption, MessageBoxButtons btns = MessageBoxButtons.YesNo)
        {
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Question);
        }
        public static Vector2 ToVector2f(this Vector2d v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }
    }
}