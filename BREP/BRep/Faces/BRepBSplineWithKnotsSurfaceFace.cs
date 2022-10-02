using BREP.BRep.Curves;
using BREP.BRep.Outlines;
using BREP.BRep.Surfaces;
using BREP.Parsers.Step;
using LiteCAD.Common;
using System;
using System.Linq;

namespace BREP.BRep.Faces
{
    public class BRepBSplineWithKnotsSurfaceFace : BRepFace
    {
        public BRepBSplineWithKnotsSurfaceFace(Part parent) : base(parent) { }

        public override BRepFace Clone()
        {
            throw new NotImplementedException();
        }

        public override MeshNode ExtractMesh()
        {
            var surf = Surface as BRepBSplineWithKnotsSurface;

            foreach (var item in Wires)
            {
                foreach (var edge in item.Edges)
                {

                }
            }

            throw new NotImplementedException();
        }

        
        public override void Load(AdvancedFace face)
        {
            var ss = face.Surface as BSplineSurfaceWithKnots;
            Surface = new BRepBSplineWithKnotsSurface()
            {
                //Location = face.Surface.Axis.Location,
                //Normal = face.Surface.Axis.Dir1
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


                        }
                        else
                        {
                            throw new StepParserException($"unknown geometry: {sc.Geometry}");
                        }
                    }
                    else if (crv is Circle crc)
                    {

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
                        Items.Add(new LineItem() { Start = start, End = end });
                    }
                    else
                    if (crv is BSplineCurveWithKnots bcrv)
                    {
                        BRepEdge edge = new BRepEdge();
                        edge.Start = start;
                        edge.End = end;
                        BRepSpline spl = new BRepSpline();

                        edge.Curve = spl;

                        spl.Multiplicities = bcrv.Multiplicities.ToList();
                        spl.Poles = bcrv.ControlPoints.ToList();
                        spl.Knots = bcrv.Knots.ToList();
                        spl.Weights = bcrv.Weights.ToList();
                        edge.Param1 = bcrv.Param1;
                        edge.Param2 = bcrv.Param2;
                        spl.IsBSpline = true;
                        spl.Degree = bcrv.Degree[0];

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
                    else
                    {
                        throw new StepParserException($"unknown curve: {crv}");
                    }
                }
            }
        }
    }
}