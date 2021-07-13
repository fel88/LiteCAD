using OpenTK;
using System;

namespace LiteCAD.BRep
{
    public class BRepEdge
    {
        public BRepCurve Curve;
        public Vector3d Start { get; set; }
        public Vector3d End { get; set; }

        public bool IsSame(BRepEdge e0, double eps = 1e-8)
        {
            if (e0.Curve is BRepLineCurve && Curve is BRepLineCurve)
            {
                if (Math.Abs((e0.Start - Start).Length) < eps && Math.Abs((e0.End - End).Length) < eps)
                    return true;
                if (Math.Abs((e0.End - Start).Length) < eps && Math.Abs((e0.Start - End).Length) < eps)
                    return true;
            }
            return false;
        }
    }
}