using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace LiteCAD.BRep
{
    public class PlaneItem : DrawItem
    {
        public Vector3d Dir;
        public Vector3d Position;
        public override void Draw()
        {
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Position);
            GL.Vertex3(Position + Dir * 100);
            GL.End();
            GL.Color3(Color.Blue);
        }
    }
    

}