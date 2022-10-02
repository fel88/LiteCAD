using BREP.BRep.Curves;
using OpenTK;
using System;

namespace BREP.BRep
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

        internal BRepEdge Clone()
        {
            BRepEdge edge = new BRepEdge();
            edge.Start = Start;
            edge.End = End;
            edge.Param1 = Param1;
            edge.Param2 = Param2;
            edge.Curve = Curve.Clone();
            return edge;
        }

        public void Transform(Matrix4d mtr4)
        {
            Start = Vector3d.Transform(Start, mtr4);
            End = Vector3d.Transform(End, mtr4);
            Curve.Transform(mtr4);
        }
    }
}