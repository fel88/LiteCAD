using OpenTK;

namespace BREP.BRep.Curves
{
    public class BRepEllipseCurve : BRepCurve
    {
        public Vector3d Location;
        public Vector3d RefDir;

        public double SemiAxis1;
        public double SemiAxis2;
        
        public Vector3d Axis;
        public Vector3d Dir;        

        public double SweepAngle;
    }
}