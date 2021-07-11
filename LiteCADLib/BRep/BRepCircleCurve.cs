using OpenTK;

namespace LiteCAD.BRep
{
    public class BRepCircleCurve : BRepCurve
    {
        public double Radius;
        public Vector3d Axis;
        public Vector3d Dir;
        public Vector3d Location;

        public double SweepAngle;
    }
    public class BRepSeamCurve : BRepCurve
    {
        
    }
}