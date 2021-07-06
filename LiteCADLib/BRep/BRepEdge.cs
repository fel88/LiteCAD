using OpenTK;

namespace LiteCAD.BRep
{
    public class BRepEdge
    {
        public BRepCurve Curve;
        public Vector3d Start { get; set; }
        public Vector3d End { get; set; }
    }
}