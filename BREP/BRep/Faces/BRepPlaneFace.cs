using BREP.BRep.Curves;
using BREP.BRep.Outlines;
using BREP.BRep.Surfaces;
using BREP.Common;
using BREP.Parsers.Step;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BREP.BRep.Faces
{
    public class BRepPlaneFace : BRepFace
    {
        public BRepPlaneFace(Part p) : base(p) { }
        public BRepPlaneFace()  { }
        public override MeshNode ExtractMesh()
        {
            MeshNode ret = null;
            Vector3d proj1;
            Vector3d v1;

            List<Contour[]> ll = new List<Contour[]>();

            var pl = (Surface as BRepPlane);

            if (Wires.Count == 0 || Wires.Sum(z => z.Edges.Count) == 0) return null;
            //var fr = Wires.First(z => z.Edges.Any(u => u.Curve is BRepLineCurve));
            var fr = Wires.First(z => z.Edges.Any());
            var efr = fr.Edges.First();
            var dir = efr.End - efr.Start;
            if (efr.Curve is BRepCircleCurve ccc)
            {
                dir = efr.Start - ccc.Location;
            }
            else
            if (efr.Curve is BRepEllipseCurve elc)
            {
                dir = efr.Start - elc.Location;
            }
            proj1 = pl.GetProjPoint(pl.Location + dir);

            v1 = (proj1 - pl.Location).Normalized();

            if (double.IsNaN(v1.X))
            {
                throw new BrepException("normal is NaN");
            }
            var axis2 = Vector3d.Cross(pl.Normal, v1).Normalized();
            foreach (var wire in Wires)
            {
                List<Contour> l1 = new List<Contour>();

                foreach (var edge in wire.Edges)
                {
                    List<Segment> ll1 = new List<Segment>();
                    if (edge.Curve is BRepLineCurve lc)
                    {
                        var p0 = pl.GetUVProjPoint(edge.Start, v1, axis2);
                        var p1 = pl.GetUVProjPoint(edge.End, v1, axis2);
                        ll1.Add(new Segment() { Start = p0, End = p1 });
                    }
                    else if (edge.Curve is BRepCircleCurve cc)
                    {
                        var step = Math.PI * 15 / 180f;
                        var pnts = GeometryUtils.ExtractPoints(edge, cc, step);
                        /*List<Vector3d> pnts = new List<Vector3d>();
                        
                        for (double i = 0; i < cc.SweepAngle; i += step)
                        {
                            var mtr4 = Matrix4d.CreateFromAxisAngle(cc.Axis, i);
                            var res = Vector4d.Transform(new Vector4d(cc.Dir), mtr4);
                            pnts.Add(cc.Location + res.Xyz);
                        }
                        pnts.Add(edge.End);*/
                        for (int i = 1; i < pnts.Length; i++)
                        {
                            var pp0 = pnts[i - 1];
                            var pp1 = pnts[i];
                            var p0 = pl.GetUVProjPoint(pp0, v1, axis2);
                            var p1 = pl.GetUVProjPoint(pp1, v1, axis2);
                            ll1.Add(new Segment() { Start = p0, End = p1 });
                        }
                    }
                    else if (edge.Curve is BRepBSplineWithKnotsCurve spline)
                    {
                        List<Vector3d> pnts = new List<Vector3d>();
                        pnts.Add(edge.Start);
                        pnts.AddRange(spline.ControlPoints);
                        pnts.Add(edge.End);
                        for (int i = 1; i < pnts.Count; i++)
                        {
                            var pp0 = pnts[i - 1];
                            var pp1 = pnts[i];
                            var p0 = pl.GetUVProjPoint(pp0, v1, axis2);
                            var p1 = pl.GetUVProjPoint(pp1, v1, axis2);
                            ll1.Add(new Segment() { Start = p0, End = p1 });
                        }
                    }
                    else if (edge.Curve is BRepEllipseCurve elc)
                    {
                        List<Vector3d> pnts = new List<Vector3d>();
                        var step = Math.PI * 5 / 180f;
                        var norm = elc.Dir.Normalized();
                        var maxr = Math.Max(elc.SemiAxis1, elc.SemiAxis2);
                        var minr = Math.Min(elc.SemiAxis1, elc.SemiAxis2);
                        elc.SemiAxis1 = maxr;
                        elc.SemiAxis2 = minr;
                        //var angb = Vector3d.CalculateAngle(norm, elc.RefDir);
                        for (double i = 0; i < elc.SweepAngle; i += step)
                        {                            
                            var mtr4 = Matrix4d.CreateFromAxisAngle(elc.Axis, i);
                            
                            var res = Vector4d.Transform(new Vector4d(norm ), mtr4);
                            var realAng = Vector3d.CalculateAngle(res.Xyz, elc.RefDir);

                            var rad = elc.SemiAxis1 * elc.SemiAxis2 / (Math.Sqrt(Math.Pow(elc.SemiAxis1 * Math.Sin(realAng), 2) + Math.Pow(elc.SemiAxis2 * Math.Cos(realAng), 2)));
                            res *= rad;

                            pnts.Add(elc.Location + res.Xyz);
                        }
                        pnts.Add(edge.End);
                        for (int i = 1; i < pnts.Count; i++)
                        {
                            var pp0 = pnts[i - 1];
                            var pp1 = pnts[i];
                            var p0 = pl.GetUVProjPoint(pp0, v1, axis2);
                            var p1 = pl.GetUVProjPoint(pp1, v1, axis2);
                            ll1.Add(new Segment() { Start = p0, End = p1 });
                        }
                    }
                    else if (edge.Curve is BRepSpline spl)
                    {
                        var pnts = spl.GetPoints(edge);
                        var projs = pnts.Select(z => pl.GetUVProjPoint(z, v1, axis2)).ToArray();
                        for (int j = 1; j < projs.Length; j++)
                        {
                            var p0 = projs[j - 1];
                            var p1 = projs[j];
                            ll1.Add(new Segment() { Start = p0, End = p1 });
                        }
                    }
                    else
                    {
                        throw new BrepException("unsupported curve type");
                    }
                    if (ll1.Any())
                    {
                        var zz = new Contour() { Wire = wire };
                        var arr3 = ll1.ToList();
                        while (true)
                        {
                            var nl = zz.ConnectNext(arr3.ToArray());
                            if (nl == null)
                            {
                                if (arr3.Any())
                                {
                                    throw new BrepException("");
                                }
                                break;
                            }
                            arr3.Remove(nl);
                        }

                        l1.Add(zz);
                    }
                }
                if (l1.Any())
                {
                    ll.Add(l1.ToArray());
                }
            }
            List<Contour> cntrs = new List<Contour>();
            foreach (var item in ll)
            {
                Contour ccn = new Contour();
                var ar = item.ToList();
                while (true)
                {
                    var res = ccn.ConnectNext(ar.ToArray());

                    if (res == null)
                    {
                        if (ar.Any())
                        {
                            throw new BrepException("bad contour");
                        }
                        break;
                    }
                    ar.Remove(res);
                }
                cntrs.Add(ccn);
            }
            cntrs = cntrs.OrderByDescending(z => GeometryUtils.CalculateArea(z.Elements.Select(u => u.Start).ToArray())).ToList();

            if (cntrs.Count == 0) return null;
            if (!(cntrs[0].Elements.Count > 2)) return null;
            int mult = 1;
            DebugHelpers.ToBitmap(cntrs.ToArray(), mult);

            var triangls = GeometryUtils.TriangulateWithHoles(new[] { cntrs[0].Elements.Select(z => z.Start).ToArray() },
                cntrs.Skip(1).Select(z => z.Elements.Select(u => u.Start).ToArray()).ToArray(), true);
            DebugHelpers.ToBitmap(cntrs.ToArray(), triangls.ToArray(), mult);

            ret = new MeshNode();
            ret.Parent = this;
            List<TriangleInfo> tt = new List<TriangleInfo>();
            foreach (var item in triangls)
            {
                tt.Add(new TriangleInfo()
                {
                    Vertices = item.Select(z => new VertexInfo()
                    {
                        Position = z.X * v1 + z.Y * axis2 + pl.Location,
                        Normal = -pl.Normal
                    }).ToArray()
                });
            }
            ret.Triangles.AddRange(tt);

            Node = ret;
            return ret;
        }

        public override Line3D[] Get3DSegments(BRepEdge edge, double eps = 1E-08)
        {
            List<Line3D> ret = new List<Line3D>();
            if (edge.Curve is BRep.Curves.BRepLineCurve ll)
            {
                ret.Add(new Line3D() { Start = edge.Start, End = edge.End });
            }
            if (edge.Curve is BRep.Curves.BRepCircleCurve cc)
            {
                var step = Math.PI * 15 / 180f;
                var pnts = GeometryUtils.ExtractPoints(edge, cc, step);
                for (int i = 1; i < pnts.Length; i++)
                {
                    ret.Add(new Line3D() { Start = pnts[i-1], End = pnts[i]});
                }                
            }
            return ret.ToArray();
        }

        public BRepEdge ExtractCircleEdge(Vector3d start, Vector3d end1, double radius,
            Vector3d axis, Vector3d location)
        {
            BRepEdge edge = new BRepEdge();

            var rad = radius;
            var cc = new BRepCircleCurve();
            edge.Curve = cc;
            cc.Radius = rad;
            var pos = location;
            edge.Start = start;
            edge.End = end1;

            //var axis3d = circ.Axis as Axis2Placement3d;
            //var axis = new Vector3d(axis3d.Dir1.X, axis3d.Dir1.Y, axis3d.Dir1.Z);
            //var refdir = new Vector3d(axis3d.RefDirection.X, axis3d.RefDirection.Y, axis3d.RefDirection.Z);
            /*var pos = new Vector3d(circ.Axis.Location.X,
                circ.Axis.Location.Y,
                circ.Axis.Location.Z);*/
            var dir1 = start - pos;
            var dir2 = end1 - pos;
            List<Vector3d> pnts = new List<Vector3d>();



            var dot = Vector3d.Dot(dir2, dir1);
            var crs = Vector3d.Cross(dir2, dir1);
            var ang2 = Vector3d.CalculateAngle(dir1, dir2);

            if (!(Vector3d.Dot(axis, crs) < 0))
            {
                ang2 = (2 * Math.PI) - ang2;
            }

            pnts.Add(pos + dir1);
            cc.Axis = axis;
            cc.Dir = dir1;
            cc.Location = pos;
            cc.SweepAngle = ang2;
            if ((start - end1).Length < 1e-8)
            {
                cc.SweepAngle = Math.PI * 2;
            }
            var step = Math.PI * 15 / 180f;
            for (double i = 0; i < ang2; i += step)
            {
                var mtr4 = Matrix4d.CreateFromAxisAngle(axis, i);
                var res = Vector4d.Transform(new Vector4d(dir1), mtr4);
                pnts.Add(pos + res.Xyz);
            }
            pnts.Add(pos + dir2);
            for (int j = 1; j < pnts.Count; j++)
            {
                var p0 = pnts[j - 1];
                var p1 = pnts[j];
                Items.Add(new LineItem() { Start = p0, End = p1 });
            }
            return edge;
        }

        public virtual BRepEdge ExtractEllipseEdge(
            Vector3d start,
            Vector3d end1,
            double radius1,
            double radius2,
            Vector3d axis,
            Vector3d refDir,
            Vector3d location)
        {
            BRepEdge edge = new BRepEdge();

            var cc = new BRepEllipseCurve();
            edge.Curve = cc;
            cc.SemiAxis1 = radius1;
            cc.SemiAxis2 = radius2;
            var pos = location;
            edge.Start = start;
            edge.End = end1;

            var dir1 = start - pos;
            var dir2 = end1 - pos;
            List<Vector3d> pnts = new List<Vector3d>();

            var dot = Vector3d.Dot(dir2, dir1);
            var crs = Vector3d.Cross(dir2, dir1);
            var ang2 = Vector3d.CalculateAngle(dir1, dir2);
            var ang3 = Vector3d.CalculateAngle(dir1, refDir);

            if (!(Vector3d.Dot(axis, crs) < 0))
            {
                ang2 = (2 * Math.PI) - ang2;
            }

            pnts.Add(pos + dir1);
            cc.Axis = axis;
            cc.Dir = dir1;
            cc.RefDir = refDir;
            cc.Location = pos;
            cc.SweepAngle = ang2;
            if ((start - end1).Length < 1e-8)
            {
                cc.SweepAngle = Math.PI * 2;
            }
            var maxr = Math.Max(radius1, radius2);
            var len = Math.PI * 2 * maxr;
            var step = Math.PI * 5 / 180f;
            var norm = dir1.Normalized();
            for (double i = 0; i < ang2; i += step)
            {
                var mtr4 = Matrix4d.CreateFromAxisAngle(axis, i);
                //var rad = (radius1 * radius2) / (Math.Sqrt(radius2 * radius2 * Math.Pow(Math.Sin(i), 2)) + Math.Sqrt(radius1 * radius1 * Math.Pow(Math.Cos(i), 2)));
                var res = Vector4d.Transform(new Vector4d(norm ), mtr4);
                var realAng = Vector3d.CalculateAngle(res.Xyz, cc.RefDir);
                var rad = (radius1 * radius2) / (Math.Sqrt(Math.Pow(radius1 * Math.Sin(realAng), 2) + Math.Pow(radius2 * Math.Cos(realAng), 2)));

                res *= rad;

                pnts.Add(pos + res.Xyz);

                /*var mtr4 = Matrix4d.CreateFromAxisAngle(elc.Axis, i);
                            
                            var res = Vector4d.Transform(new Vector4d(norm ), mtr4);
                            var realAng = Vector3d.CalculateAngle(res.Xyz, elc.RefDir);

                            var rad = elc.SemiAxis1 * elc.SemiAxis2 / (Math.Sqrt(Math.Pow(elc.SemiAxis1 * Math.Sin(realAng), 2) + Math.Pow(elc.SemiAxis2 * Math.Cos(realAng), 2)));
                            res *= rad;

                            pnts.Add(elc.Location + res.Xyz);
                 */
            }
            pnts.Add(pos + dir2);
            for (int j = 1; j < pnts.Count; j++)
            {
                var p0 = pnts[j - 1];
                var p1 = pnts[j];
                Items.Add(new LineItem() { Start = p0, End = p1 });
            }
            return edge;
        }

        public BRepPlane Plane => Surface as BRepPlane;
        public override void Load(AdvancedFace face)
        {
            Surface = new BRepPlane() { Location = face.Surface.Axis.Location, Normal = face.Surface.Axis.Dir1 };
            foreach (var item in face.Bounds)
            {
                BRepWire wire = new BRepWire();
                Wires.Add(wire);
                foreach (var litem in item.Loop.Edges)
                {
                    var start = litem.Curve.Start.Point;
                    var end = litem.Curve.End.Point;
                    var crv = litem.Curve.EdgeGeometry;
                    if (crv is SurfaceCurve sc)
                    {
                        if (sc.Geometry is Line ln2)
                        {
                            BRepEdge edge = new BRepEdge();
                            edge.Curve = new BRepLineCurve() { Point = ln2.Point, Vector = ln2.Vector.Location };
                            wire.Edges.Add(edge);
                            edge.Start = litem.Curve.Start.Point;
                            edge.End = litem.Curve.End.Point;
                            Outlines.Add(new LineItem() { Start = edge.Start, End = edge.End });
                        }
                        else
                        if (sc.Geometry is Circle circ2)
                        {
                            wire.Edges.Add(ExtractCircleEdge(start, end, circ2.Radius, circ2.Axis.Dir1, circ2.Axis.Location));
                        }
                        else
                        if (sc.Geometry is Ellipse elp2)
                        {
                            wire.Edges.Add(ExtractEllipseEdge(
                                start,
                                end,
                                elp2.MinorRadius,
                                elp2.MajorRadius,
                                elp2.Axis.Dir1,
                                elp2.Axis.Dir2,
                                elp2.Axis.Location));
                        }
                        else
                        {
                            throw new StepParserException($"unknown geometry: {sc.Geometry}");
                        }
                    }
                    else if (crv is Line ln)
                    {
                        BRepEdge edge = new BRepEdge();
                        edge.Curve = new BRepLineCurve() { Point = ln.Point, Vector = ln.Vector.Location };
                        wire.Edges.Add(edge);
                        edge.Start = litem.Curve.Start.Point;
                        edge.End = litem.Curve.End.Point;
                        Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
                    }
                    else if (crv is Circle circ)
                    {
                        wire.Edges.Add(ExtractCircleEdge(start, end, circ.Radius, circ.Axis.Dir1, circ.Axis.Location));
                    }
                    else if (crv is Ellipse elp)
                    {
                        wire.Edges.Add(ExtractEllipseEdge(
                               start,
                               end,
                               elp.MinorRadius,
                               elp.MajorRadius,
                               elp.Axis.Dir1,
                               elp.Axis.Dir2,
                               elp.Axis.Location));
                    }
                    else if (crv is BSplineCurveWithKnots bcrv)
                    {
                        BRepEdge edge = new BRepEdge();
                        edge.Start = start;
                        edge.End = end;
                        BRepSpline spl = new BRepSpline();

                        edge.Curve = spl;

                        spl.Multiplicities = bcrv.Multiplicities.ToList();
                        spl.Poles = bcrv.ControlPoints.ToList();
                        spl.Knots = bcrv.Knots.ToList();
                        spl.Weights = bcrv.Weights.ToList();
                        edge.Param1 = bcrv.Param1;
                        edge.Param2 = bcrv.Param2;
                        spl.IsBSpline = true;
                        spl.Degree = bcrv.Degree[0];

                        if (spl.Poles.Count == (spl.Multiplicities.Sum() - spl.Degree - 1))
                        {
                            spl.IsNonPeriodic = true;
                        }
                        if (spl.Poles.Count == (spl.Multiplicities.Sum() - spl.Multiplicities.First()))
                        {
                            spl.IsPeriodic = true;
                        }

                        var pnts = spl.GetPoints(edge);

                        for (int j = 1; j < pnts.Length; j++)
                        {
                            var p0 = pnts[j - 1];
                            var p1 = pnts[j];
                            Items.Add(new LineItem() { Start = p0, End = p1 });
                        }

                        wire.Edges.Add(edge);
                    }
                    else if (crv is BoundedCurve bcrv2)
                    {
                        BRepEdge edge = new BRepEdge();
                        edge.Start = start;
                        edge.End = end;
                        BRepSpline spl = new BRepSpline();
                        edge.Curve = spl;
                        if (bcrv2.Curves.Any(z => z is BSplineCurveWithKnots))
                        {
                            var t1 = bcrv2.Curves.First(z => z is BSplineCurveWithKnots) as BSplineCurveWithKnots;
                            spl.Knots = new List<double>() { t1.Param1, t1.Param2 };
                            spl.Multiplicities = t1.Degree.ToList();
                            edge.Param1 = t1.Param1;
                            edge.Param2 = t1.Param2;
                        }
                        if (bcrv2.Curves.Any(z => z is BSplineCurve))
                        {
                            var t1 = bcrv2.Curves.First(z => z is BSplineCurve) as BSplineCurve;
                            spl.Poles = t1.Poles.ToList();
                            spl.Degree = t1.Degree;
                        }
                        if (bcrv2.Curves.Any(z => z is RationalBSplineCurve))
                        {
                            var t1 = bcrv2.Curves.First(z => z is RationalBSplineCurve) as RationalBSplineCurve;
                            spl.Weights = t1.Weights.ToList();
                        }

                        if (spl.Poles.Count == (spl.Multiplicities.Sum() - spl.Degree - 1))
                        {
                            spl.IsNonPeriodic = true;
                        }
                        if (spl.Poles.Count == (spl.Multiplicities.Sum() - spl.Multiplicities.First()))
                        {
                            spl.IsPeriodic = true;
                        }

                        var pnts = spl.GetPoints(edge);

                        for (int j = 1; j < pnts.Length; j++)
                        {
                            var p0 = pnts[j - 1];
                            var p1 = pnts[j];
                            Items.Add(new LineItem() { Start = p0, End = p1 });
                        }

                        wire.Edges.Add(edge);
                    }
                    else
                    {
                        throw new StepParserException($"unknown curve: {crv}");
                    }
                }
            }
            base.Load(face);
        }

        public override BRepFace Clone()
        {
            BRepPlaneFace ret = new BRepPlaneFace();
            ret.Surface = Surface.Clone();            
            foreach (var item in Wires)
            {
                ret.Wires.Add(item.Clone());
            }
            return ret;
        }
        public override void Transform(Matrix4d mtr4)
        {
            Surface.Transform(mtr4);
            foreach (var item in Wires)
            {
                item.Transform(mtr4);
            }
        }
    }
}