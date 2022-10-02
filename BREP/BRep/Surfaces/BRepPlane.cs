using OpenTK;
using System;

namespace BREP.BRep.Surfaces
{
    public class BRepPlane : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        public Vector2d GetUVProjPoint(Vector3d point, Vector3d axis1, Vector3d axis2)
        {
            var p = GetProjPoint(point) - Location;
            var p1 = Vector3d.Dot(p, axis1);
            var p2 = Vector3d.Dot(p, axis2);
            return new Vector2d(p1, p2);
        }
        public Vector3d GetProjPoint(Vector3d point)
        {
            var v = point - Location;
            var nrm = Normal;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            return (Math.Abs(Vector3d.Dot(Location - v, Normal)) < eps);
        }

        public override BRepSurface Clone()
        {
            BRepPlane ret = new BRepPlane();
            ret.Location = Location;
            ret.Normal = Normal;
            return ret;
        }

        public override void Transform(Matrix4d mtr4)
        {
            Location = Vector3d.Transform(Location, mtr4);
            var rot = mtr4.ExtractRotation();            
            Normal = Vector3d.Transform(Normal, rot);
        }
    }
}