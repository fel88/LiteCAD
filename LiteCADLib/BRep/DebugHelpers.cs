using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LiteCAD.BRep
{
    public static class DebugHelpers
    {

        public static Action<string> Error;
        
        public static void ToBitmap(Contour cntr)
        {
            if (!Debugger.IsAttached) return;
            Bitmap bmp = new Bitmap(1000, 1000);
            var gr = Graphics.FromImage(bmp);
            var maxx = cntr.Elements.Max(z => Math.Max(z.Start.X, z.End.X));
            var minx = cntr.Elements.Min(z => Math.Min(z.Start.X, z.End.X));
            var maxy = cntr.Elements.Max(z => Math.Max(z.Start.Y, z.End.Y));
            var miny = cntr.Elements.Min(z => Math.Min(z.Start.Y, z.End.Y));
            var dx = (float)(maxx - minx);
            var dy = (float)(maxy - miny);
            var mdx = Math.Max(dx, dy);
            foreach (var cc in cntr.Elements)
            {
                var x1 = (float)(cc.Start.X - minx);
                x1 = (x1 / mdx) * (bmp.Width - 1);
                var y1 = (float)(cc.Start.Y - miny);
                y1 = (y1 / mdx) * (bmp.Height - 1);
                var x2 = (float)(cc.End.X - miny);
                x2 = (x2 / mdx) * (bmp.Width - 1);
                var y2 = (float)(cc.End.Y - miny);
                y2 = (y2 / mdx) * (bmp.Height - 1);

                gr.DrawLine(Pens.Black, x1, y1, x2, y2);
            }
            Thread thread = new Thread(() => Clipboard.SetImage(bmp)
            );
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

        }
    }
}