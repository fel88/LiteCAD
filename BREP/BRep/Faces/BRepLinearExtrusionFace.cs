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
    public class BRepLinearExtrusionFace : BRepPlaneFace
    {
        public BRepLinearExtrusionFace(Part p) : base(p) { }
        public override MeshNode ExtractMesh()
        {
            var ss = Surface as BRepLinearExtrusionSurface;

            //get all on the main plane


            Vector3d? extrusionDir = null;
            BRepPlane plane = new BRepPlane() { Location = ss.Location, Normal = ss.Normal };


            MeshNode ret = new MeshNode();
            List<BRepEdge> edges = new List<BRepEdge>();
            foreach (var wire in Wires)
            {
                foreach (var edge in wire.Edges)
                {
                    if (plane.IsOnSurface(edge.Start) && plane.IsOnSurface(edge.End))
                    {
                        edges.Add(edge);
                        continue;
                    }
                    else
                    {
                        double eps = 1e-8;
                        var dir = (edge.End - edge.Start).Normalized();
                        var ang = Vector3d.CalculateAngle(dir, ss.Normal);
                        if (edge.Curve is BRepLineCurve lc)
                        {

                            if (ang < eps || Math.Abs(ang - Math.PI) < eps)
                            {

                                if (plane.IsOnSurface(edge.End))
                                {
                                    extrusionDir = edge.Start - edge.End;
                                }
                                else
                                {
                                    extrusionDir = edge.End - edge.Start;
                                }
                            }
                        }
                        else if (edge.Curve is BRepSeamCurve seam)
                        {
                            if (ang < eps || Math.Abs(ang - Math.PI) < eps)
                            {

                                if (plane.IsOnSurface(edge.End))
                                {
                                    extrusionDir = edge.Start - edge.End;
                                }
                                else
                                {
                                    extrusionDir = edge.End - edge.Start;
                                }
                            }
                        }
                    }
                }
            }
            if (extrusionDir == null) throw new BrepException("linear extrusion: extract mesh -  extrusion dir wrong detection");
            Vector3d v1 = Vector3d.Zero;
            foreach (var item in edges)
            {
                if (item.Curve is BRepEllipseCurve el)
                {
                    v1 = item.Start - el.Location;
                }
                if (item.Curve is BRepCircleCurve cc)
                {
                    v1 = item.Start - cc.Location;
                }
                if (item.Curve is BRepLineCurve ln)
                {
                    v1 = item.Start - item.End;
                }
            }
            v1.Normalize();
            var axis2 = Vector3d.Cross(plane.Normal, v1).Normalized();

            List<Segment> ll1 = new List<Segment>();
            foreach (var item in edges)
            {
                if (item.Curve is BRepEllipseCurve elc)
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
                    pnts.Add(item.End);
                    for (int i = 1; i < pnts.Count; i++)
                    {
                        var pp0 = pnts[i - 1];
                        var pp1 = pnts[i];
                        var p0 = plane.GetUVProjPoint(pp0, v1, axis2);
                        var p1 = plane.GetUVProjPoint(pp1, v1, axis2);
                        ll1.Add(new Segment() { Start = p0, End = p1 });
                    }
                }
            }
            List<Contour> l1 = new List<Contour>();
            if (ll1.Any())
            {
                var zz = new Contour() { Wire = null };
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
            List<Contour[]> ll = new List<Contour[]>();
            ll.Add(l1.ToArray());
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
            int mult = 10;
            DebugHelpers.ToBitmap(cntrs.ToArray(), mult);
            ret.Parent = this;
            var shift = extrusionDir.Value;
            for (int i = 0; i < cntrs[0].Elements.Count; i++)
            {
                var _p0 = cntrs[0].Elements[i].Start;
                var _p1 = cntrs[0].Elements[i].End;
                var p0 = _p0.X * v1 + _p0.Y * axis2 + plane.Location;
                var p1 = _p1.X * v1 + _p1.Y * axis2 + plane.Location;
                var p2 = p0 + shift;
                var p3 = p1 + shift;
                var tr1 = new TriangleInfo() { };
                var tr2 = new TriangleInfo() { };

                tr1.Vertices = new VertexInfo[] {
                    new VertexInfo() { Position = p0 } ,new VertexInfo() { Position = p1 },new VertexInfo() { Position = p2 }
                };
                var nrm1 = tr1.Normal();
                for (int j = 0; j < tr1.Vertices.Length; j++)
                {
                    tr1.Vertices[j].Normal = nrm1;
                }
                tr2.Vertices = new VertexInfo[] {
                    new VertexInfo() { Position = p2 } ,new VertexInfo() { Position = p1 },new VertexInfo() { Position = p3 }
                };
                var nrm2 = tr2.Normal();
                for (int j = 0; j < tr2.Vertices.Length; j++)
                {
                    tr2.Vertices[j].Normal = nrm2;
                }
                ret.Triangles.Add(tr1);
                ret.Triangles.Add(tr2);
            }
            Node = ret;
            return ret;
        }
                

        public override void Load(AdvancedFace face)
        {
            var surf = face.Surface as LinearExtrusionSurface;
            Vector3d loc = Vector3d.Zero;
            Vector3d nrm = Vector3d.Zero;
            if (surf.Curve is IAxis ax)
            {
                loc = ax.Axis.Location;
                nrm = ax.Axis.Dir1;
            }
            Surface = new BRepLinearExtrusionSurface()
            {
                Length = surf.Vector.Length,
                Vector = surf.Vector.Location,
                Location = loc,
                Normal = nrm
            };

            foreach (var bitem in face.Bounds)
            {

                BRepWire wire = new BRepWire();
                Wires.Add(wire);
                foreach (var litem in bitem.Loop.Edges)
                {
                    var start = litem.Curve.Start.Point;
                    var end = litem.Curve.End.Point;
                    var crv = litem.Curve.EdgeGeometry;

                    if (crv is SeamCurve seam)
                    {
                        BRepEdge edge = new BRepEdge();
                        var cc = new BRepSeamCurve();
                        edge.Start = start;
                        edge.End = end;
                        edge.Curve = cc;
                        wire.Edges.Add(edge);
                        Outlines.Add(new LineItem()
                        {
                            Start = start,
                            End = end
                        });
                    }
                    /*  else
                     if (crv is Line sc)
                      {

                      }*/
                    else if (crv is SurfaceCurve curve)
                    {
                        /*  if (curve.Geometry is Circle circ2)
                          {

                          }

                          else */
                        if (curve.Geometry is Line _line)
                        {
                            BRepEdge edge = new BRepEdge();
                            var cc = new BRepLineCurve() { Vector = end - start, Point = start };
                            edge.Start = start;
                            edge.End = end;
                            edge.Curve = cc;
                            wire.Edges.Add(edge);
                            Outlines.Add(new LineItem()
                            {
                                Start = start,
                                End = end
                            });
                        }
                        else if (curve.Geometry is Ellipse elp2)
                        {
                            var edge = ExtractEllipseEdge(start, end, elp2.MinorRadius, elp2.MajorRadius, elp2.Axis.Dir1,
                                elp2.Axis.Dir2, elp2.Axis.Location);
                            wire.Edges.Add(edge);
                        }
                        else
                        {
                            DebugHelpers.Warning($"unsupported curve geometry: {curve.Geometry}");
                        }
                    }
                    else
                    {
                        DebugHelpers.Warning($"linear extrusion surface. unsupported curve: {crv}");
                    }
                }
            }
        }
    }
}