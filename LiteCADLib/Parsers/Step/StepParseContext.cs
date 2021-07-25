using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LiteCADLib.Parsers.Step
{
    public class StepParseContext
    {
        public StepParseContext()
        {
            ItemParsers.Add(new VertexPointParseItem());
            ItemParsers.Add(new EdgeLoopParseItem());
            ItemParsers.Add(new FaceBoundParseItem());
            ItemParsers.Add(new FaceOuterBoundParseItem());
            ItemParsers.Add(new OrientedEdgeParseItem());
            ItemParsers.Add(new AdvancedFaceParseItem());
            ItemParsers.Add(new EdgeCurveParseItem());
            ItemParsers.Add(new SurfaceCurveParseItem());
            ItemParsers.Add(new CylindricalSurfaceParseItem());
            ItemParsers.Add(new PlaneSurfaceParseItem());
            ItemParsers.Add(new CircleParseItem());
            ItemParsers.Add(new LineParseItem());
            ItemParsers.Add(new PCurveParseItem());
            ItemParsers.Add(new DefinitionalRepresentationParseItem());
            ItemParsers.Add(new ShellParseItem());

            if (ItemParsers.GroupBy(z => z.Key).Any(z => z.Count() > 1))
                throw new StepParserException("duplicate parser items");
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

        public Vector ParseVector(StepLineItem item)
        {
            Vector ret = new Vector();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            spl = spl.Where(z => !z.Contains('\'')).ToArray();
            var zz = spl.Skip(2).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            var refs = spl.Skip(1).Take(1).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            foreach (var ritem in refs)
            {
                if (Cache[ritem].Tag != null) continue;
                parseItem(Cache[ritem]);
            }

            var objs = refs.Select(z => Cache[z].Tag).ToArray();

            ret.Location = (Vector3d)objs[0];
            ret.Length = zz[0];

            return ret;
        }

        public Vector3d ParseCartesianPoint(StepLineItem item)
        {
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var zz = spl.Skip(1).Where(z => !z.Contains('\'')).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            if (zz.Length < 1 || zz.Length > 3)
            {
                throw new WrongArgumentsException();
            }
            Vector3d ret = new Vector3d();
            if (zz.Length >= 1)
                ret.X = zz[0];
            if (zz.Length >= 2)
                ret.Y = zz[1];
            if (zz.Length >= 3)
                ret.Z = zz[2];

            return ret;
        }

        public Axis2Placement3d ParseAxis2Placement3d(StepLineItem item)
        {
            Axis2Placement3d ret = new Axis2Placement3d();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var aa = spl.Skip(1).Select(z => z.Trim()).ToArray();

            var refs = aa.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            foreach (var ritem in refs)
            {
                if (Cache[ritem].Tag != null) continue;
                parseItem(Cache[ritem]);
            }
            var objs = refs.Select(z => Cache[z].Tag).ToArray();
            ret.Location = (Vector3d)objs[0];
            ret.Dir1 = (Vector3d)objs[1];
            ret.Dir2 = (Vector3d)objs[2];


            return ret;
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
                if (item.Value.Contains("AXIS2_PLACEMENT_3D"))
                {

                    item.Tag = ParseAxis2Placement3d(item);

                }
                else
                if (item.Value.Contains("CARTESIAN_POINT"))
                {
                    item.Tag = ParseCartesianPoint(item);
                }
                else
                if (item.Value.Contains("DIRECTION"))
                {
                    item.Tag = ParseCartesianPoint(item);
                }
                else
                if (item.Value.Contains("VECTOR"))
                {
                    item.Tag = ParseVector(item);
                }

            }
            catch (Exception ex)
            {

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
    public class Vector
    {
        public Vector3d Location;
        public double Length;
    }

    public class StepPlane : Surface
    {

    }
    public class Curve
    {

    }
    public class Circle : Curve
    {
        public double Radius;
        public Axis2Placement3d Axis;
    }
    public class Line : Curve
    {
        public Vector3d Point;
        public Vector Vector;
    }

    public class BoundedCurve : SurfaceCurve
    {

    }

    public class BSplineSurface : SurfaceCurve
    {
        public int Degree;
    }
    public class RationalBSplineSurface : SurfaceCurve
    {

    }
    public class BSplineSurfaceWithKnots : BSplineSurface
    {

    }
}
