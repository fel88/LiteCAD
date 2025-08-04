using OpenTK;
using OpenTK.Mathematics;

namespace BREP.BRep.Curves
{
    public class BRepLineCurve : BRepCurve
    {
        public Vector3d Point;
        public Vector3d Vector;

        public override BRepCurve Clone()
        {
            BRepLineCurve ret = new BRepLineCurve();
            ret.Point = Point;
            ret.Vector = Vector;
            return ret;
        }

        public override void Transform(Matrix4d mtr4)
        {
            Point = Vector3d.TransformVector(Point, mtr4);
            var p2 = Point + Vector;
            p2 = Vector3d.TransformVector(p2, mtr4);

            Vector = p2 - Point;
        }
    }
}