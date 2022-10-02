using BREP.BRep;
using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;

namespace LiteCAD
{
    public class IntersectInfo
    {
        public double Distance;
        public TriangleInfo Target;
        public IMeshNodesContainer Model;
        public Vector3d Point { get; set; }
        public object Parent;
    }

}