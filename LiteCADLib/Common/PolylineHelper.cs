using BREP.Common;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace LiteCAD.Common
{
    public class PolylineHelper : AbstractDrawable
    {
        public PolylineHelper()
        {

        }
        public bool ShowIndex { get; set; }
        public int LinesCount => Lines.Length;
        public int Index { get; set; }
        public PolylineHelper(Line3D[] ll)
        {
            Lines = ll;
        }
        public Line3D[] Lines { get; set; }

        public override void Draw()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Color3(Color.Blue);

            GL.Begin(PrimitiveType.Lines);
            foreach (var item in Lines)
            {
                GL.Vertex3(item.Start);
                GL.Vertex3(item.End);
            }

            GL.End();
            GL.Disable(EnableCap.DepthTest);
            GL.LineWidth(3);
            if (ShowIndex && Index < Lines.Length)
            {
                GL.Color3(Color.Red);
                GL.Begin(PrimitiveType.Lines);
                for (int i = 0; i <= Index; i++)
                {
                    GL.Vertex3(Lines[i].Start);
                    GL.Vertex3(Lines[i].End);
                }
                GL.End();
            }
            GL.Enable(EnableCap.DepthTest);
        }
    }
}