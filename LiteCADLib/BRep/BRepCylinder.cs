﻿using OpenTK;
using System;

namespace LiteCAD.BRep
{
    public class BRepCylinder : BRepSurface
    {
        public Vector3d Location;
        public double Radius;
        public Vector3d Axis;
        public Vector3d RefDir;

        public bool IsOnSurface(Vector3d v, double eps = 1e-8)
        {
            BRepPlane plane = new BRepPlane() { Location = Location, Normal = Axis };
            var proj0 = plane.GetProjPoint(v);
            return Math.Abs(Vector3d.Distance(proj0, Location) - Radius) <= eps;

        }
        public Vector2d GetProj(Vector3d v)
        {
            if (!IsOnSurface(v)) throw new ArgumentException("point is not on surface");
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
    }
}