using OpenTK;

namespace BREP.BRep.Curves
{
    public class BRepCircleCurve : BRepCurve
    {
        public double Radius;
        public Vector3d Axis;
        public Vector3d Dir;
        public Vector3d Location;

        public double SweepAngle;

        public override BRepCurve Clone()
        {
            BRepCircleCurve cc = new BRepCircleCurve();

            cc.Radius = Radius;
            cc.Axis = Axis;
            cc.Dir = Dir;
            cc.Location = Location;
            cc.SweepAngle = SweepAngle;

            return cc;
        }

        public override void Transform(Matrix4d mtr4)
        {
            var rot = mtr4.ExtractRotation();
            Axis = Vector3d.Transform(Axis, rot); ;
            Dir = Vector3d.Transform(Dir, rot);
            Location = Vector3d.Transform(Location, mtr4);
            
        }
    }
}