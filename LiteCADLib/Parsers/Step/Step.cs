using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiteCADLib.Parsers.Step
{
    public static class StepParser
    {
        public static string[] Tokenize(string data)
        {
            List<string> ret = new List<string>();
            bool insideString = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (!insideString && (data[i] == '\n' || data[i] == '\r'))
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }
                if (data[i] == '\'')
                {
                    insideString = !insideString;
                    if (!insideString)
                    {
                        sb.Append('\'');
                        ret.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }
                if (data[i] == '=' && !insideString)
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    ret.Add("=");
                    continue;

                }
                if (data[i] == ';' && !insideString)
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    ret.Add(";");
                    continue;
                }
                sb.Append(data[i]);
            }
            return ret.ToArray();
        }

        //static BRepFace toPlaneFace(Part p, AdvancedFace face)
        //{
        //    BRepPlaneFace ret = new BRepPlaneFace(p);
        //    ret.Surface = new BRepPlane() { Location = face.Surface.Axis.Location, Normal = face.Surface.Axis.Dir1 };
        //    foreach (var item in face.Bounds)
        //    {
        //        BRepWire wire = new BRepWire();
        //        ret.Wires.Add(wire);
        //        foreach (var litem in item.Loop.Edges)
        //        {
        //            var crv = litem.Curve.EdgeGeometry;
        //            if (crv is SurfaceCurve sc)
        //            {
        //                if (sc.Geometry is Line ln2)
        //                {

        //                    BRepEdge edge = new BRepEdge();
        //                    edge.Curve = new BRepLineCurve() { Point = ln2.Point, Vector = ln2.Vector.Location };
        //                    wire.Edges.Add(edge);
        //                    edge.Start = litem.Curve.Start.Point;
        //                    edge.End = litem.Curve.End.Point;
        //                    ret.Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
        //                }
        //                else
        //                if (sc.Geometry is Circle circ2)
        //                {
        //                    var start = litem.Curve.Start.Point;
        //                    var end = litem.Curve.End.Point;
        //                    wire.Edges.Add(BRepPlaneFace.ExtractCircleEdge(ret, start, end, circ2.Radius, circ2.Axis.Dir1, circ2.Axis.Location));
        //                }
        //                else
        //                if (sc.Geometry is Ellipse elp2)
        //                {
        //                    var start = litem.Curve.Start.Point;
        //                    var end = litem.Curve.End.Point;
        //                    wire.Edges.Add(BRepPlaneFace.ExtractEllipseEdge(ret, 
        //                        start, 
        //                        end,
        //                        elp2.MinorRadius, 
        //                        elp2.MajorRadius, 
        //                        elp2.Axis.Dir1,
        //                        elp2.Axis.Location));
        //                }
        //                else
        //                {
        //                    throw new StepParserException($"unknown geometry: {sc.Geometry}");
        //                }
        //            }
        //            else
        //            if (crv is Line ln)
        //            {
        //                BRepEdge edge = new BRepEdge();
        //                edge.Curve = new BRepLineCurve() { Point = ln.Point, Vector = ln.Vector.Location };
        //                wire.Edges.Add(edge);
        //                edge.Start = litem.Curve.Start.Point;
        //                edge.End = litem.Curve.End.Point;
        //                ret.Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
        //            }
        //            else
        //            if (crv is Circle circ)
        //            {
        //                BRepEdge edge = new BRepEdge();
        //                edge.Curve = new BRepCircleCurve() { Radius = circ.Radius, Location = circ.Axis.Location };
        //                wire.Edges.Add(edge);
        //                edge.Start = litem.Curve.Start.Point;
        //                edge.End = litem.Curve.End.Point;
        //                //ret.Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
        //            }
        //            else
        //            {
        //                throw new StepParserException($"unknown curve: {crv}");
        //            }
        //        }
        //    }
        //    return ret;
        //}

        //static BRepFace toCylinderFace(Part p, AdvancedFace face)
        //{
        //    var ss = (face.Surface as CylindricalSurface);
        //    BRepCylinderSurfaceFace ret = new BRepCylinderSurfaceFace(p);
        //    ret.Surface = new BRepCylinder()
        //    {
        //        Location = face.Surface.Axis.Location,
        //        Radius = ss.Radius,
        //        Axis = ss.Axis.Dir1,
        //        RefDir = ss.Axis.Dir2
        //    };
        //    foreach (var item in face.Bounds)
        //    {
        //        BRepWire wire = new BRepWire();
        //        ret.Wires.Add(wire);
        //        foreach (var litem in item.Loop.Edges)
        //        {
        //            var crv = litem.Curve.EdgeGeometry;
        //            var start = litem.Curve.Start.Point;
        //            var end = litem.Curve.End.Point;
        //            if (crv is SurfaceCurve sc)
        //            {
        //                if (sc.Geometry is Line ln2)
        //                {
        //                    BRepEdge edge = new BRepEdge();
        //                    edge.Curve = new BRepLineCurve() { Point = ln2.Point, Vector = ln2.Vector.Location };
        //                    wire.Edges.Add(edge);
        //                    edge.Start = litem.Curve.Start.Point;
        //                    edge.End = litem.Curve.End.Point;
        //                    ret.Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
        //                }
        //                else
        //                if (sc.Geometry is Circle circ2)
        //                {
        //                    wire.Edges.Add(BRepCylinderSurfaceFace.ExtractCircleEdge(start, end, circ2.Axis.Location,
        //                     circ2.Axis.Dir1, circ2.Radius));
        //                }
        //            }
        //            else
        //            if (crv is Line ln)
        //            {
        //                BRepEdge edge = new BRepEdge();
        //                edge.Curve = new BRepLineCurve() { Point = ln.Point, Vector = ln.Vector.Location };
        //                wire.Edges.Add(edge);
        //                edge.Start = litem.Curve.Start.Point;
        //                edge.End = litem.Curve.End.Point;
        //                ret.Items.Add(new LineItem() { Start = edge.Start, End = edge.End });
        //            }
        //        }
        //    }
        //    return ret;
        //}

        

    

        public static Part Parse(string filename)
        {
            var txt = File.ReadAllText(filename);
            var tkns = Tokenize(txt);

            StepParseContext ctx = new StepParseContext();

            StringBuilder accum = new StringBuilder();
            foreach (var token in tkns)
            {
                if (token == ";")
                {
                    var str = accum.ToString();
                    if (str.StartsWith("#"))
                    {
                        StepLineItem item = new StepLineItem();
                        var spl = str.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        item.Index = int.Parse(spl[0].Trim().TrimStart('#'));
                        item.Value = spl[1];
                        ctx.AddItem(item);
                    }
                    accum.Clear();
                    continue;
                }
                accum.Append(token);
            }

            ctx.ParseShells();
            var part = ctx.ToPart();
            part.Name = new FileInfo(filename).Name;
            if (Part.AutoExtractMeshOnLoad)
                part.ExtractMesh();
            part.FixNormals();
            DebugHelpers.Progress(false, 0);
            return part;
        }
    }

    public class VertexPoint
    {
        public Vector3d Point;
    }
    public class EdgeLoop
    {
        public List<OrientedEdge> Edges = new List<OrientedEdge>();
    }

    public class FaceBound
    {
        public EdgeLoop Loop;
    }
    public class FaceOuterBound : FaceBound
    {

    }
}
