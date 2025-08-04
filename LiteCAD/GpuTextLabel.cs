using LiteCAD.Common;
using OpenTK.Mathematics;
using System.Drawing;

namespace LiteCAD
{
    public class GpuTextLabel
    {
        public void Draw(GpuDrawingContext ctx)
        {
            if (!Visible)
                return;

            ctx.TextRenderer.RenderText(Text, Position.X, Position.Y, Scale, new Vector3()
            {
                X = Color.R / 255.0f,
                Y = Color.G / 255.0f,
                Z = Color.B / 255.0f,
            });
        }
        public Color Color = Color.Blue;
        public float Scale = 1;
        public Vector2 Position { get; set; }

        public string Text { get; set; }
        public bool Visible { get; internal set; } = true;
    }
}