using OpenTK;

namespace BREP.BRep.Curves
{
    public class BRepBSplineWithKnotsCurve : BRepCurve
    {
        public Vector3d Location;
        public Vector3d RefDir;
        public int Degree;
        public bool Closed;
        public int[] KnotMultiplicities;
        public Vector3d[] ControlPoints;

        public double[] Knots;                

    }

    
}