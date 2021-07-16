using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.BRep
{
    public class BRepFace
    {
        public BRepFace(Part parent)
        {
            Parent = parent;
            Id = NewId++;
        }
        public MeshNode Node;
        public Part Parent;
        bool _selected;
        public bool Selected
        {
            get => _selected; set
            {
                _selected = value;
                foreach (var item in Items)
                {
                    item.Selected = value;
                }
            }
        }
        public static int NewId = 0;
        public int Id { get; set; }
        public List<DrawItem> Items = new List<DrawItem>();
        public BRepSurface Surface;
        public List<BRepWire> Wires = new List<BRepWire>();
        public BRepWire OutterWire;
        public bool Visible { get; set; } = true;
        public virtual MeshNode ExtractMesh()
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
                        for (int i = 1; i < pnts.Count; i++)
                        {
                            var pp0 = pnts[i - 1];
                            var pp1 = pnts[i];
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
    }
}