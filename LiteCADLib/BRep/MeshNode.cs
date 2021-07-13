using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace LiteCAD.BRep
{
    public class MeshNode
    {
        public BRepFace Parent;
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();

        public virtual void SwitchNormal()
        {
            if (!(Parent.Surface is BRepPlane pl)) return;

            foreach (var item in Triangles)
            {
                foreach (var vv in item.Vertices)
                {
                    vv.Normal *= -1;
                }
            }
        }
    }

    public class CylinderMeshNode : MeshNode
    {
        public override void SwitchNormal()
        {
            var cl = Parent.Surface as BRepCylinder;
            foreach (var item in Triangles)
            {
                foreach (var vv in item.Vertices)
                {
                    vv.Normal *= -1;
                }
            }
        }
    }
}