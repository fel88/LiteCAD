using BREP.BRep;
using BREP.BRep.Faces;
using BREP.Common;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class StepParseContext
    {
        public StepParseContext()
        {
            binds = new Type[][] {
                new[] { typeof(PlaneSurface), typeof(BRepPlaneFace) } ,
                new[] { typeof(CylindricalSurface), typeof(BRepCylinderSurfaceFace) } ,
                new[] { typeof(LinearExtrusionSurface), typeof(BRepLinearExtrusionFace) } ,
                new[] { typeof(ConicalSurface), typeof(BRepConicalSurfaceFace) } ,
                new[] { typeof(BSplineSurfaceWithKnots), typeof(BRepBSplineWithKnotsSurfaceFace) } ,
            };
            ItemParsers.Add(new VertexPointParseItem());
            ItemParsers.Add(new EdgeLoopParseItem());
            ItemParsers.Add(new VectorParseItem());
            ItemParsers.Add(new Axis2Placement3dParseItem());
            ItemParsers.Add(new DirectionParseItem());
            ItemParsers.Add(new ColourRGBParseItem());
            ItemParsers.Add(new FaceBoundParseItem());
            ItemParsers.Add(new CartesianPointParseItem());
            ItemParsers.Add(new FaceOuterBoundParseItem());
            ItemParsers.Add(new OrientedEdgeParseItem());
            ItemParsers.Add(new AdvancedFaceParseItem());
            ItemParsers.Add(new EdgeCurveParseItem());
            ItemParsers.Add(new SeamCurveParseItem());
            ItemParsers.Add(new SurfaceCurveParseItem());
            ItemParsers.Add(new CylindricalSurfaceParseItem());
            ItemParsers.Add(new ConicalSurfaceParseItem());
            ItemParsers.Add(new LinearExtrusionSurfaceParseItem());
            ItemParsers.Add(new PlaneSurfaceParseItem());
            ItemParsers.Add(new CircleParseItem());
            ItemParsers.Add(new EllipseParseItem());
            ItemParsers.Add(new BoundedCurveParseItem());
            ItemParsers.Add(new BoundedSurfaceParseItem());
            ItemParsers.Add(new BSplineCurveWithKnotsParseItem());
            ItemParsers.Add(new BSplineSurfaceWithKnotsParseItem());
            ItemParsers.Add(new LineParseItem());
            ItemParsers.Add(new PCurveParseItem());
            ItemParsers.Add(new DefinitionalRepresentationParseItem());
            ItemParsers.Add(new ClosedShellParseItem());
            ItemParsers.Add(new OpenShellParseItem());

            if (ItemParsers.GroupBy(z => z.Key).Any(z => z.Count() > 1))
                throw new StepParserException("duplicate parser items");
        }
        public static bool SkipFaceOnException = true;
        public static bool SkipWireOnException = true;
        public Part ToPart()
        {
            Part ret = new Part();
            foreach (var item in Shells.Select(z => z.Value.Tag).OfType<Shell>())
            {
                foreach (var face in item.Faces)
                {
                    try
                    {
                        var ee = toFace(ret, face);
                        if (ee == null)
                        {
                            DebugHelpers.Warning($"null face");
                            continue;
                        }
                        ret.Faces.Add(ee);
                    }
                    catch (Exception ex) when (SkipFaceOnException)
                    {
                        DebugHelpers.Error($"face error. skipped {ex.Message}");
                    }
                }
            }
            return ret;
        }

        Type[][] binds;
        BRepFace toFace(Part p, AdvancedFace face)
        {
            foreach (var item in binds)
            {
                if (face.Surface.GetType() == item[0])
                {
                    var ret = Activator.CreateInstance(item[1], p) as BRepFace;
                    ret.Load(face);                    
                    return ret;
                }
            }
            throw new StepParserException($"unknown surface face: {face.Surface}");
        }
        public List<ParserItem> ItemParsers = new List<ParserItem>();
        public Dictionary<int, StepLineItem> Cache = new Dictionary<int, StepLineItem>();
        public Dictionary<int, StepLineItem> Shells = new Dictionary<int, StepLineItem>();

        public void AddItem(StepLineItem s)
        {
            if (s.Value.Contains("SHELL"))
            {
                Shells.Add(s.Index, s);
            }
            Cache.Add(s.Index, s);
        }

        public object GetRefObj(int z)
        {
            updateRef(z);
            return Cache[z].Tag;
        }

        private void updateRef(int ritem)
        {
            if (Cache[ritem].Tag != null) return;
            parseItem(Cache[ritem]);
        }

        void parseItem(StepLineItem item)
        {
            try
            {
                foreach (var parser in ItemParsers)
                {
                    if (parser.IsApplicable(item))
                    {
                        item.Tag = parser.Parse(this, item);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelpers.Exception(ex);
            }
            if (item.Tag == null)
            {
                if (DebugInfoEnabled)
                    DebugHelpers.Error("not parsed item: " + item);
            }
        }
        public static bool DebugInfoEnabled = false;

        public void ParseShells()
        {
            foreach (var item in Cache)
            {
                if (item.Value.Tag != null) continue;
                if (item.Value.Value.Contains("SHELL"))
                    parseItem(item.Value);

            }
        }
    }
    public class Surface
    {
        public Axis2Placement3d Axis;
    }
    public class CylindricalSurface : Surface
    {
        public double Radius;
    }
    public class LinearExtrusionSurface : Surface
    {
        public Vector Vector;
        public Curve Curve;
    }
    public class ConicalSurface : Surface
    {
        public double Radius;
        public double SemiAngle;
    }
    public class Vector
    {
        public Vector3d Location;
        public double Length;
    }

    public class PlaneSurface : Surface
    {

    }
    public class Curve
    {

    }
    public class Circle : Curve, IAxis
    {
        public double Radius;
        public Axis2Placement3d Axis { get; set; }
    }

    public class Ellipse : Curve, IAxis
    {
        public double MinorRadius;
        public double MajorRadius;
        public Axis2Placement3d Axis { get; set; }
    }
    public interface IAxis
    {
        Axis2Placement3d Axis { get; }
    }
    public class Line : Curve
    {
        public Vector3d Point;
        public Vector Vector;
    }


    public class SplineCurve : Curve
    {

    }
    public class BoundedSurface : Surface
    {
        public List<Surface> Surfaces = new List<Surface>();
    }
}
