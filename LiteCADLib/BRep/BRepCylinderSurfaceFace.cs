using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

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

            var triangls = GeometryUtils.TriangulateWithHoles(tops.Select(z => z.Elements.Select(u => u.Start).ToArray()).ToArray(),
           cntrs.Except(tops).Select(z => z.Elements.Select(u => u.Start).ToArray()).ToArray(), true);
            int mult2 = 1;

            DebugHelpers.ToBitmap(cntrs.ToArray(), triangls, mult2);
            DebugHelpers.ToBitmap(cntrs.ToArray(), triangls, mult2, true);

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
    }
}