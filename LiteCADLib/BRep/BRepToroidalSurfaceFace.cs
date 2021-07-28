using IxMilia.Step.Items;
using LiteCAD.Common;
using LiteCADLib.Parsers.Step;
using OpenTK;
using System;
using System.Linq;

namespace LiteCAD.BRep
{
    public class BRepToroidalSurfaceFace : BRepFace
    {
        public BRepToroidalSurfaceFace(Part parent) : base(parent) { }

        public override MeshNode ExtractMesh()
        {
            throw new NotImplementedException();
        }

        public override void Load(StepAdvancedFace face, StepSurface _cyl)
        {
            var tor = _cyl as StepToroidalSurface;
            var loc = tor.Position.Location;
            var loc2 = new Vector3d(loc.X, loc.Y, loc.Z);
            var nrm = tor.Position.Axis;
            var nrm2 = new Vector3d(nrm.X, nrm.Y, nrm.Z);
            Surface = new BRepToroidalSurface()
            {
                Location = loc2,
                Normal = nrm2,
                MinorRadius = tor.MinorRadius,
                MajorRadius = tor.MajorRadius
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


                    if (crv.EdgeGeometry is StepCircle circ)
                    {

                    }
                    else if (crv.EdgeGeometry is StepCurveSurface curve)
                    {
                        if (curve.EdgeGeometry is StepCircle _circle)
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
                        else
                        {
                            DebugHelpers.Warning($"unsupported curve geometry: {curve.EdgeGeometry}");

                        }
                    }
                    else
                    {
                        DebugHelpers.Warning($"toroidal surface. unsupported curve: {crv}");
                    }
                }
            }
        }

        public override void Load(AdvancedFace face)
        {
            throw new NotImplementedException();
        }
    }
}