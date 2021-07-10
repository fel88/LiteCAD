using System.Windows.Forms;

namespace LiteCAD
{
    public class Helpers
    {
        public static DialogResult ShowQuestion(string text, string caption, MessageBoxButtons btns = MessageBoxButtons.YesNo)
        {
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Question);
        }
    }
}