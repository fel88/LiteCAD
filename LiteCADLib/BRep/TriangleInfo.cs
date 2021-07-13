using OpenTK;

namespace LiteCAD.BRep
{
    public class TriangleInfo
    {
        public VertexInfo[] Vertices;
        public Vector3d Center()
        {
            Vector3d z1 = Vector3d.Zero;
            foreach (var item in Vertices)
            {
                z1 += item.Position;
            }
            z1 /= 3;
            return z1;
        }
    }
    public class VertexInfo
    {
        public Vector3d Position;
        public Vector3d Normal;
    }

}