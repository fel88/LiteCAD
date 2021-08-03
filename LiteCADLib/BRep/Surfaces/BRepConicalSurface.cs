using OpenTK;

namespace LiteCAD.BRep.Surfaces
{
    public class BRepConicalSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            throw new System.NotImplementedException();
        }
    }
}