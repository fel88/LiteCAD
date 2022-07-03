using OpenTK;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LiteCAD.BRep.Editor
{
    public interface IDrawingContext
    {
        event Action<float, float, MouseButtons> MouseUp;
        event Action<float, float, MouseButtons> MouseDown;
        void DrawPolygon(Pen p, PointF[] pointFs);
        bool MiddleDrag { get; }
        Action PaintAction { get; set; }
        MouseButtons DragButton { get; set; }
        PointF GetCursor();
        void Init(Control pb);

        PointF Transform(PointF p1);
        PointF Transform(float x, float y);
        PointF Transform(double x, double y);
        PointF Transform(Vector2d p1);
        void UpdateDrag();
        Control GenerateRenderControl();
        void ResetView();
        object Tag { get; set; }
        EventWrapperPictureBox PictureBox { get; set; }
        void InitGraphics();
        bool isMiddleDrag { get; set; }
        bool isLeftDrag { get; set; }
        float startx { get; set; }
        float starty { get; set; }
        float sx { get; set; }
        float sy { get; set; }

        void FitToPoints(PointF[] points, int gap = 0);

        void DrawLineTransformed(Pen black, PointF point, PointF point2);

        PointF BackTransform(PointF p1);
        PointF BackTransform(float x, float y);
        float zoom { get; set; }
        void FillCircle(Brush brush, float v1, float v2, int rad);
        void DrawCircle(Pen pen, float v1, float v2, float rad);
        SizeF MeasureString(string text, Font font);
        void DrawString(string text, Font font, Brush brush, PointF position);
        void DrawString(string text, Font font, Brush brush, float x, float y);
        void DrawLine(Pen black, float x0, float y0, float x1, float y1);
        void Clear(Color white);
        void DrawLine(Pen black, PointF pp, PointF pp2);
        void FillRoundRectangle(Brush blue, SKRoundRect rr);
        void DrawArrowedLine(Pen p, PointF tr0, PointF tr1, int v);
        void DrawRoundRectangle(Pen pen, SKRoundRect rect);
        void DrawRectangle(Pen pen, float rxm, float rym, float rdx, float rdy);

        void FillRectangle(Brush blue, float v1, float v2, float v3, float v4);




    }
}
