using LiteCAD.BRep.Curves;
using LiteCAD.BRep.Surfaces;
using LiteCAD.Common;
using LiteCAD.Parsers.Step;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LiteCAD.BRep.Faces
{
    public class BRepCylinderSurfaceFace : BRepFace
    {
        public BRepCylinderSurfaceFace(Part parent) : base(parent) { }

        Segment[] getSegments(Vector3d pp0, Vector3d pp1, double step = 0.1)
        {
            var cl = Surface as BRepCylinder;
            List<Segment> ll1 = new List<Segment>();
            var p0 = cl.GetProj(pp0);
            var p1 = cl.GetProj(pp1);
            if (p0.X >= (Math.PI * 2 - 2 * step) && p1.X < 2 * step)
            {
                var dy = p1.Y - p0.Y;
                var dx = (p1.X + Math.PI * 2) - p0.X;
                var tx = (Math.PI * 2 - p0.X) / dx;
                var ty = dy * tx;

                ll1.Add(new Segment() { Start = p0, End = new Vector2d(Math.PI * 2, ty + p0.Y) });
                ll1.Add(new Segment() { Start = new Vector2d(0, ty + p0.Y), End = p1 });
                ll1.RemoveAll(z => z.Length() < 1e-16);

            }
            else if (p1.X >= (Math.PI * 2 - 2 * step) && p0.X < 2 * step)
            {
                var dy = p0.Y - p1.Y;
                var dx = (p0.X + Math.PI * 2) - p1.X;
                var tx = (Math.PI * 2 - p1.X) / dx;
                var ty = dy * tx;

                ll1.Add(new Segment() { Start = p1, End = new Vector2d(Math.PI * 2, ty + p1.Y) });
                ll1.Add(new Segment() { Start = new Vector2d(0, ty + p1.Y), End = p0 });
                ll1.RemoveAll(z => z.Length() < 1e-16);
            }
            else
            if (Math.Abs(Math.Abs(p0.X - p1.X) - step) > step)
            {

                if (Math.Abs(p0.X) < 1e-16)
                {
                    p0.X += Math.PI * 2;
                    if (Math.Abs((p0 - p1).Length - step) > step)
                    {
                        throw new LiteCadException("wrong angles");
                    }
                    else
                    {
                        ll1.Add(new Segment() { Start = p0, End = p1 });
                    }
                }
                else if (Math.Abs(p1.X) < 1e-16)
                {
                    p1.X += Math.PI * 2;
                    if (Math.Abs((p0 - p1).Length - step) > step)
                    {
                        throw new LiteCadException("wrong angles");

                    }
                    else
                    {
                        ll1.Add(new Segment() { Start = p0, End = p1 });
                    }
                }
                else
                {
                    throw new LiteCadException("wrong angles");

                }
            }
            else
            {
                ll1.Add(new Segment() { Start = p0, End = p1 });
            }
            return ll1.ToArray();
        }
        public override Line3D[] Get3DSegments(BRepEdge edge, double eps = 1e-8)
        {
            var cl = Cylinder;
            List<Line3D> ll1 = new List<Line3D>();
           
            if (edge.Curve is BRepEllipseCurve elc)
            {
                var norm = elc.Dir.Normalized();
                List<Vector3d> pnts = new List<Vector3d>();
                //check
                var mtr44 = Matrix4d.CreateFromAxisAngle(elc.Axis, elc.SweepAngle);
                var res44 = Vector4d.Transform(new Vector4d(norm), mtr44);
                var realAng2 = Vector3d.CalculateAngle(res44.Xyz, elc.RefDir);
                var rad2 = elc.SemiAxis1 * elc.SemiAxis2 / (Math.Sqrt(Math.Pow(elc.SemiAxis1 * Math.Sin(realAng2), 2) + Math.Pow(elc.SemiAxis2 * Math.Cos(realAng2), 2)));
                res44 *= rad2;
                var checkPoint = (elc.Location + res44.Xyz);
                var realAxis = elc.Axis;
                if ((checkPoint - edge.End).Length > 1e-5)
                {
                    //try to fix, by switch rotation
                    realAxis *= -1;
                }

                var step = Math.PI * 5 / 180f;

                var angb = Vector3d.CalculateAngle(norm, elc.RefDir);
                for (double i = 0; i < elc.SweepAngle; i += step)
                {
                    var mtr4 = Matrix4d.CreateFromAxisAngle(realAxis, i);
                    var res = Vector4d.Transform(new Vector4d(norm), mtr4);
                    var realAng = Vector3d.CalculateAngle(res.Xyz, elc.RefDir);
                    var rad = elc.SemiAxis1 * elc.SemiAxis2 / (Math.Sqrt(Math.Pow(elc.SemiAxis1 * Math.Sin(realAng), 2) + Math.Pow(elc.SemiAxis2 * Math.Cos(realAng), 2)));
                    res *= rad;
                    pnts.Add(elc.Location + res.Xyz);
                }

                //check #2
                mtr44 = Matrix4d.CreateFromAxisAngle(realAxis, elc.SweepAngle);
                res44 = Vector4d.Transform(new Vector4d(norm), mtr44);
                realAng2 = Vector3d.CalculateAngle(res44.Xyz, elc.RefDir);
                rad2 = elc.SemiAxis1 * elc.SemiAxis2 / (Math.Sqrt(Math.Pow(elc.SemiAxis1 * Math.Sin(realAng2), 2) + Math.Pow(elc.SemiAxis2 * Math.Cos(realAng2), 2)));
                res44 *= rad2;
                checkPoint = (elc.Location + res44.Xyz);

                if ((checkPoint - edge.End).Length > 1e-5)
                {
                    throw new LiteCadException("wrong end point");
                }

                if ((pnts.First() - edge.Start).Length > eps)
                {
                    throw new LiteCadException("wrong start point");
                }

                if ((pnts.Last() - edge.End).Length > eps && (pnts.First() - edge.End).Length > eps)
                {
                    pnts.Add(edge.End);
                }
                for (int i = 1; i < pnts.Count; i++)
                {
                    var pp0 = pnts[i - 1];
                    var pp1 = pnts[i];
                    ll1.Add(new Line3D() { Start = pp0, End = pp1 });
                }
                //ll1.RemoveAll(z => z.Length() < 1e-16);
            }
            else if (edge.Curve is BRepCircleCurve cc)
            {
                var step0 = Math.PI * 5 / 180f;
                //better step required.
                var len = Math.PI * 2 * cc.Radius;
                var step = (Math.PI * 2) / len;
                if (step > step0)
                {
                    step = step0;
                }
                var pnts = GeometryUtils.ExtractPoints(edge, cc, step);
                for (int i = 1; i < pnts.Length; i++)
                {
                    var pp0 = pnts[i - 1];
                    var pp1 = pnts[i];
                    ll1.Add(new Line3D() { Start = pp0, End = pp1 });
                }
            }
            else if (edge.Curve is BRepLineCurve lnc)
            {                
                ll1.Add(new Line3D() { Start = edge.Start, End = edge.End });
            }
            else if (edge.Curve is BRepBSplineWithKnotsCurve bspline)
            {
                ll1.Add(new Line3D() { Start = edge.Start, End = edge.End });                
            }
            else if (edge.Curve is BRepSeamCurve seam)
            {
                //skip?
                //ll1.Add(new Line3D() { Start = edge.Start, End = edge.End });                
            }
            else if (edge.Curve is BRepSpline spl)
            {
                var pnts = spl.GetPoints(edge);
                var projs = pnts.Select(z => cl.GetProj(z)).ToArray();
                for (int i = 1; i < pnts.Length; i++)
                {
                    var pp0 = pnts[i - 1];
                    var pp1 = pnts[i];
                    ll1.Add(new Line3D() { Start = pp0, End = pp1 });
                }
            }
            else
            {
                DebugHelpers.Warning($"unknown curve: {edge.Curve}");
            }
            return ll1.ToArray();
        }
        Segment[] getSegments(BRepEdge edge, double eps = 1e-8)
        {
            var cl = Cylinder;
            Vector2d[] seamp = null;

            List<Segment> ll1 = new List<Segment>();
            if (edge.Curve is BRepEllipseCurve elc)
            {
                List<Vector3d> pnts = new List<Vector3d>();
                var step = Math.PI * 5 / 180f;
                var norm = elc.Dir.Normalized();
                var angb = Vector3d.CalculateAngle(norm, elc.RefDir);
                for (double i = 0; i < elc.SweepAngle; i += step)
                {
                    var mtr4 = Matrix4d.CreateFromAxisAngle(elc.Axis, i);
                    var res = Vector4d.Transform(new Vector4d(norm), mtr4);
                    var realAng = Vector3d.CalculateAngle(res.Xyz, elc.RefDir);
                    var rad = elc.SemiAxis1 * elc.SemiAxis2 / (Math.Sqrt(Math.Pow(elc.SemiAxis1 * Math.Sin(realAng), 2) + Math.Pow(elc.SemiAxis2 * Math.Cos(realAng), 2)));
                    res *= rad;
                    pnts.Add(elc.Location + res.Xyz);
                }
                if ((pnts.First() - edge.Start).Length > eps)
                {
                    throw new LiteCadException("wrong point");
                }
                if ((pnts.Last() - edge.End).Length > eps && (pnts.First() - edge.End).Length > eps)
                {
                    pnts.Add(edge.End);
                }
                for (int i = 1; i < pnts.Count; i++)
                {
                    var pp0 = pnts[i - 1];
                    var pp1 = pnts[i];
                    ll1.AddRange(getSegments(pp0, pp1, step));
                }
                ll1.RemoveAll(z => z.Length() < 1e-16);
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
                if ((pnts.Last() - edge.End).Length > 1e-8 && (pnts.First() - edge.End).Length > 1e-8)
                {
                    pnts.Add(edge.End);
                }*/
                for (int i = 1; i < pnts.Length; i++)
                {
                    var pp0 = pnts[i - 1];
                    var pp1 = pnts[i];
                    var p0 = cl.GetProj(pp0);
                    var p1 = cl.GetProj(pp1);

                        ll1.Add(new Segment() { Start = p0, End = p1 });
                    }
                }
            else if (edge.Curve is BRepLineCurve lnc)
            {
                var p0 = cl.GetProj(edge.Start);
                var p1 = cl.GetProj(edge.End);
                ll1.Add(new Segment() { Start = p0, End = p1 });
            }
            else if (edge.Curve is BRepBSplineWithKnotsCurve bspline)
            {
                var p0 = cl.GetProj(edge.Start);
                var p1 = cl.GetProj(edge.End);
                ll1.Add(new Segment() { Start = p0, End = p1 });
            }
            else if (edge.Curve is BRepSeamCurve seam)
            {
                if (seamp == null)
                {
                    var p0 = cl.GetProj(edge.Start);
                    var p1 = cl.GetProj(edge.End);

                    seamp = new[] { p0, p1 };
                    ll1.Add(new Segment() { Start = p0, End = p1 });
                    if (Math.Abs(p0.X) < 1e-16 && Math.Abs(p1.X) < 1e-16)
                    {
                        //  p0.X += Math.PI * 2;
                        //  p1.X += Math.PI * 2;
                        // Contour aa = new Contour();
                        //   aa.Elements.Add(new Segment() { Start = p0, End = p1 });
                        //   l1.Add(aa);
                    }
                }
            }
            else if (edge.Curve is BRepSpline spl)
            {
                var pnts = spl.GetPoints(edge);
                var projs = pnts.Select(z => cl.GetProj(z)).ToArray();
                for (int j = 1; j < projs.Length; j++)
                {
                    var p0 = projs[j - 1];
                    var p1 = projs[j];
                    double step = 0.1;
                    if (p0.X >= (Math.PI * 2 - 2 * step) && p1.X < 2 * step)
                    {
                        var dy = p1.Y - p0.Y;
                        var dx = (p1.X + Math.PI * 2) - p0.X;
                        var tx = (Math.PI * 2 - p0.X) / dx;
                        var ty = dy * tx;

                        ll1.Add(new Segment() { Start = p0, End = new Vector2d(Math.PI * 2, ty + p0.Y) });
                        ll1.Add(new Segment() { Start = new Vector2d(0, ty + p0.Y), End = p1 });
                        ll1.RemoveAll(z => z.Length() < 1e-16);

                    }
                    else if (p1.X >= (Math.PI * 2 - 2 * step) && p0.X < 2 * step)
                    {
                        var dy = p0.Y - p1.Y;
                        var dx = (p0.X + Math.PI * 2) - p1.X;
                        var tx = (Math.PI * 2 - p1.X) / dx;
                        var ty = dy * tx;

                        ll1.Add(new Segment() { Start = p1, End = new Vector2d(Math.PI * 2, ty + p1.Y) });
                        ll1.Add(new Segment() { Start = new Vector2d(0, ty + p1.Y), End = p0 });
                        ll1.RemoveAll(z => z.Length() < 1e-16);
                    }
                    else

                        ll1.Add(new Segment() { Start = p0, End = p1 });
                }
            }
            else
            {
                DebugHelpers.Warning($"unknown curve: {edge.Curve}");
            }
            return ll1.ToArray();
        }

        public BRepCylinder Cylinder
        {
            get
            {
                return Surface as BRepCylinder;
            }
        }

        Contour[] getContours(BRepWire wire)
        {
            List<Contour> l1 = new List<Contour>();

            float feps = 1e-5f;
            foreach (var edge in wire.Edges)
            {
                var segs = getSegments(edge);
              
                var zz2 = new Contour() { Wire = wire };
                foreach (var sitem in segs)
                {
                    if (zz2.Elements.Any() && (zz2.End - sitem.Start).Length > feps)
                    {
                        throw new LiteCadException("wrong dist");
                    }
                    zz2.Elements.Add(sitem);
                }
                l1.Add(zz2);
                //if (false)
                //    if (segs.Any())
                //    {
                //        var zz = new Contour() { Wire = wire };
                //        var arr3 = segs.ToList();
                //        while (true)
                //        {
                //            var nl = zz.ConnectNext(arr3.ToArray());
                //            if (nl == null)
                //            {
                //                if (arr3.Any())
                //                {
                //                    l1.Add(zz);
                //                    zz = new Contour() { Wire = wire };
                //                    //throw new LiteCadException("wrong elems connect");
                //                }
                //                else
                //                    break;
                //            }
                //            arr3.Remove(nl);
                //        }

                //        l1.Add(zz);
                //    }
            }
            return l1.ToArray();
        }
        public override MeshNode ExtractMesh()
        {
            MeshNode ret = null;
            Vector3d proj1;
            Vector3d v1;

            List<Line3D[]> ll = new List<Line3D[]>();

            var cl = Cylinder;

            List<Contour> cntrs = new List<Contour>();
            var len = Math.PI * 2 * cl.Radius;
            bool special = false;
            Vector3d vec0 = Vector3d.Zero;

            foreach (var wire in Wires)
            {
                //find edges order
                //if (!wire.IsClosed()) continue;
                foreach (var edge in wire.Edges)
                {
                    var l1 = Get3DSegments(edge);
                    if (l1.Any())
                    {
                        ll.Add(l1.ToArray());
                    }
                }
            }

            List<Vector2d[]> triangls = new List<Vector2d[]>();

            var allLines = ll.SelectMany(z => z).ToArray();

            double step = 5 * Math.PI / 180f;
            for (double i = 0; i < (Math.PI * 2 + step); i += step)
            {

                try
                {


                    if (i > Math.PI * 2) break;
                    var startAng = i;
                    var endAng = Math.Min(i + step, Math.PI * 2);
                    List<Line3D> cands = new List<Line3D>();
                    foreach (var line in allLines)
                    {
                        //proj to cylinder surface and get ang
                        var ps = cl.GetProj(line.Start);
                        var pe = cl.GetProj(line.End);

                        if (ps.X < 0 || pe.X < 0 || ps.X > Math.PI * 2 || pe.X > Math.PI * 2)
                        {

                        }
                        //fix ang to [0;pi*2]
                        float gap = 0.02f;
                        if ((ps.X >= (startAng - gap) && ps.X <= (endAng + gap)) || (pe.X >= (startAng - gap) && pe.X <= (endAng + gap)))
                        {
                            cands.Add(line);
                        }
                        //intersects all lines with vertical lines to get new intersections points
                    }


                    //make contours
                    List<Contour> rawContours = new List<Contour>();

                    if (cands.Any())
                    {
                        Contour cc = new Contour();
                        rawContours.Add(cc);
                        var projs = cands.SelectMany(z => new[] { cl.GetProj(z.Start), cl.GetProj(z.End) }).ToArray();
                        var maxy = projs.Max(z => z.Y);
                        var miny = projs.Min(z => z.Y);

                        foreach (var cand in cands)
                        {
                            //cut any segment with vertical lines
                            var ss = cl.GetProj(cand.Start);
                            var ee = cl.GetProj(cand.End);

                            //check diff
                            if (Math.Abs(ss.X - ee.X) > Math.PI)
                            {
                                //fix
                                if (GeometryUtils.AlmostEqual(ss.X, 0))
                                {
                                    ss.X = Math.PI * 2;
                                }
                                if (GeometryUtils.AlmostEqual(ee.X, 0))
                                {
                                    ee.X = Math.PI * 2;
                                }
                            }

                            Vector2d res1 = new Vector2d();
                            Vector2d res2 = new Vector2d();
                            var inter1 = GeometryUtils.IntersectSegments(ss, ee,
                                new Vector2d(startAng, miny - 5), new Vector2d(startAng, maxy + 5), ref res1);
                            var inter2 = GeometryUtils.IntersectSegments(ss, ee,
                                new Vector2d(endAng, miny - 5), new Vector2d(endAng, maxy + 5), ref res2);


                            var ord1 = new[] { ss, ee }.OrderBy(z => z.X).ToArray();
                            ss = ord1[0];
                            ee = ord1[1];
                            if (inter1)
                            {
                                ss = res1;
                            }
                            if (inter2)
                            {
                                ee = res2;
                            }

                            cc.Elements.Add(new Segment() { Start = ss, End = ee });
                        }
                        List<Segment> toAdd = new List<Segment>();
                        var jj = cc.Elements.SelectMany(z => new[] { z.Start, z.End }).ToArray();
                        var uniqs = jj.GroupBy(z => GeometryUtils.PointHashKey(z, 10000)).Select(z => z.First()).ToArray();
                        float eps2 = 1e-5f;
                        var leftSide = uniqs.Where(z => Math.Abs(z.X - startAng) < eps2).OrderBy(z => z.Y).ToArray();
                        var rightSide = uniqs.Where(z => Math.Abs(z.X - endAng) < eps2).OrderBy(z => z.Y).ToArray();
                        if (leftSide.Length % 2 != 0 || rightSide.Length % 2 != 0)
                        {
                            //bad
                            DebugHelpers.Error("Cylinder mesh extraction: projection bad parity");
                            throw new LiteCadException("Cylinder mesh extraction: projection bad parity");

                        }
                        for (int j = 0; j < leftSide.Length; j += 2)
                        {
                            var j1 = leftSide[j];
                            var j2 = leftSide[j + 1];
                            //check thi segment already exists
                            cc.Elements.Add(new Segment() { Start = j1, End = j2 });
                        }
                        for (int j = 0; j < rightSide.Length; j += 2)
                        {
                            var j1 = rightSide[j];
                            var j2 = rightSide[j + 1];
                            //check thi segment already exists
                            cc.Elements.Add(new Segment() { Start = j1, End = j2 });
                        }
                        cc.Elements = cc.Elements.GroupBy(z => GeometryUtils.SegmentHashKeyInvariant(z, 10000)).Select(z => z.First()).ToList();
                        //remove same lines
                        var mult = 100;
                        DebugHelpers.ToBitmap(rawContours.ToArray(), mult);
                        //extract contours here
                    }

                    //var allElems = rawContours.SelectMany(z => z.Elements).ToArray();
                    foreach (var item in rawContours)
                    {
                        Contour ccn = new Contour();
                        var ar = item.Elements.ToList();
                        while (true)
                        {
                            var res = ccn.ConnectNext(ar.ToArray());

                            if (res == null)
                            {
                                if (ar.Any())
                                {
                                    //throw new LiteCadException("bad contour");
                                    ccn.Reduce();
                                    cntrs.Add(ccn);
                                    ccn = new Contour();
                                }
                                else
                                    break;
                            }
                            ar.Remove(res);
                        }
                        ccn.Reduce();
                        cntrs.Add(ccn);
                    }
                    //connectNext contours here
                    //cntrs.AddRange(rawContours);

                    //break;
                }
                catch (Exception ex)
                {

                }
            }
            cntrs = cntrs.Where(z => z.Elements.Count > 3).OrderByDescending(z => GeometryUtils.CalculateArea(z.Elements.Select(u => u.Start).ToArray())).ToList();

            //check nesting
            List<Contour> tops = new List<Contour>();
            tops = cntrs;
            /*foreach (var item in cntrs)
            {
                bool good = true;
                foreach (var item2 in cntrs)
                {
                    if (item == item2) continue;
                    var pnts2 = item2.Elements.Select(z => z.End).ToArray();

                    if (GeometryUtils.pnpoly(pnts2, item.Elements[0].Start.X, item.Elements[0].Start.Y))
                    {
                        good = false; break;
                    }
                }
                if (good) tops.Add(item);
            }*/
            foreach (var item in tops)
            {
                try
                {
                    List<Contour> holes = new List<Contour>();
                    var pnts2 = item.Elements.Select(z => z.End).ToArray();

                    foreach (var xitem in cntrs.Except(tops))
                        if (GeometryUtils.pnpoly(pnts2, xitem.Elements[0].Start.X, xitem.Elements[0].Start.Y))
                            holes.Add(xitem);

                    //double step = 15 / 180f * Math.PI;

                    PolyBoolCS.PolyBool pb = new PolyBoolCS.PolyBool();
                    PolyBoolCS.Polygon p1 = new PolyBoolCS.Polygon();
                    var pl1 = new PolyBoolCS.PointList();
                    p1.regions = new List<PolyBoolCS.PointList>();

                    pl1.AddRange(item.Elements.Select(z => z.Start).Select(z => new PolyBoolCS.Point(z.X, z.Y)).ToArray());
                    p1.regions.Add(pl1);
                    var maxy = pl1.Max(z => z.y) + 1;
                    var miny = pl1.Min(z => z.y) - 1;
                    double last = 0;
                    //if (item.Elements.Count > 3)
                    holes.Clear();
                    {
                        var triangls2 = GeometryUtils.TriangulateWithHoles(
                                       new[] { item.Elements.Select(u => u.Start).ToArray() }
                                        ,
                          holes.Select(z => z.Elements.Select(u => u.Start).ToArray()).ToArray(), true);
                        triangls.AddRange(triangls2);

                    }
                }
                catch (Exception ex)
                {

                }

            }
            vec0 = cl.RefDir * cl.Radius;
            //transform back 2d->3d
            CylinderMeshNode node = new CylinderMeshNode();
            node.Parent = this;
            List<TriangleInfo> tt = new List<TriangleInfo>();
            foreach (var item in triangls)
            {
                TriangleInfo tin = new TriangleInfo();
                List<VertexInfo> v = new List<VertexInfo>();
                foreach (var d in item)
                {
                    //var ang = d.X * Math.PI * 2;
                    var ang = d.X;
                    var mtr = Matrix4d.CreateFromAxisAngle(cl.Axis, -ang);

                    var rot0 = Vector3d.Transform(vec0 + cl.Axis * d.Y, mtr);
                    PlaneHelper ph = new PlaneHelper();
                    //ph.Position = cl.Location;
                    ph.Normal = cl.Axis;
                    var projn = ph.GetProjPoint(rot0);
                    //var norm = rot0.Normalized();
                    var norm = projn.Normalized();
                    v.Add(new VertexInfo() { Position = cl.Location + rot0, Normal = norm });
                }
                var v01 = v[1].Position - v[0].Position;
                var v11 = v[2].Position - v[0].Position;
                var crs = Vector3d.Cross(v01, v11).Normalized();

                if (double.IsNaN(crs.X))
                {
                    //   throw new LiteCadException("normal is NaN");
                }

                /*foreach (var item0 in v)
                {
                    item0.Normal = crs;
                }*/
                tin.Vertices = v.ToArray();

                tt.Add(tin);
            }
            node.Triangles = tt;
            ret = node;
            //ret.SwitchNormal();
            Node = ret;
            return ret;
        }

        public BRepEdge ExtractEllipseEdge(Vector3d start, Vector3d end1, Vector3d pos, Vector3d axis, Vector3d refDir
            , double radius1, double radius2, bool edgeOrientation)
        {
            var edge = new BRepEdge();

            if (!edgeOrientation)
            {
                axis *= -1;
            }


            edge.Start = start;
            edge.End = end1;
            var loc1 = pos;

            var dir2 = end1 - pos;
            var dir1 = start - pos;

            //var crs = Vector3d.Cross(dir2, dir1);
            //var ang2 = Vector3d.CalculateAngle(dir1, dir2);
            var ang2 = GeometryUtils.CalculateAngle(dir1, dir2, axis);

            /*if (!(Vector3d.Dot(axis, crs) < 0))
            {
                ang2 = (2 * Math.PI) - ang2;
            }*/

            var sweep = ang2;
            if ((start - end1).Length < 1e-8)
            {
                sweep = Math.PI * 2;
            }

            var cc = new BRepEllipseCurve()
            {
                Location = loc1,
                SemiAxis1 = radius1,
                SemiAxis2 = radius2,
                RefDir = refDir,
                Axis = axis,
                Dir = dir1,
                SweepAngle = sweep
            };
            edge.Curve = cc;
            List<Vector3d> pnts = new List<Vector3d>();
            var step = Math.PI * 5 / 180f;
            var norm = cc.Dir.Normalized();
            var angb = Vector3d.CalculateAngle(norm, cc.RefDir);
            var maxr = Math.Max(cc.SemiAxis1, cc.SemiAxis2);
            var minr = Math.Min(cc.SemiAxis1, cc.SemiAxis2);
            cc.SemiAxis1 = maxr;
            cc.SemiAxis2 = minr;
            for (double i = 0; i < cc.SweepAngle; i += step)
            {
                var mtr4 = Matrix4d.CreateFromAxisAngle(cc.Axis, i);
                var res = Vector4d.Transform(new Vector4d(norm), mtr4);
                var realAng = Vector3d.CalculateAngle(res.Xyz, cc.RefDir);
                var rad = cc.SemiAxis1 * cc.SemiAxis2 / (Math.Sqrt(Math.Pow(cc.SemiAxis1 * Math.Sin(realAng), 2) + Math.Pow(cc.SemiAxis2 * Math.Cos(realAng), 2)));

                res *= rad;
                pnts.Add(cc.Location + res.Xyz);
            }

            pnts.Add(edge.End);

            for (int j = 1; j < pnts.Count; j++)
            {
                var p0 = pnts[j - 1];
                var p1 = pnts[j];
                Items.Add(new LineItem() { Start = p0, End = p1 });
            }
            return edge;
        }

        public const int AngleResolution = 15;

        public BRepEdge ExtractCircleEdge(Vector3d start, Vector3d end1, Vector3d pos, Vector3d axis
            , double radius, bool edgeOrientation, bool sameSense)
        {

            /*if (!edgeOrientation)
        {
                var temp = end1;
                end1 = start;
                start = temp;
            }
            if (!sameSense)
            {
                var temp = end1;
                end1 = start;
                start = temp;
            }*/
            var edge = new BRepEdge();

            edge.Start = start;
            edge.End = end1;
            var loc1 = pos;

            var dir2 = end1 - pos;
            var dir1 = start - pos;


            if (!edgeOrientation)
            {
                axis *= -1;
            }
            var ang2 = GeometryUtils.CalculateAngle(dir1, dir2, axis);

            var sweep = ang2;
            if ((start - end1).Length < 1e-8)
            {
                sweep = Math.PI * 2;
            }
          
            var cc = new BRepCircleCurve()
            {
                Location = loc1,
                Radius = radius,
                Axis = axis,
                Dir = dir1,
                SweepAngle = sweep
            };
            edge.Curve = cc;

            List<Vector3d> pnts = new List<Vector3d>();
            var len = cc.SweepAngle * radius;
            double precision = 1;//1mm
            len /= precision;

            //var step = Math.PI * AngleResolution / 180f;
            var step = ((Math.PI * 2) / (len / precision));
            var axis1 = cc.Axis;
            /*if (!edgeOrientation)
            {
                axis1 *= -1;
            }*/
            for (double i = 0; i < cc.SweepAngle; i += step)
            {
                var mtr4 = Matrix4d.CreateFromAxisAngle(axis1, i);
                var res = Vector4d.Transform(new Vector4d(cc.Dir), mtr4);
                pnts.Add(cc.Location + res.Xyz);
            }
            if ((pnts.Last() - edge.End).Length > 1e-8 && (pnts.First() - edge.End).Length > 1e-8)
            {
                pnts.Add(edge.End);
            }
            for (int j = 1; j < pnts.Count; j++)
            {
                var p0 = pnts[j - 1];
                var p1 = pnts[j];
                Items.Add(new LineItem() { Start = p0, End = p1 });
            }
            return edge;
        }


        BRepWire extractWire(FaceBound bound)
        {
            BRepWire wire = new BRepWire();
            foreach (var litem in bound.Loop.Edges)
            {
                var crv = litem.Curve.EdgeGeometry;
                var start = litem.Curve.Start.Point;
                var end = litem.Curve.End.Point;
                if (!litem.Curve.SameSense)
                {
                    start = litem.Curve.End.Point;
                    end = litem.Curve.Start.Point;
                }
                if (!litem.Orientation)
                {
                    var temp = start;
                    start = end;
                    end = temp;
                }
                if (crv is BSplineCurveWithKnots bsk)
                {

                    BRepEdge edge = new BRepEdge();
                    edge.Start = start;
                    edge.End = end;
                    BRepSpline spl = new BRepSpline();

                    edge.Curve = spl;

                    spl.Multiplicities = bsk.Multiplicities.ToList();
                    spl.Poles = bsk.ControlPoints.ToList();
                    spl.Knots = bsk.Knots.ToList();
                    spl.Weights = bsk.Weights.ToList();
                    edge.Param1 = bsk.Param1;
                    edge.Param2 = bsk.Param2;
                    spl.IsBSpline = true;
                    spl.Degree = bsk.Degree[0];

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
                else if (crv is SurfaceCurve sc)
                {
                    if (sc.Geometry is Line ln2)
                    {
                        BRepEdge edge = new BRepEdge();
                        edge.Curve = new BRepLineCurve() { Point = ln2.Point, Vector = ln2.Vector.Location };
                        wire.Edges.Add(edge);
                        edge.Start = litem.Curve.Start.Point;
                        edge.End = litem.Curve.End.Point;
                        Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
                    }
                    else
                    if (sc.Geometry is Circle circ2)
                    {
                        var ss = litem.Curve.SameSense;
                        wire.Edges.Add(ExtractCircleEdge(start, end, circ2.Axis.Location,
                         circ2.Axis.Dir1, circ2.Radius, litem.Orientation, ss));
                    }
                    else
                    if (sc.Geometry is Ellipse elp2)
                    {
                        wire.Edges.Add(ExtractEllipseEdge(start, end, elp2.Axis.Location, elp2.Axis.Dir1, elp2.Axis.Dir2, elp2.MajorRadius, elp2.MinorRadius, litem.Orientation));
                    }
                    else
                    if (sc.Geometry is BSplineCurveWithKnots bcrv2)
                    {
                        BRepEdge edge = new BRepEdge();
                        edge.Start = start;
                        edge.End = end;
                        BRepSpline spl = new BRepSpline();

                        edge.Curve = spl;

                        spl.Multiplicities = bcrv2.Multiplicities.ToList();
                        spl.Poles = bcrv2.ControlPoints.ToList();
                        spl.Knots = bcrv2.Knots.ToList();
                        spl.Weights = bcrv2.Weights.ToList();
                        edge.Param1 = bcrv2.Param1;
                        edge.Param2 = bcrv2.Param2;
                        spl.IsBSpline = true;
                        spl.Degree = bcrv2.Degree[0];

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
                        throw new UnsupportedCurveException($"unsupported geometry: {sc.Geometry}");
                    }
                }
                else if (crv is BoundedCurve bcrv)
                {
                    BRepEdge edge = new BRepEdge();
                    edge.Start = start;
                    edge.End = end;
                    BRepSpline spl = new BRepSpline();
                    edge.Curve = spl;
                    if (bcrv.Curves.Any(z => z is BSplineCurveWithKnots))
                    {
                        var t1 = bcrv.Curves.First(z => z is BSplineCurveWithKnots) as BSplineCurveWithKnots;
                        spl.Knots = new List<double>() { t1.Param1, t1.Param2 };
                        spl.Multiplicities = t1.Degree.ToList();
                        edge.Param1 = t1.Param1;
                        edge.Param2 = t1.Param2;
                    }
                    if (bcrv.Curves.Any(z => z is BSplineCurve))
                    {
                        var t1 = bcrv.Curves.First(z => z is BSplineCurve) as BSplineCurve;
                        spl.Poles = t1.Poles.ToList();
                        spl.Degree = t1.Degree;
                    }
                    if (bcrv.Curves.Any(z => z is RationalBSplineCurve))
                    {
                        var t1 = bcrv.Curves.First(z => z is RationalBSplineCurve) as RationalBSplineCurve;
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
                else if (crv is Line ln)
                {
                    BRepEdge edge = new BRepEdge();
                    edge.Curve = new BRepLineCurve() { Point = ln.Point, Vector = ln.Vector.Location };
                    wire.Edges.Add(edge);
                    edge.Start = litem.Curve.Start.Point;
                    edge.End = litem.Curve.End.Point;
                    Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
                }
                else if (crv is SeamCurve seam)
                {
                    BRepEdge edge = new BRepEdge();
                    edge.Curve = new BRepSeamCurve() { };
                    wire.Edges.Add(edge);
                    edge.Start = litem.Curve.Start.Point;
                    edge.End = litem.Curve.End.Point;
                    var es = edge.Start;
                    var ee = edge.End;
                    if (!litem.Orientation)
                    {
                        es = edge.End;
                        ee = edge.Start;
                        edge.End = litem.Curve.Start.Point;
                        edge.Start = litem.Curve.End.Point;
                    }
                    Items.Add(new LineItem() { Start = es, End = ee });
                }
                else if (crv is Circle circ)
                {
                    var ss = litem.Curve.SameSense;
                    wire.Edges.Add(ExtractCircleEdge(start, end, circ.Axis.Location,
                     circ.Axis.Dir1, circ.Radius, litem.Orientation, ss));
                }
                else
                {
                    throw new UnsupportedCurveException($"unsupported curve: {crv}");
                }
            }
            return wire;
        }
        public override void Load(AdvancedFace face)
        {
            var ss = (face.Surface as CylindricalSurface);

            Surface = new BRepCylinder()
            {
                Location = face.Surface.Axis.Location,
                Radius = ss.Radius,
                Axis = ss.Axis.Dir1,
                RefDir = ss.Axis.Dir2
            };
            foreach (var item in face.Bounds)
            {
                try
                {
                    Wires.Add(extractWire(item));
                }
                catch (Exception ex) when (BRepFace.SkipWireOnParseException)
                {

                }
            }
            base.Load(face);
        }
    }
}