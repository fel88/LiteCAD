using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LiteCAD.BRep
{
    public class LineItem : DrawItem
    {
        public override void Draw()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Start);
            GL.Vertex3(End);
            GL.End();
        }
        public Vector3d Start;
        public Vector3d End;
    }
}