using BREP.BRep.Surfaces;
using OpenTK;

namespace BREP.BRep
{
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
        public void SetNormal(TriangleInfo triangleInfo, Vector3d nrm)
        {
            if(Vector3d.Dot(triangleInfo.Vertices[0].Normal, nrm) < 0)
            {
                SwitchNormal();
            }
        }
    }
}