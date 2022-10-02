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
    public class BRepConicalSurfaceFace : BRepFace
    {
        public BRepConicalSurfaceFace(Part parent) : base(parent) { }

        public override MeshNode ExtractMesh()
        {
            MeshNode ret = null;
            Vector3d proj1;
            Vector3d v1;

            List<Line3D[]> ll = new List<Line3D[]>();

            var cl = Cone;

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

            }

            return ret;
        }

        public BRepConicalSurface Cone
            {
            get
                {
                return Surface as BRepConicalSurface;
            }
        }

        public override Line3D[] Get3DSegments(BRepEdge edge, double eps = 1e-8)
        {
            var cl = Cone;
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
                    throw new BrepException("wrong end point");
                }

                if ((pnts.First() - edge.Start).Length > eps)
                {
                    throw new BrepException("wrong start point");
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

        public static BRepEdge ExtractCircleEdge(BRepFace face, Vector3d start, Vector3d end1, double radius,
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

            var dir1 = start - pos;
            var dir2 = end1 - pos;
            List<Vector3d> pnts = new List<Vector3d>();

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
                face.Items.Add(new LineItem() { Start = p0, End = p1 });
            }
            return edge;
        }

        public override void Load(AdvancedFace face)
        {
            var ss = (face.Surface as ConicalSurface);

            Surface = new BRepConicalSurface()
            {
                Location = face.Surface.Axis.Location,
                Normal = face.Surface.Axis.Dir1,
                Radius = ss.Radius,
                SemiAngle = ss.SemiAngle
            };

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
                        if (sc.Geometry is Circle crc2)
                        {
                            wire.Edges.Add(ExtractCircleEdge(this, start, end, crc2.Radius, crc2.Axis.Dir1, crc2.Axis.Location));

                        }
                        else if (sc.Geometry is Line line)
                        {
                            Items.Add(new LineItem() { Start = start, End = end });
                        }
                        else
                        {
                            throw new StepParserException($"unknown geometry: {sc.Geometry}");
                        }
                    }
                    else if (crv is Circle crc)
                    {
                        wire.Edges.Add(ExtractCircleEdge(this, start, end, crc.Radius, crc.Axis.Dir1, crc.Axis.Location));
                    }
                    else if (crv is SeamCurve seam)
                    {
                        Items.Add(new LineItem() { Start = start, End = end });
                    }
                    else
                    {
                        throw new StepParserException($"unknown curve: {crv}");
                    }
                }
            }
        }

        public override BRepFace Clone()
        {
            throw new NotImplementedException();
        }
    }
}