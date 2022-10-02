using OpenTK;

namespace BREP.BRep.Surfaces
{
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