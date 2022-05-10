using LiteCAD.BRep.Curves;
using LiteCAD.BRep.Surfaces;
using LiteCAD.Common;
using LiteCAD.Parsers.Step;
using OpenTK;
using System;
using System.Collections.Generic;

namespace LiteCAD.BRep.Faces
{
    public class BRepConicalSurfaceFace : BRepFace
    {
        public BRepConicalSurfaceFace(Part parent) : base(parent) { }

        public override MeshNode ExtractMesh()
        {
            MeshNode ret = new MeshNode();
            foreach (var item in Wires)
            {
                foreach (var edge in item.Edges)
                {

                }
            }
            return ret;
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
            Surface = new BRepConicalSurface() { Location = face.Surface.Axis.Location, Normal = face.Surface.Axis.Dir1 };
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
    }
}