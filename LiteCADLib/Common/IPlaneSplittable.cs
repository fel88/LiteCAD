using BREP.Common;

namespace LiteCAD.Common
{
    public interface IPlaneSplittable
    {
        Line3D[] SplitPyPlane(PlaneHelper ph);
    }
}