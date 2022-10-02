using BREP.BRep;
using BREP.BRep.Curves;
using BREP.BRep.Faces;
using BREP.BRep.Surfaces;
using BREP.Common;
using IxMilia.Iges;
using IxMilia.Iges.Entities;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LiteCAD.Parsers.Iges
{
    public static class IgesParser
    {
        public static Part Parse(string filename)
        {
            Part ret = new Part();
            IgesFile igesFile = IgesFile.Load(filename);
            ret.Name = new FileInfo(filename).Name;
            //List<IIgesSurface> surfaces = new List<IIgesSurface>();
            foreach (IgesEntity entity in igesFile.Entities)
            {
               /* if (entity is IIgesSurface ss)
                {
                    surfaces.Add(ss);
                }*/
                switch (entity.EntityType)
                {
                    case IgesEntityType.RationalBSplineCurve:
                        {
                            IgesRationalBSplineCurve bs = entity as IgesRationalBSplineCurve;
                            var fr = bs.FormNumber;
                            if (fr == 1)//curve
                            {

                            }

                        }
                        break;
                    case IgesEntityType.TrimmedParametricSurface:
                        {

                            IgesTrimmedParametricSurface tps = entity as IgesTrimmedParametricSurface;
                            var mtr = tps.TransformationMatrix;
                            if (!mtr.IsIdentity)
                            {

                            }
                            var outter = tps.OuterBoundary;
                            var surf = tps.Surface;
                            BRepFace face = null;
                            if (surf is IgesPlane pl)
                            {
                                var normal = new OpenTK.Vector3d(pl.PlaneCoefficientA, pl.PlaneCoefficientB, pl.PlaneCoefficientC).Normalized();
                                var pos = normal * pl.PlaneCoefficientD;
                                face = new BRepPlaneFace()
                                {
                                    Surface = new BRepPlane()
                                    {
                                        Normal = normal,
                                        Location = pos
                                    }
                                };
                            }
                            if (surf is IgesRationalBSplineSurface rbs)
                            {
                                if (outter is IgesCurveOnAParametricSurface p)
                                {
                                    var owire = GetWire(p);
                                    var v0 = owire.Edges[0].Start;
                                    var v1 = owire.Edges[0].End;
                                    var v2 = owire.Edges[1].Start;
                                    var v3 = owire.Edges[1].End;

                                    var normal = Vector3d.Cross(v1 - v0, v3 - v2).Normalized();
                                    var pos = owire.Edges[0].Start;
                                    if (!double.IsNaN(normal.X))
                                        face = new BRepPlaneFace()
                                        {
                                            Surface = new BRepPlane()
                                            {
                                                Normal = normal,
                                                Location = pos
                                            }
                                        };
                                }
                            }

                            if (face != null)
                            {
                                ret.Faces.Add(face);

                                if (outter is IgesCurveOnAParametricSurface cps)
                                {
                                    var owire = GetWire(cps);
                                    face.Wires.Add(owire);
                                    face.OutterWire = owire;
                                }
                                foreach (var item in tps.BoundaryEntities.OfType<IgesCurveOnAParametricSurface>())
                                {
                                    face.Wires.Add(GetWire(item));
                                }
                            }
                        }
                        break;
                    case IgesEntityType.Line:
                        IgesLine line = (IgesLine)entity;
                        // ...
                        break;
                        // ...
                }
            }

            if (Part.AutoExtractMeshOnLoad)
                ret.ExtractMesh();
            try
            {
                ret.FixNormals();
            }
            catch (Exception ex)
            {
                DebugHelpers.Error("fix normals exception: " + ex.Message);
            }
            DebugHelpers.Progress(false, 0);
            return ret;
        }

        public static BRepWire GetWire(IgesCurveOnAParametricSurface cps)
        {
            BRepWire ret = new BRepWire();
            if (cps.CurveDefinitionC is IgesCompositeCurve cc)
            {
                foreach (var citem in cc.Entities)
                {
                    if (citem is IgesLine ln)
                    {
                        var s = new OpenTK.Vector3d(ln.P1.X, ln.P1.Y, ln.P1.Z);
                        var ee = new OpenTK.Vector3d(ln.P2.X, ln.P2.Y, ln.P2.Z);
                        ret.Edges.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
                            {
                                Point = s,
                                Vector = ee - s
                            },
                            Start = s,
                            End = ee
                        });
                    }
                }

            }
            return ret;
        }
    }
}
