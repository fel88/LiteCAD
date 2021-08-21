using LiteCAD.BRep.Curves;
using OpenTK;
using System;

namespace LiteCAD.BRep
{
    public class BRepEdge
    {
        public BRepCurve Curve;
        public Vector3d Start { get; set; }
        public Vector3d End { get; set; }

        public double Param1;
        public double Param2;

        bool isEndsEqual(BRepEdge e0, double eps = 1e-8)
        {
            if (Math.Abs((e0.Start - Start).Length) < eps && Math.Abs((e0.End - End).Length) < eps)
                return true;
            if (Math.Abs((e0.End - Start).Length) < eps && Math.Abs((e0.Start - End).Length) < eps)
                return true;
            return false;
        }

        public bool IsSame(BRepEdge e0, double eps = 1e-8)
        {
            if (!isEndsEqual(e0, eps)) return false;
            if (e0.Curve is BRepLineCurve && Curve is BRepLineCurve)
            {
                return true;
            }
            return false;
        }
    }
}