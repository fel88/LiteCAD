using LiteCAD.Common;
using OpenTK;
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
                var fr = Wires.First(z => z.Edges.Any(u => u.Curve is BRepLineCurve));
                var efr = fr.Edges.First(z => z.Curve is BRepLineCurve);
                var dir = efr.End - efr.Start;
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
            return ret;
        }
    }
}