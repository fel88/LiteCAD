﻿using IxMilia.Step.Items;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.BRep
{
    public class BRepPlaneFace : BRepFace
    {
        public BRepPlaneFace(Part p) : base(p) { }
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

        private BRepEdge extractCircleEdge(Vector3d start, Vector3d end1, StepCircle circ)
        {
            BRepEdge edge = new BRepEdge();

            var rad = circ.Radius;
            var cc = new BRepCircleCurve();
            edge.Curve = cc;
            cc.Radius = rad;

            edge.Start = start;
            edge.End = end1;


            var axis3d = circ.Position as StepAxis2Placement3D;
            var axis = new Vector3d(axis3d.Axis.X, axis3d.Axis.Y, axis3d.Axis.Z);
            var refdir = new Vector3d(axis3d.RefDirection.X, axis3d.RefDirection.Y, axis3d.RefDirection.Z);
            var pos = new Vector3d(circ.Position.Location.X,
                circ.Position.Location.Y,
                circ.Position.Location.Z);
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

        public override void Load(StepAdvancedFace face, StepSurface _pl)
        {
            var pl = _pl as StepPlane;
            var loc = pl.Position.Location;
            var loc2 = new Vector3d(loc.X, loc.Y, loc.Z);
            var nrm = pl.Position.Axis;
            var nrm2 = new Vector3d(nrm.X, nrm.Y, nrm.Z);
            Surface = new BRepPlane()
            {
                Location = loc2,
                Normal = nrm2
            };
            foreach (var bitem in face.Bounds)
            {
                var loop = bitem.Bound as StepEdgeLoop;
                BRepWire wire = new BRepWire() { IsOutter = bitem is StepFaceOuterBound };
                Wires.Add(wire);
                foreach (var litem in loop.EdgeList)
                {
                    StepEdgeCurve crv = litem.EdgeElement as StepEdgeCurve;
                    var strt = (crv.EdgeStart as StepVertexPoint).Location;
                    var end = (crv.EdgeEnd as StepVertexPoint).Location;
                    var start = new Vector3d(strt.X, strt.Y, strt.Z);
                    var end1 = new Vector3d(end.X, end.Y, end.Z);
                    if (crv.EdgeGeometry is StepCircle circ)
                    {
                        var edge = extractCircleEdge(start, end1, circ);
                        wire.Edges.Add(edge);

                        //var rad = circ.Radius;
                        //var cc = new BRepCircleCurve();
                        //edge.Curve = cc;
                        //cc.Radius = rad;

                        //edge.Start = start;
                        //edge.End = end1;


                        //var axis3d = circ.Position as StepAxis2Placement3D;
                        //var axis = new Vector3d(axis3d.Axis.X, axis3d.Axis.Y, axis3d.Axis.Z);
                        //var refdir = new Vector3d(axis3d.RefDirection.X, axis3d.RefDirection.Y, axis3d.RefDirection.Z);
                        //var pos = new Vector3d(circ.Position.Location.X,
                        //    circ.Position.Location.Y,
                        //    circ.Position.Location.Z);
                        //var dir1 = start - pos;
                        //var dir2 = end1 - pos;
                        //List<Vector3d> pnts = new List<Vector3d>();



                        //var dot = Vector3d.Dot(dir2, dir1);
                        //var crs = Vector3d.Cross(dir2, dir1);
                        //var ang2 = Vector3d.CalculateAngle(dir1, dir2);

                        //if (!(Vector3d.Dot(axis, crs) < 0))
                        //{
                        //    ang2 = (2 * Math.PI) - ang2;
                        //}

                        //pnts.Add(pos + dir1);
                        //cc.Axis = axis;
                        //cc.Dir = dir1;
                        //cc.Location = pos;
                        //cc.SweepAngle = ang2;
                        //if ((start - end1).Length < 1e-8)
                        //{
                        //    cc.SweepAngle = Math.PI * 2;
                        //}
                        //var step = Math.PI * 15 / 180f;
                        //for (double i = 0; i < ang2; i += step)
                        //{
                        //    var mtr4 = Matrix4d.CreateFromAxisAngle(axis, i);
                        //    var res = Vector4d.Transform(new Vector4d(dir1), mtr4);
                        //    //var rot = new Vector4d(dir1) * mtr4;
                        //    pnts.Add(pos + res.Xyz);
                        //}
                        //pnts.Add(pos + dir2);
                        //for (int j = 1; j < pnts.Count; j++)
                        //{
                        //    var p0 = pnts[j - 1];
                        //    var p1 = pnts[j];
                        //    Items.Add(new LineItem() { Start = p0, End = p1 });
                        //}

                    }
                    else if (crv.EdgeGeometry is StepLine lin)
                    {
                        BRepEdge edge = new BRepEdge();
                        edge.Curve = new BRepLineCurve() { };
                        wire.Edges.Add(edge);

                        var vec = new Vector3d(lin.Vector.Direction.X,
                            lin.Vector.Direction.Y,
                            lin.Vector.Direction.Z);
                        edge.Start = start;
                        edge.End = end1;
                        Items.Add(new LineItem()
                        {
                            Start = start,
                            End = end1
                        });
                    }
                    else if (crv.EdgeGeometry is StepCurveSurface csurf)
                    {
                        if (csurf.EdgeGeometry is StepLine ln)
                        {
                            BRepEdge edge = new BRepEdge();
                            edge.Curve = new BRepLineCurve() { };
                            wire.Edges.Add(edge);

                            edge.Start = start;
                            edge.End = end1;

                            Items.Add(new LineItem()
                            {
                                Start = start,
                                End = end1
                            });
                        }
                        else if (csurf.EdgeGeometry is StepCircle circ2)
                        {
                            var edge = extractCircleEdge(start, end1, circ2);
                            wire.Edges.Add(edge);

                            //var rad = circ2.Radius;
                            //var cc = new BRepCircleCurve();
                            //edge.Curve = cc;
                            //cc.Radius = rad;

                            //edge.Start = start;
                            //edge.End = end1;


                            //var axis3d = circ2.Position as StepAxis2Placement3D;
                            //var axis = new Vector3d(axis3d.Axis.X, axis3d.Axis.Y, axis3d.Axis.Z);
                            //var refdir = new Vector3d(axis3d.RefDirection.X, axis3d.RefDirection.Y, axis3d.RefDirection.Z);
                            //var pos = new Vector3d(circ2.Position.Location.X,
                            //    circ2.Position.Location.Y,
                            //    circ2.Position.Location.Z);

                            //var dir1 = start - pos;
                            //var dir2 = end1 - pos;
                            //List<Vector3d> pnts = new List<Vector3d>();

                            //var crs = Vector3d.Cross(dir2, dir1);
                            //var dot = Vector3d.Dot(dir2, dir1);

                            //var ang2 = Vector3d.CalculateAngle(dir1, dir2);// (Math.Acos(dot / dir2.Length / dir1.Length));
                            //if (!(Vector3d.Dot(axis, crs) < 0))
                            //{
                            //    ang2 = (2 * Math.PI) - ang2;
                            //}
                            //pnts.Add(pos + dir1);
                            //cc.Axis = axis;
                            //cc.Dir = dir1;
                            //cc.Location = pos;
                            //cc.SweepAngle = ang2;
                            //if ((start - end1).Length < 1e-8)
                            //{
                            //    cc.SweepAngle = Math.PI * 2;
                            //}
                            //var step = Math.PI * 15 / 180f;
                            //for (double i = 0; i < ang2; i += step)
                            //{
                            //    var mtr4 = Matrix4d.CreateFromAxisAngle(axis, i);
                            //    var res = Vector4d.Transform(new Vector4d(dir1), mtr4);
                            //    pnts.Add(pos + res.Xyz);
                            //}
                            //pnts.Add(pos + dir2);
                            //for (int j = 1; j < pnts.Count; j++)
                            //{
                            //    var p0 = pnts[j - 1];
                            //    var p1 = pnts[j];
                            //    Items.Add(new LineItem() { Start = p0, End = p1 });
                            //}
                        }
                        else if (csurf.EdgeGeometry is StepBSplineCurveWithKnots bspline)
                        {
                            BRepEdge edge = new BRepEdge();
                            var cc = new BRepBSplineWithKnotsCurve();
                            edge.Start = start;
                            edge.End = end1;
                            cc.Degree = bspline.Degree;
                            cc.Closed = bspline.ClosedCurve;
                            cc.ControlPoints = bspline.ControlPointsList.Select(z => new Vector3d(z.X, z.Y, z.Z)).ToArray();
                            cc.KnotMultiplicities = bspline.KnotMultiplicities.ToArray();
                            cc.Knots = bspline.Knots.ToArray();
                            edge.Curve = cc;
                            wire.Edges.Add(edge);
                            Items.Add(new LineItem() { Start = start, End = end1 });
                        }
                        else
                        {
                            DebugHelpers.Warning($"unsupported geometry: {csurf.EdgeGeometry}");
                        }
                    }
                    else
                    {
                        DebugHelpers.Warning($"plane surface. unsupported: {crv}");
                    }
                }
            }
        }
    }
}