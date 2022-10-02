using OpenTK;

namespace BREP.BRep.Surfaces
{
    public class BRepLinearExtrusionSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        public double Length;
        public Vector3d Vector;

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            throw new System.NotImplementedException();
        }
    }
}