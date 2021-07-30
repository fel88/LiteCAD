using OpenTK;

namespace LiteCAD.BRep
{
    public class BRepToroidalSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal; 
        public double MajorRadius { get; set; }
        public double MinorRadius { get; set; }

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            throw new System.NotImplementedException();
        }
    }
    public class BRepConicalSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            throw new System.NotImplementedException();
        }
    }
    public class BRepBSplineWithKnotsSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        public int uDegree;
        public int vDegree;
        public Vector3d[][] ControlPoints;

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            throw new System.NotImplementedException();
        }
    }
}