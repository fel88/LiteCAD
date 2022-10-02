using BREP.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LiteCAD.Common
{
    public class LineHelper : AbstractDrawable
    {
        public LineHelper()
        {

        }
        public LineHelper(Line3D ll)
        {
            Start = ll.Start;
            End = ll.End;
        }
        public Vector3d Start { get; set; }
        public Vector3d End { get; set; }
        public override void Draw()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Start);
            GL.Vertex3(End);
            GL.End();
        }
    }
}