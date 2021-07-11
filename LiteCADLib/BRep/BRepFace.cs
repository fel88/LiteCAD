using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.BRep
{
    public class BRepFace
    {
        public List<DrawItem> Items = new List<DrawItem>();
        public BRepSurface Surface;
        public List<BRepWire> Wires = new List<BRepWire>();
        public BRepWire OutterWire;

        public MeshNode ExtractMesh()
        {
            MeshNode ret = null;
            Vector3d proj1;
            Vector3d v1;

            List<Contour[]> ll = new List<Contour[]>();

            if (Surface is BRepPlane pl)
            {
                if (Wires.Count == 0 || Wires.Sum(z => z.Edges.Count) == 0) return null;
                //var fr = Wires.First(z => z.Edges.Any(u => u.Curve is BRepLineCurve));
                var fr = Wires.First(z => z.Edges.Any());
                var efr = fr.Edges.First();
                var dir = efr.End - efr.Start;
                if (efr.Curve is BRepCircleCurve ccc)
                {
                    dir = efr.Start - ccc.Location;
                }
                proj1 = pl.GetProjPoint(pl.Location + dir);

                v1 = (proj1 - pl.Location).Normalized();

                if (double.IsNaN(v1.X))
                {
                    throw new LiteCadException("normal is NaN");
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
                            List<Vector3d> pnts = new List<Vector3d>();
                            var step = Math.PI * 15 / 180f;
                            for (double i = 0; i < cc.SweepAngle; i += step)
                            {
                                var mtr4 = Matrix4d.CreateFromAxisAngle(cc.Axis, i);
                                var res = Vector4d.Transform(new Vector4d(cc.Dir), mtr4);
                                pnts.Add(cc.Location + res.Xyz);
                            }
                            pnts.Add(edge.End);
                            for (int i = 0; i < pnts.Count; i++)
                            {
                                var pp0 = pnts[i];
                                var pp1 = pnts[(i + 1) % pnts.Count];
                                var p0 = pl.GetUVProjPoint(pp0, v1, axis2);
                                var p1 = pl.GetUVProjPoint(pp1, v1, axis2);
                                ll1.Add(new Segment() { Start = p0, End = p1 });
                            }
                        }
                        else
                        {
                            throw new LiteCadException("unsupported curve type");
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
                                        throw new LiteCadException("");
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
                                throw new LiteCadException("bad contour");
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

                var triangls = GeometryUtils.TriangulateWithHoles(new[] { cntrs[0].Elements.Select(z => z.Start).ToArray() },
                    cntrs.Skip(1).Select(z => z.Elements.Select(u => u.Start).ToArray()).ToArray(), true);
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
            }
            else if (Surface is BRepCylinder cl)
            {
                List<Contour> cntrs = new List<Contour>();
                var len = Math.PI * 2 * cl.Radius;

                Vector3d vec0 = Vector3d.Zero;
                //special case 1
                if (Wires.Count == 1 && Wires[0].Edges.Count(z => z.Curve is BRepSeamCurve) == 2)
                {
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
                    for (int i = 0; i < steps; i++)
                    {
                        var x = step1 * i;
                        xx.Add(x);
                    }
                    for (int i = 1; i < xx.Count; i++)
                    {
                        var x0 = xx[i - 1];
                        var x1 = xx[i];
                        cc1.Elements.Add(new Segment() { Start = new Vector2d(x0 / len, 0), End = new Vector2d(x1 / len, 0) });
                    }
                    for (int i = xx.Count - 1; i >= 1; i--)
                    {
                        var x0 = xx[i];
                        var x1 = xx[i - 1];

                        cc1.Elements.Add(new Segment() { Start = new Vector2d(x0 / len, len2), End = new Vector2d(x1 / len, len2) });
                    }
                    cntrs.Add(cc1);


                }
                else
                {
                    foreach (var wire in Wires)
                    {
                        List<Contour> l1 = new List<Contour>();

                        foreach (var edge in wire.Edges)
                        {
                            List<Segment> ll1 = new List<Segment>();
                            if (edge.Curve is BRepCircleCurve cc)
                            {

                            }
                            else if (edge.Curve is BRepLineCurve lnc)
                            {

                            }
                        }
                    }
                }
                var triangls = GeometryUtils.TriangulateWithHoles(new[] { cntrs[0].Elements.Select(z => z.Start).ToArray() },
               cntrs.Skip(1).Select(z => z.Elements.Select(u => u.Start).ToArray()).ToArray(), true);


                //transform back 2d->3d
                MeshNode node = new MeshNode();
                List<TriangleInfo> tt = new List<TriangleInfo>();
                foreach (var item in triangls)
                {
                    TriangleInfo tin = new TriangleInfo();
                    List<VertexInfo> v = new List<VertexInfo>();
                    foreach (var d in item)
                    {
                        var ang = d.X * Math.PI * 2;
                        var mtr = Matrix4d.CreateFromAxisAngle(cl.Axis, ang);

                        var rot0 = Vector3d.Transform(vec0 + cl.Axis * d.Y, mtr);
                        v.Add(new VertexInfo() { Position = cl.Location + rot0 });
                    }
                    tin.Vertices = v.ToArray();
                    tt.Add(tin);
                }
                node.Triangles = tt;
                ret = node;
                return ret;
            }
            return ret;
        }
    }
}