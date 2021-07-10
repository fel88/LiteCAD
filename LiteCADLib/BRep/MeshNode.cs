using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace LiteCAD.BRep
{
    public class MeshNode 
    {
        public BRepFace Parent;
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();
        /*public override void Draw()
        {
            GL.Begin(PrimitiveType.Triangles);
            foreach (var item in Triangles)
            {
                foreach (var vv in item.Verticies)
                {
                    GL.Vertex3(vv);
                }
            }
            GL.End();
        }*/
    }
    
}