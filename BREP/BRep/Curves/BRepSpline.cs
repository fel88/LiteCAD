using BREP.Common;
using LiteCAD.Common;
using LiteCADLib.NURBS;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BREP.BRep.Curves
{
    public class BRepSpline : BRepCurve
    {
        public List<Vector3d> Poles = new List<Vector3d>();
        public List<double> Weights = new List<double>();
        public List<double> Knots = new List<double>();
        public List<int> Multiplicities = new List<int>();
        public int Degree;

        public bool IsPolynomial { get; set; }
        public bool IsPeriodic { get; set; }
        public bool IsBSpline { get; set; }
        public bool IsNonPeriodic { get; set; }

        public Vector3d[] CachedPoints;
        public Vector3d[] GetPoints(BRepEdge edge)
        {
            NURBS n = new NURBS();
            n.IsBSpline = IsBSpline;
            double stepSize = 0.01;
            for (int i = 0; i < Poles.Count; i++)
            {
                var vv = Poles[i];
                n.WeightedPointSeries.Add(new RationalBSplinePoint(new Vector3d(vv.X, vv.Y, vv.Z), Weights[i]));
            }

            int deg = Degree;

            //if (n.WeightedPointSeries.Count <= 3) return null;

            List<double> knots = new List<double>();
            for (int i = 0; i < Multiplicities.Count; i++)
            {
                for (int j = 0; j < Multiplicities[i]; j++)
                {
                    knots.Add(Knots[i]);
                }
            }
            if (!IsNonPeriodic)
            {
                knots.Insert(0, 0);
                knots.Add(1);
            }
            var knt = knots.ToArray();
            var first = knt[deg];
            var last = knt[knt.Length - deg - 1];

            var wpnts = n.WeightedPointSeries.ToList();
            if (!IsNonPeriodic)
            {
                wpnts.Add(n.WeightedPointSeries.First());
            }
            stepSize *= Math.Abs(last - first);
            first = edge.Param1;
            last = edge.Param2;
            
            var pnts = n.BSplineCurve(wpnts.ToArray(), deg, knt, stepSize, first, last);
            double epsilon = 1e-3;
            if (!((pnts[0] - edge.Start).Length < epsilon || (pnts[pnts.Length - 1] - edge.Start).Length < epsilon))
            {
                throw new BrepException("wrong bspline points");
            }
            if (!((pnts[0] - edge.End).Length < epsilon || (pnts[pnts.Length - 1] - edge.End).Length < epsilon))
            {
                throw new BrepException("wrong bspline points");
            }
            return pnts;
        }
    }
}
