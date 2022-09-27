using IxMilia.Iges;
using IxMilia.Iges.Entities;
using LiteCAD.BRep;
using LiteCAD.BRep.Curves;
using LiteCAD.BRep.Faces;
using LiteCAD.BRep.Surfaces;
using LiteCAD.Common;
using System;
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
            foreach (IgesEntity entity in igesFile.Entities)
            {
                switch (entity.EntityType)
                {
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
