using SkiaSharp;
using System.Drawing;
using System.Windows.Forms;

namespace LiteCAD.BRep.Editor
{
    public class GdiDrawingContext : AbstractDrawingContext
    {
        public Graphics gr;
        public Bitmap Bmp;
        public override void InitGraphics()
        {
            /*            Bmp = new Bitmap(PictureBox.Control.Width, PictureBox.Control.Height);
                        gr = Graphics.FromImage(Bmp);            
                        gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;            */
        }

        public override void FillCircle(Brush brush, float v1, float v2, int rad)
        {
            gr.FillEllipse(brush, v1 - rad, v2 - rad, rad * 2, rad * 2);
        }

        public override void DrawLineTransformed(Pen black, PointF point, PointF point2)
        {
            var tr0 = Transform(point);
            var tr1 = Transform(point2);
            gr.DrawLine(black, tr0, tr1);
        }

        public override void DrawPolygon(Pen p, PointF[] pointFs)
        {

        }

        public override void DrawCircle(Pen pen, float v1, float v2, float rad)
        {

        }

        public override SizeF MeasureString(string text, Font font)
        {
            return gr.MeasureString(text, font);
        }

        public override void DrawString(string text, Font font, Brush brush, PointF position)
        {

        }

        public override void DrawString(string text, Font font, Brush brush, float x, float y)
        {

        }

        public override void DrawLine(Pen black, float x0, float y0, float x1, float y1)
        {
            gr.DrawLine(black, x0, y0, x1, y1);
        }

        public override void DrawLine(Pen black, PointF pp, PointF pp2)
        {
            gr.DrawLine(black, pp, pp2);
        }

        public override void FillRoundRectangle(Brush blue, SKRoundRect rr)
        {

        }

        public override void DrawArrowedLine(Pen p, PointF tr0, PointF tr1, int v)
        {

        }

        public override void DrawRoundRectangle(Pen pen, SKRoundRect rect)
        {

        }

        public override void DrawRectangle(Pen pen, float rxm, float rym, float rdx, float rdy)
        {
            gr.DrawRectangle(pen, rxm, rym, rdx, rdy);
        }

        

        public override void FillRectangle(Brush blue, float v1, float v2, float v3, float v4)
        {

        }

        public override void Clear(Color color)
        {
            gr.Clear(color);
        }

        public override Control GenerateRenderControl()
        {
            PictureBox pb = new PictureBox();
            pb.Paint += Pb_Paint;
            return pb;
        }

        private void Pb_Paint(object sender, PaintEventArgs e)
        {
            gr = e.Graphics;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            PaintAction?.Invoke();
        }
    }
}
