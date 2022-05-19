using OpenTK;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace LiteCAD
{
    public static class GUIHelpers
    {        
        public static DialogResult ShowQuestion(string text, string caption = null, MessageBoxButtons btns = MessageBoxButtons.YesNo)
        {
            if (caption == null) { caption = Form1.Form.Text; }
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Question);
        }
        public static DialogResult Warning(string text, string caption=null,  MessageBoxButtons btns = MessageBoxButtons.OK)
        {
            if (caption == null) { caption = Form1.Form.Text; }
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Warning);
        }              
    }
}