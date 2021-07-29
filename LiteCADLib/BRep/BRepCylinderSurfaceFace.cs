using IxMilia.Step.Items;
using LiteCAD.Common;
using LiteCAD.Parsers.Step;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LiteCAD.BRep
{
    public class BRepCylinderSurfaceFace : BRepFace
    {
        public BRepCylinderSurfaceFace(Part parent) : base(parent) { }
        public override MeshNode ExtractMesh()
        {
            MeshNode ret = null;
            Vector3d proj1;
            Vector3d v1;

            List<Contour[]> ll = new List<Contour[]>();

            var cl = Surface as BRepCylinder;

            List<Contour> cntrs = new List<Contour>();
            var len = Math.PI * 2 * cl.Radius;

            bool special = false;
            Vector3d vec0 = Vector3d.Zero;
            Vector2d[] seamp = null;
            //special case 1
            /*   if (Wires.Count == 1 && Wires[0].Edges.Count(z => z.Curve is BRepSeamCurve) == 2)
               {
                   special = true;
                   var wr = Wires[0];
                   var cc = wr.Edges.Where(z => z.Curve is BRepCircleCurve).ToArray();

                   var pl0 = cc[0].Curve as BRepCircleCurve;
                   var loc0 = pl0.Location;

                   vec0 = cc[0].Start - loc0;
                   var pl1 = cc[1].Curve as BRepCircleCurve;
                   var loc1 = pl1.Location;

                   var len2 = (loc0 - loc1).Length;
                   int steps = 32;
                   var step1 = len / steps;
                   Contour cc1 = new Contour();
                   List<double> xx = new List<double>();
                   for (int i = 0; i <= steps; i++)
                   {
                       var x = step1 * i;
                       xx.Add((x / len) * 2.0 * Math.PI);
                   }
                   for (int i = 1; i < xx.Count; i++)
                   {
                       var x0 = xx[i - 1];
                       var x1 = xx[i];
                       cc1.Elements.Add(new Segment() { Start = new Vector2d(x0, 0), End = new Vector2d(x1, 0) });
                   }
                   cc1.Elements.Add(new Segment() { Start = cc1.Elements.Last().End, End = new Vector2d(xx[xx.Count - 1], len2) });

                   for (int i = xx.Count - 1; i >= 1; i--)
                   {
                       var x0 = xx[i];
                       var x1 = xx[i - 1];

                       cc1.Elements.Add(new Segment() { Start = new Vector2d(x0, len2), End = new Vector2d(x1, len2) });
                   }
                   cc1.Elements.Add(new Segment() { Start = cc1.Elements.Last().End, End = cc1.Elements.First().Start });
                   cntrs.Add(cc1);
               }
               else*/
            {

                foreach (var wire in Wires)
                {
                    List<Contour> l1 = new List<Contour>();

                    foreach (var edge in wire.Edges)
                    {
                        List<Segment> ll1 = new List<Segment>();
                        if (edge.Curve is BRepEllipseCurve elc)
                        {
                            List<Vector3d> pnts = new List<Vector3d>();
                            var step = Math.PI * 15 / 180f;
                            var norm = elc.Dir.Normalized();
                            var angb = Vector3d.CalculateAngle(norm, elc.RefDir);
                            for (double i = 0; i < elc.SweepAngle; i += step)
                            {
                                var realAng = angb + i;
                                var mtr4 = Matrix4d.CreateFromAxisAngle(elc.Axis, i);
                                var rad = elc.SemiAxis1 * elc.SemiAxis2 / (Math.Sqrt(Math.Pow(elc.SemiAxis1 * Math.Sin(realAng), 2) + Math.Pow(elc.SemiAxis2 * Math.Cos(realAng), 2)));
                                var res = Vector4d.Transform(new Vector4d(norm * rad), mtr4);
                                pnts.Add(elc.Location + res.Xyz);
                            }
                            if ((pnts.Last() - edge.End).Length > 1e-8 && (pnts.First() - edge.End).Length > 1e-8)
                            {
                                pnts.Add(edge.End);
                            }
                            for (int i = 1; i < pnts.Count; i++)
                            {
                                var pp0 = pnts[i - 1];
                                var pp1 = pnts[i];
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
                                if (Math.Abs((p0 - p1).Length - step) > step)
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
                            }
                        }
                        else
                        if (edge.Curve is BRepCircleCurve cc)
                        {
                            List<Vector3d> pnts = new List<Vector3d>();
                            var step = Math.PI * 15 / 180f;
                            for (double i = 0; i < cc.SweepAngle; i += step)
                            {
                                var mtr4 = Matrix4d.CreateFromAxisAngle(cc.Axis, i);
                                var res = Vector4d.Transform(new Vector4d(cc.Dir), mtr4);
                                pnts.Add(cc.Location + res.Xyz);
                            }
                            if ((pnts.Last() - edge.End).Length > 1e-8 && (pnts.First() - edge.End).Length > 1e-8)
                            {
                                pnts.Add(edge.End);
                            }
                            for (int i = 1; i < pnts.Count; i++)
                            {
                                var pp0 = pnts[i - 1];
                                var pp1 = pnts[i];
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
                                if (Math.Abs((p0 - p1).Length - step) > step)
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
                                    p0.X += Math.PI * 2;
                                    p1.X += Math.PI * 2;
                                    Contour aa = new Contour();
                                    aa.Elements.Add(new Segment() { Start = p0, End = p1 });
                                    l1.Add(aa);
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
                                ll1.Add(new Segment() { Start = p0, End = p1 });
                            }
                        }
                        else
                        {
                            DebugHelpers.Warning($"unknown curve: {edge.Curve}");
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
                                        l1.Add(zz);
                                        zz = new Contour() { Wire = wire };
                                        //throw new LiteCadException("wrong elems connect");
                                    }
                                    else
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
            }
            if (!special)
            {
                cntrs = new List<Contour>();
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

                //search connect components and split
                foreach (var item in cntrs)
                {
                    if ((item.Start - item.End).Length > 1e-8)
                    {
                        item.Elements.Add(new Segment() { Start = item.End, End = item.Start });
                    }
                }

                cntrs = cntrs.OrderByDescending(z => GeometryUtils.CalculateArea(z.Elements.Select(u => u.Start).ToArray())).ToList();

                if (cntrs.Count == 0) return null;
                if (!(cntrs[0].Elements.Count > 2)) return null;
                int mult = 1;
                DebugHelpers.ToBitmap(cntrs.ToArray(), mult);

                vec0 = cl.RefDir * cl.Radius;
            }
            //check nesting
            List<Contour> tops = new List<Contour>();
            foreach (var item in cntrs)
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
            }

            List<Vector2d[]> triangls = new List<Vector2d[]>();
            foreach (var item in tops)
            {
                List<Contour> holes = new List<Contour>();
                var pnts2 = item.Elements.Select(z => z.End).ToArray();

                foreach (var xitem in cntrs.Except(tops))
                    if (GeometryUtils.pnpoly(pnts2, xitem.Elements[0].Start.X, xitem.Elements[0].Start.Y))
                        holes.Add(xitem);

                double step = 15 / 180f * Math.PI;

                PolyBoolCS.PolyBool pb = new PolyBoolCS.PolyBool();
                PolyBoolCS.Polygon p1 = new PolyBoolCS.Polygon();
                var pl1 = new PolyBoolCS.PointList();
                p1.regions = new List<PolyBoolCS.PointList>();

                pl1.AddRange(item.Elements.Select(z => z.Start).Select(z => new PolyBoolCS.Point(z.X, z.Y)).ToArray());
                p1.regions.Add(pl1);
                var maxy = pl1.Max(z => z.y) + 1;
                var miny = pl1.Min(z => z.y) - 1;
                double last = 0;
                while (true)
                //for (double i = step; i < (Math.PI * 2); i += step)
                {

                    var p0 = last;
                    var p11 = p0 + step;
                    last += step;

                    p0 = Math.Min(p0, Math.PI * 2);
                    p11 = Math.Min(p11, Math.PI * 2);

                    if (Math.Abs(p0 - p11) < 1e-8) break;


                    PolyBoolCS.Polygon p2 = new PolyBoolCS.Polygon();
                    p2.regions = new List<PolyBoolCS.PointList>();
                    var pl2 = new PolyBoolCS.PointList();

                    pl2.Add(new PolyBoolCS.Point(p0, miny));
                    pl2.Add(new PolyBoolCS.Point(p0, maxy));
                    pl2.Add(new PolyBoolCS.Point(p11, maxy));
                    pl2.Add(new PolyBoolCS.Point(p11, miny));


                    p2.regions.Add(pl2);
                    DebugHelpers.ExecuteSTA(() =>
                    {
                        Clipboard.SetText(p1.ToXml().ToString());
                    });
                    DebugHelpers.ExecuteSTA(() =>
                    {
                        Clipboard.SetText(p2.ToXml().ToString());
                    });

                    var res = pb.intersect(p1, p2);
                    if (res.regions.Any())
                    {
                        foreach (var region in res.regions)
                        {
                            var triangls2 = GeometryUtils.TriangulateWithHoles(
                                new[] { region.Select(z => new Vector2d(z.x, z.y)).ToArray() }
                                ,
                  holes.Select(z => z.Elements.Select(u => u.Start).ToArray()).ToArray(), true);
                            triangls.AddRange(triangls2);
                        }
                    }
                }
            }

            int mult2 = 1;

            DebugHelpers.ToBitmap(cntrs.ToArray(), triangls.ToArray(), mult2);
            DebugHelpers.ToBitmap(cntrs.ToArray(), triangls.ToArray(), mult2, true);

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
                    v.Add(new VertexInfo() { Position = cl.Location + rot0 });
                }
                var v01 = v[1].Position - v[0].Position;
                var v11 = v[2].Position - v[0].Position;
                var crs = Vector3d.Cross(v01, v11).Normalized();
                if (double.IsNaN(crs.X)) throw new LiteCadException("normal is NaN");
                foreach (var item0 in v)
                {
                    item0.Normal = crs;
                }
                tin.Vertices = v.ToArray();

                tt.Add(tin);
            }
            node.Triangles = tt;
            ret = node;
            //ret.SwitchNormal();
            Node = ret;
            return ret;

        }
        public static BRepEdge ExtractEllipseEdge(Vector3d start, Vector3d end1, Vector3d pos, Vector3d axis, Vector3d refDir
            , double radius1, double radius2)
        {
            var edge = new BRepEdge();

            edge.Start = start;
            edge.End = end1;
            var loc1 = pos;

            var dir2 = end1 - pos;
            var dir1 = start - pos;

            var crs = Vector3d.Cross(dir2, dir1);
            var ang2 = Vector3d.CalculateAngle(dir1, dir2);
            if (!(Vector3d.Dot(axis, crs) < 0))
            {
                ang2 = (2 * Math.PI) - ang2;
            }

            var sweep = ang2;
            if ((start - end1).Length < 1e-8)
            {
                sweep = Math.PI * 2;
            }

            edge.Curve = new BRepEllipseCurve()
            {
                Location = loc1,
                SemiAxis1 = radius1,
                SemiAxis2 = radius2,
                RefDir = refDir,
                Axis = axis,
                Dir = dir1,
                SweepAngle = sweep
            };
            return edge;
        }

        public static BRepEdge ExtractCircleEdge(Vector3d start, Vector3d end1, Vector3d pos, Vector3d axis
            , double radius)
        {
            var edge = new BRepEdge();

            edge.Start = start;
            edge.End = end1;
            var loc1 = pos;

            var dir2 = end1 - pos;
            var dir1 = start - pos;

            var crs = Vector3d.Cross(dir2, dir1);
            var ang2 = Vector3d.CalculateAngle(dir1, dir2);
            if (!(Vector3d.Dot(axis, crs) < 0))
            {
                ang2 = (2 * Math.PI) - ang2;
            }

            var sweep = ang2;
            if ((start - end1).Length < 1e-8)
            {
                sweep = Math.PI * 2;
            }

            edge.Curve = new BRepCircleCurve()
            {
                Location = loc1,
                Radius = radius,
                Axis = axis,
                Dir = dir1,
                SweepAngle = sweep
            };
            return edge;
        }

        public override void Load(StepAdvancedFace face, StepSurface _cyl)
        {
            var cyl = _cyl as StepCylindricalSurface;
            var loc = cyl.Position.Location;
            var loc2 = new Vector3d(loc.X, loc.Y, loc.Z);
            var nrm = cyl.Position.Axis;
            var ref1 = cyl.Position.RefDirection;
            var nrm2 = new Vector3d(nrm.X, nrm.Y, nrm.Z);
            var ref2 = new Vector3d(ref1.X, ref1.Y, ref1.Z);
            Surface = new BRepCylinder()
            {
                Location = loc2,
                Radius = cyl.Radius,
                Axis = nrm2,
                RefDir = ref2
            };

            var rad = cyl.Radius;
            foreach (var bitem in face.Bounds)
            {
                BRepWire wire = new BRepWire();
                Wires.Add(wire);
                var loop = bitem.Bound as StepEdgeLoop;
                foreach (var litem in loop.EdgeList)
                {
                    StepEdgeCurve crv = litem.EdgeElement as StepEdgeCurve;

                    var strt = (crv.EdgeStart as StepVertexPoint).Location;
                    var end = (crv.EdgeEnd as StepVertexPoint).Location;
                    var start = new Vector3d(strt.X, strt.Y, strt.Z);
                    var end1 = new Vector3d(end.X, end.Y, end.Z);


                    if (crv.EdgeGeometry is StepCircle circ)
                    {
                        var axis3d = circ.Position as StepAxis2Placement3D;
                        var edge = ExtractCircleEdge(start, end1, circ.Position.Location.ToVector3d(),
                          axis3d.Axis.ToVector3d(), circ.Radius);
                        wire.Edges.Add(edge);
                    }
                    else if (crv.EdgeGeometry is StepLine lin)
                    {
                        var edge = new BRepEdge();
                        wire.Edges.Add(edge);

                        edge.Curve = new BRepLineCurve() { };
                        edge.Start = start;
                        edge.End = end1;

                        /*var c = crv.EdgeStart as StepVertexPoint;
                        var c0 = c.Location;
                        var c1 = crv.EdgeEnd as StepVertexPoint;
                        var c01 = c1.Location;

                        Items.Add(new LineItem()
                        {
                            Start = new Vector3d(
                                c0.X, c0.Y, c0.Z
                            ),
                            End = new Vector3d(
                                c01.X, c01.Y, c01.Z
                            )
                        });*/
                    }
                    else if (crv.EdgeGeometry is StepCurveSurface csurf)
                    {
                        if (csurf.EdgeGeometry is StepCircle circ2)
                        {
                            var axis3d = circ2.Position as StepAxis2Placement3D;
                            var edge = ExtractCircleEdge(start, end1, circ2.Position.Location.ToVector3d(),
                              axis3d.Axis.ToVector3d(), circ2.Radius);
                            wire.Edges.Add(edge);
                        }
                        else if (csurf.EdgeGeometry is StepLine lin2)
                        {
                            var edge = new BRepEdge();
                            wire.Edges.Add(edge);
                            edge.Start = start;
                            edge.End = end1;

                            var pos = new Vector3d(lin2.Point.X,
                              lin2.Point.Y,
                              lin2.Point.Z);
                            var vec = new Vector3d(lin2.Vector.Direction.X,
                          lin2.Vector.Direction.Y,
                          lin2.Vector.Direction.Z);
                            edge.Curve = new BRepLineCurve() { Point = pos, Vector = vec };
                        }
                        else if (csurf.EdgeGeometry is StepEllipse elp)
                        {
                            var edge = new BRepEdge();
                            wire.Edges.Add(edge);
                            edge.Start = start;
                            edge.End = end1;
                            var pos = new Vector3d(elp.Position.Location.X,
                             elp.Position.Location.Y,
                             elp.Position.Location.Z);
                            var vec = new Vector3d(elp.Position.RefDirection.X,
                          elp.Position.RefDirection.Y,
                          elp.Position.RefDirection.Z);
                            edge.Curve = new BRepEllipseCurve()
                            {
                                Location = pos,
                                Dir = vec,
                                SemiAxis1 = elp.SemiAxis1,
                                SemiAxis2 = elp.SemiAxis2
                            };
                        }
                        else if (csurf.EdgeGeometry is StepBSplineCurveWithKnots bspline)
                        {
                            var edge = new BRepEdge();
                            wire.Edges.Add(edge);
                            edge.Start = start;
                            edge.End = end1;
                            edge.Curve = new BRepBSplineWithKnotsCurve()
                            {
                                Degree = bspline.Degree,
                                Closed = bspline.ClosedCurve,
                                KnotMultiplicities = bspline.KnotMultiplicities.ToArray(),
                                Knots = bspline.Knots.ToArray()
                            };
                        }
                        else
                        {
                            DebugHelpers.Warning($"unknown geometry: {csurf.EdgeGeometry}");
                        }
                    }
                    else if (crv.EdgeGeometry is StepSeamCurve seam)
                    {
                        var edge = new BRepEdge();
                        wire.Edges.Add(edge);
                        edge.Curve = new BRepSeamCurve();
                        edge.Start = start;
                        edge.End = end1;
                    }
                    else
                    {

                    }
                }
            }
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
                BRepWire wire = new BRepWire();
                Wires.Add(wire);
                foreach (var litem in item.Loop.Edges)
                {
                    var crv = litem.Curve.EdgeGeometry;
                    var start = litem.Curve.Start.Point;
                    var end = litem.Curve.End.Point;
                    if (crv is SurfaceCurve sc)
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
                            wire.Edges.Add(ExtractCircleEdge(start, end, circ2.Axis.Location,
                             circ2.Axis.Dir1, circ2.Radius));
                        }
                        else
                        if (sc.Geometry is Ellipse elp2)
                        {
                            wire.Edges.Add(ExtractEllipseEdge(start, end, elp2.Axis.Location, elp2.Axis.Dir1, elp2.Axis.Dir2, elp2.MajorRadius, elp2.MinorRadius));
                        }

                        else
                        {
                            throw new StepParserException($"unsupported geometry: {sc.Geometry}");
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
                        if (bcrv.Curves.Any(z => z is RationalBSplineSurface))
                        {
                            var t1 = bcrv.Curves.First(z => z is RationalBSplineSurface) as RationalBSplineSurface;
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
                        Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
                    }
                    else if (crv is Circle circ)
                    {
                        wire.Edges.Add(ExtractCircleEdge(start, end, circ.Axis.Location,
                         circ.Axis.Dir1, circ.Radius));
                    }
                    else
                    {
                        throw new StepParserException($"unsupported curve: {crv}");
                    }
                }
            }

        }
    }
}