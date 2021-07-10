using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace LiteCAD.BRep
{
    public class PointItem : DrawItem
    {

        public Vector3d Position;
        public override void Draw()
        {
            GL.Color3(Color.Yellow);
            GL.PointSize(4);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(Position);
            GL.End();
            GL.Color3(Color.Blue);
        }
    }
    

}