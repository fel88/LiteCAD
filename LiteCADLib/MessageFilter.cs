using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiteCAD
{
    public class MessageFilter : IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEWHEEL:
                case WM_MOUSEHWHEEL:
                    IntPtr hControlUnderMouse = WindowFromPoint(new System.Drawing.Point((int)m.LParam));
                    if (hControlUnderMouse == m.HWnd)
                    {
                        return false;
                    }
                    else
                    {
                        uint u = Convert.ToUInt32(m.Msg);
                        SendMessage(hControlUnderMouse, u, m.WParam, m.LParam);
                        return true;
                    }
                default:
                    return false;
            }
        }
    }
}
