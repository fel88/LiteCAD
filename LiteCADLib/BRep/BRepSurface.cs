using OpenTK;

namespace LiteCAD.BRep
{
    public abstract class BRepSurface
    {
        public abstract bool IsOnSurface(Vector3d v, double eps = 1e-8);
    }
}