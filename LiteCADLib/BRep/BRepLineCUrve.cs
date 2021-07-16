using OpenTK;
using System.Collections.Generic;

namespace LiteCAD.BRep
{
    public class BRepLineCurve : BRepCurve
    {
        public Vector3d Point;
        public Vector3d Vector;
    }
    public class BRepEllipseCurve : BRepCurve
    {
        public Vector3d Location;
        public Vector3d RefDir;

        public double SemiAxis1;
        public double SemiAxis2;
    }
    public class BRepBSplineWithKnotsCurve : BRepCurve
    {
        public Vector3d Location;
        public Vector3d RefDir;
        public int Degree;
        public bool Closed;
        public int[] KnotMultiplicities;

        public double[] Knots;


    }
}