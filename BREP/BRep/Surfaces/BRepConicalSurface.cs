using BREP.Common;
using LiteCAD.Common;
using OpenTK;
using System;

namespace BREP.BRep.Surfaces
{
    public class BRepConicalSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        public double SemiAngle;
        public double Radius;
        
        public Vector2d GetProj(Vector3d v)
        {
            if (!IsOnSurface(v)) 
                throw new BrepException("point is not on surface");
            /*
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
                        return new Vector2d(ang, p);*/
            throw new NotImplementedException();
        }

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            throw new System.NotImplementedException();
        }
    }
}