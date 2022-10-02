using BREP.Common;
using LiteCAD.Common;
using OpenTK;
using System;

namespace BREP.BRep.Surfaces
{
    public class BRepCylinder : BRepSurface
    {
        public Vector3d Location;
        public double Radius;
        public Vector3d Axis;
        public Vector3d RefDir;

        public override bool IsOnSurface(Vector3d v, double eps = 1e-3)
        {
            BRepPlane plane = new BRepPlane() { Location = Location, Normal = Axis };
            var proj0 = plane.GetProjPoint(v);
            return Math.Abs(Vector3d.Distance(proj0, Location) - Radius) <= eps;
        }

        public Vector2d GetProj(Vector3d v)
        {
            if (!IsOnSurface(v)) throw new BrepException("point is not on surface");
            BRepPlane plane = new BRepPlane() { Location = Location, Normal = Axis };
            var proj0 = plane.GetProjPoint(v) - Location;
            var p = Vector3d.Dot(v - Location, Axis);
            var t1 = Location + Axis * p;
            var ang = Vector3d.CalculateAngle(proj0, RefDir);
            var crs = Vector3d.Cross(proj0, RefDir);
            if (Vector3d.Dot(Axis, crs) < 0)
            {
                ang = -ang;
            }
            if (ang < 0) ang += Math.PI * 2;
            return new Vector2d(ang, p);
        }

        public override void Transform(Matrix4d mtr4)
        {
            var rot = mtr4.ExtractRotation();
            Location = Vector3d.Transform(Location,mtr4);
            Axis = Vector3d.Transform(Axis, rot);
            RefDir = Vector3d.Transform(RefDir, rot);
        }

        public override BRepSurface Clone()
        {
            BRepCylinder ret = new BRepCylinder();
            ret.Location = Location;
            ret.Radius = Radius;
            ret.Axis = Axis;
            ret.RefDir = RefDir;
            return ret;
        }
    }
}