using OpenTK;

namespace LiteCAD.BRep
{
    public class BRepEllipseCurve : BRepCurve
    {
        public Vector3d Location;
        public Vector3d RefDir;

        public double SemiAxis1;
        public double SemiAxis2;
    }
}