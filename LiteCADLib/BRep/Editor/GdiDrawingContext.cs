using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        public override void DrawLineTransformed(PointF point, PointF point2)
        {
            var tr0 = Transform(point);
            var tr1 = Transform(point2);
            gr.DrawLine(currentPen, tr0, tr1);
        }

        public override void DrawPolygon(Pen p, PointF[] pointFs)
        {

        }

        public override void DrawCircle(Pen pen, float v1, float v2, float rad)
        {
            gr.DrawEllipse(pen, new RectangleF(v1 - rad, v2 - rad, rad * 2, rad * 2));

        }

        public override SizeF MeasureString(string text, Font font)
        {
            return gr.MeasureString(text, font);
        }

        public override void DrawString(string text, Font font, Brush brush, PointF position)
        {
            gr.DrawString(text, font, brush, position);
        }

        public override void DrawString(string text, Font font, Brush brush, float x, float y)
        {
            gr.DrawString(text, font, brush, x, y);
        }

        public override void DrawLine(float x0, float y0, float x1, float y1)
        {
            gr.DrawLine(currentPen, x0, y0, x1, y1);
        }

        public override void DrawLine(PointF pp, PointF pp2)
        {
            gr.DrawLine(currentPen, pp, pp2);
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

        Pen currentPen = Pens.Black;

        public override void SetPen(Pen p)
        {
            currentPen = p;
        }
        public override void DrawRectangle(float rxm, float rym, float rdx, float rdy)
        {
            gr.DrawRectangle(currentPen, rxm, rym, rdx, rdy);
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

        public override void DrawImage(Bitmap image, float x1, float y1, float x2, float y2)
        {
            gr.DrawImage(image, x1, y1, x2 - x1, y2 - y1);
        }

        public override void ResetMatrix()
        {
            gr.ResetTransform();
        }

        public override void RotateDegress(float deg)
        {
            gr.RotateTransform(deg);
        }

        public override void Translate(double x, double y)
        {
            gr.TranslateTransform((float)x, (float)y);
        }
        Stack<Matrix> stack = new Stack<Matrix>();

        public override void PushMatrix()
        {
            stack.Push(gr.Transform);
        }

        public override void PopMatrix()
        {
            gr.Transform = stack.Pop();
        }

        public override void Scale(double x, double y)
        {
            gr.ScaleTransform((float)x, (float)y);
        }
    }
}
