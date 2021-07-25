﻿using IxMilia.Step.Items;
using LiteCAD.Common;
using OpenTK;
using System.Linq;

namespace LiteCAD.BRep
{
    public class BRepLinearExtrusionFace : BRepFace
    {
        public BRepLinearExtrusionFace(Part p) : base(p) { }
        public override MeshNode ExtractMesh()
        {
            MeshNode ret = new MeshNode();
            return ret;
        }

        public override void Load(StepAdvancedFace face, StepSurface _cyl)
        {
            var ext = _cyl as StepSurfaceOfLinearExtrusion;
            Surface = new BRepLinearExtrusionSurface()
            {
                Length = ext.Vector.Length,
                Vector = new Vector3d(ext.Vector.Direction.X, ext.Vector.Direction.Y, ext.Vector.Direction.Z)
            };
            
            foreach (var bitem in face.Bounds)
            {
                var loop = bitem.Bound as StepEdgeLoop;
                BRepWire wire = new BRepWire();
                Wires.Add(wire);
                foreach (var litem in loop.EdgeList)
                {
                    StepEdgeCurve crv = litem.EdgeElement as StepEdgeCurve;
                    var strt = (crv.EdgeStart as StepVertexPoint).Location;
                    var end = (crv.EdgeEnd as StepVertexPoint).Location;
                    var start = new Vector3d(strt.X, strt.Y, strt.Z);
                    var end1 = new Vector3d(end.X, end.Y, end.Z);
                    Items.Add(new LineItem()
                    {
                        Start = start,
                        End = end1
                    });
                    if (crv.EdgeGeometry is StepCircle circ)
                    {

                    }
                    else if (crv.EdgeGeometry is StepCurveSurface curve)
                    {
                        if (curve.EdgeGeometry is StepCircle circ2)
                        {

                        }
                        else if (curve.EdgeGeometry is StepBSplineCurveWithKnots bspline)
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
                        }
                        else if (curve.EdgeGeometry is StepLine _line)
                        {
                            BRepEdge edge = new BRepEdge();
                            var cc = new BRepLineCurve();
                            edge.Start = start;
                            edge.End = end1;
                            edge.Curve = cc;
                            wire.Edges.Add(edge);
                        }
                        else
                        {
                            DebugHelpers.Warning($"unsupported curve geometry: {curve.EdgeGeometry}");
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