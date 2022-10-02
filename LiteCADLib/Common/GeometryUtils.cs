using BREP.BRep;
using BREP.BRep.Curves;
using BREP.BRep.Surfaces;
using BREP.Common;
using LiteCAD.BRep;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace LiteCAD.Common
{
    public static class GeometryUtils
    {
        public static Vector2d GetProjPoint(Vector2d point, Vector2d loc, Vector2d norm)
        {
            var v = point - loc;
            var dist = Vector2d.Dot(v, norm);
            //var proj = point - dist * norm;
            //return proj;
            return dist * norm + loc;

        }
        public static Line3D[] SplitByPlane(this MeshNode mn, Matrix4d matrix, Plane pl)
        {
            List<Line3D> vv = new List<Line3D>();
            List<TriangleInfo> trianglesModifed = new List<TriangleInfo>();
            foreach (var item in mn.Triangles)
            {
                trianglesModifed.Add(item.Multiply(matrix));
            }
            foreach (var item in trianglesModifed)
            {
                try
                {
                    vv.AddRange(item.SplitByPlane(pl));
                }
                catch (Exception ex)
                {

                }
            }
            return vv.ToArray();
        }
        public static bool IntersectSegments(Vector2d p0, Vector2d p1, Vector2d q0, Vector2d q1, ref Vector2d c0)
        {
            double ux = p1.X - p0.X;
            double uy = p1.Y - p0.Y;
            double vx = q1.X - q0.X;
            double vy = q1.Y - q0.Y;
            double wx = p0.X - q0.X;
            double wy = p0.Y - q0.Y;

            double d = (ux * vy - uy * vx);
            double s = (vx * wy - vy * wx) / d;

            // Intersection point
            c0.X = p0.X + s * ux;
            c0.Y = p0.Y + s * uy;
            if (!IsPointInsideSegment(p0, p1, c0)) return false;
            if (!IsPointInsideSegment(q0, q1, c0)) return false;
            return true;
        }
        public static bool IsPointOnLine(Vector2d start, Vector2d end, Vector2d pnt, double epsilon = 10e-6f)
        {
            float tolerance = 10e-6f;
            var d1 = pnt - start;
            if (d1.Length < tolerance) return true;
            if ((end - start).Length < tolerance) throw new LiteCadException("degenerated line");
            d1 = d1.Normalized();
            var p2 = (end - start).Normalized();
            var crs = Vector3d.Cross(new Vector3d(d1.X, d1.Y, 0), new Vector3d(p2.X, p2.Y, 0));
            return Math.Abs(crs.Length) < epsilon;
        }
        public static bool IsPointInsideSegment(Vector2d start, Vector2d end, Vector2d pnt, double epsilon = 10e-6f)
        {
            if (!IsPointOnLine(start, end, pnt, epsilon)) return false;
            var diff1 = (pnt - start).Length + (pnt - end).Length;
            return Math.Abs(diff1 - (end - start).Length) < epsilon;
        }
        public static Vector3d? Intersect3dCrossedLines(Line3D ln0, Line3D ln1)
        {
            var v0 = ln0.Start;
            var v1 = ln1.Start;
            var d0 = ln0.Dir;
            var d1 = ln1.Dir;
            var d0n = ln0.Dir.Normalized();
            var d1n = ln1.Dir.Normalized();
            var check1 = Vector3d.Dot(Vector3d.Cross(d0n, d1n), v0 - v1);
            if (Math.Abs(check1) > 10e-6) return null;//parallel


            var cd = v1 - v0;
            var a1 = Vector3d.Cross(d1, cd).Length;
            var a2 = Vector3d.Cross(d1, d0).Length;
            var vv0 = v0 + d0n * 10000;
            var vv1 = v1 + d1n * 10000;
            var m1 = v0 + (a1 / a2) * d0;

            Line3D l1 = new Line3D() { Start = v0, End = vv0 };
            Line3D l2 = new Line3D() { Start = v1, End = vv1 };

            var m2 = v0 - (a1 / a2) * d0;
            if (Vector3d.Distance(m1, m2) < 10e-6) return m1;
            float epsilon = 10e-6f;
            if (l1.IsPointOnLine(m1, epsilon) && l2.IsPointOnLine(m1, epsilon)) return m1;

            if (l1.IsPointOnLine(m2, epsilon) && l2.IsPointOnLine(m2, epsilon)) return m2;

            return m1;
        }
        public static Vector2d[][] TriangulateWithHoles(Vector2d[][] points, Vector2d[][] holes, bool checkArea = true)
        {
            //holes = holes.Where(z => z.Length > 2).ToArray();//skip bad holes
            #region checker
            double area = 0;
            foreach (var item in points)
            {
                area += Math.Abs(signedArea(item));
            }

            if (holes != null)
                foreach (var item in holes)
                {
                    area -= Math.Abs(signedArea(item));
                }

            #endregion
            Polygon poly2 = new Polygon();

            foreach (var item in points)
            {
                var item2 = item;
                if (signedArea(item) < 0) { item2 = item.Reverse().ToArray(); }
                var a = item2.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                if (a.Count() > 2)
                {
                    poly2.Add(new TriangleNet.Geometry.Contour(a));
                }
            }

            if (holes != null)
                foreach (var item in holes)
                {
                    var item2 = item;
                    if (signedArea(item) < 0) { item2 = item.Reverse().ToArray(); }
                    var a = item2.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                    var interior = GetRandomInteriorPoint(item2);
                    if (a.Count() > 2)
                    {
                        poly2.Add(new TriangleNet.Geometry.Contour(a), new TriangleNet.Geometry.Point(interior.X, interior.Y));
                    }
                }

            ConstraintMesher.ScoutCounter = 0;


            var trng = (new GenericMesher()).Triangulate(poly2, new ConstraintOptions() { }, new QualityOptions());


            var ret1 = trng.Triangles.Select(z => new Vector2d[] {
                    new Vector2d(z.GetVertex(0).X, z.GetVertex(0).Y),
                    new Vector2d(z.GetVertex(1).X, z.GetVertex(1).Y),
                    new Vector2d(z.GetVertex(2).X, z.GetVertex(2).Y)
                }
            ).ToArray();

            double area2 = 0;
            foreach (var item in ret1)
            {
                area2 += Math.Abs(signedArea(item));
            }
            if (checkArea && Math.Abs(area2 - area) > 10e-3)
            {
                throw new LiteCadException("wrong triangulation. area mismatch");
            }

            return ret1;
        }
        public static bool pnpoly(PointF[] verts, float testx, float testy)
        {
            int nvert = verts.Length;
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verts[i].Y > testy) != (verts[j].Y > testy)) &&
                    (testx < (verts[j].X - verts[i].X) * (testy - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X))
                    c = !c;
            }
            return c;
        }
        public static bool pnpoly(Vector2d[] verts, double testx, double testy)
        {
            int nvert = verts.Length;
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verts[i].Y > testy) != (verts[j].Y > testy)) &&
                    (testx < (verts[j].X - verts[i].X) * (testy - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X))
                    c = !c;
            }
            return c;
        }

        public static Random Random = new Random();
        public static PointF GetRandomInteriorPoint(Vector2d[] polygon)
        {
            var rand = Random;
            var maxx = polygon.Max(z => z.X);
            var minx = polygon.Min(z => z.X);
            var maxy = polygon.Max(z => z.Y);
            var miny = polygon.Min(z => z.Y);
            var tx = rand.Next((int)(minx * 100), (int)(maxx * 100)) / 100f;
            var ty = rand.Next((int)(miny * 100), (int)(maxy * 100)) / 100f;
            PointF test = new PointF(tx, ty);

            int cntr = 0;
            while (true)
            {
                cntr++;
                if (cntr > 1000)
                {
                    throw new LiteCadException("GetRandomInteriorPoint failed");
                }
                if (pnpoly(polygon.ToArray(), test.X, test.Y))
                {
                    break;
                }
                tx = rand.Next((int)(minx * 10000), (int)(maxx * 10000)) / 10000f;
                ty = rand.Next((int)(miny * 10000), (int)(maxy * 10000)) / 10000f;
                test = new PointF(tx, ty);
            }
            return test;
        }
        static double signedArea(Vector2d[] polygon)
        {
            double area = 0.0;

            int j = 1;
            for (int i = 0; i < polygon.Length; i++, j++)
            {
                j = j % polygon.Length;
                area += (polygon[j].X - polygon[i].X) * (polygon[j].Y + polygon[i].Y);
            }

            return area / 2.0;
        }

        public static bool Contains(BRepWire bRepWire, TriangleInfo target)
        {
            var pnts = bRepWire.Edges.SelectMany(z => new[] { z.Start, z.End }).ToArray();
            var p = new[] { target.V0, target.V1, target.V2 };

            float eps = 1e-5f;
            return p.All(z => pnts.Any(uu => (uu - z).Length < eps));
        }

        public static bool AlmostEqual(double a, double b, double eps = 1e-8)
        {
            return Math.Abs(a - b) <= eps;
        }
        public static double CalculateArea(Vector2d[] Points)
        {
            // Add the first point to the end.
            int num_points = Points.Length;
            Vector2d[] pts = new Vector2d[num_points + 1];
            Points.CopyTo(pts, 0);
            pts[num_points] = Points[0];

            // Get the areas.
            double area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area += (pts[i + 1].X - pts[i].X) * (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return Math.Abs(area);
        }

        internal static Vector3d[] ExtractPoints(BRepEdge edge, BRepCircleCurve cc, double step)
        {
            List<Vector3d> pnts = new List<Vector3d>();

            //check
            var mtr44 = Matrix4d.CreateFromAxisAngle(cc.Axis, cc.SweepAngle);
            var res44 = Vector4d.Transform(new Vector4d(cc.Dir), mtr44);
            var checkPoint = (cc.Location + res44.Xyz);
            var realAxis = cc.Axis;
            if ((checkPoint - edge.End).Length > 1e-5)
            {
                //try to fix, by switch rotation
                realAxis *= -1;
            }

            for (double i = 0; i < cc.SweepAngle; i += step)
            {
                var mtr4 = Matrix4d.CreateFromAxisAngle(realAxis, i);
                var res = Vector4d.Transform(new Vector4d(cc.Dir), mtr4);
                pnts.Add(cc.Location + res.Xyz);
            }

            //check #2
            mtr44 = Matrix4d.CreateFromAxisAngle(realAxis, cc.SweepAngle);
            res44 = Vector4d.Transform(new Vector4d(cc.Dir), mtr44);
            checkPoint = (cc.Location + res44.Xyz);

            if ((checkPoint - edge.End).Length > 1e-5)
            {
                throw new LiteCadException("wrong end point");
            }

            if ((pnts.Last() - edge.End).Length > 1e-8 && (pnts.First() - edge.End).Length > 1e-8)
            {
                pnts.Add(edge.End);
            }
            return pnts.ToArray();
        }

        public static bool Contains(BRepWire bRepWire, TriangleInfo target, Matrix4d matrix4d)
        {
            var pnts = bRepWire.Edges.SelectMany(z => new[] { z.Start, z.End }).ToArray();
            pnts = pnts.Select(zz => Vector3d.Transform(zz, matrix4d)).ToArray();
            var p = new[] { target.V0, target.V1, target.V2 };

            float eps = 1e-5f;
            return p.All(z => pnts.Any(uu => (uu - z).Length < eps));
        }

        internal static Vector3d CalcConjugateNormal(Vector3d nm, Vector3d point0, Vector3d point1, Segment3d edge)
        {
            var neg = -nm;
            var axis = (edge.End - edge.Start).Normalized();
            BRepPlane pp = new BRepPlane() { Location = edge.Start, Normal = axis };
            var prj0 = pp.GetProjPoint(point0) - edge.Start;
            var prj1 = pp.GetProjPoint(point1) - edge.Start;

            var crs1 = Vector3d.Cross(prj0, prj1) / prj0.Length / prj1.Length;
            var ang2 = Vector3d.CalculateAngle(prj0, prj1);
            if (crs1.Length > 1e-8)
            {
                axis = -crs1.Normalized();
            }
            var mtr2 = Matrix4d.CreateFromAxisAngle(axis, -ang2);
            var trans = Vector3d.Transform(neg, mtr2);

            return trans;
        }

        public static double CalculateAngle(Vector3d dir1, Vector3d dir2, Vector3d axis)
        {
            var crs = Vector3d.Cross(dir2, dir1);
            var ang2 = Vector3d.CalculateAngle(dir1, dir2);
            if (!(Vector3d.Dot(axis, crs) < 0))
            {
                ang2 = (2 * Math.PI) - ang2;
            }
            return ang2;
        }

        internal static string PointHashKey(Vector2d z, int v)
        {
            return (int)(z.X * v) + ";" + (int)(z.Y * v);
        }

        internal static string SegmentHashKey(BREP.BRep.Segment z, int v)
        {
            return PointHashKey(z.Start, v) + ";" + PointHashKey(z.End, v);
        }

        internal static string SegmentHashKeyInvariant(BREP.BRep.Segment z, int v)
        {
            var str1 = PointHashKey(z.Start, v);
            var str2 = PointHashKey(z.End, v);
            if (string.Compare(str1, str2) < 0)
            {
                return str1 + ";" + str2;
            }
            return str2 + ";" + str1;
        }

        public static double polygonArea(NFP polygon)
        {
            double area = 0;
            int i, j;
            for (i = 0, j = polygon.Points.Length - 1; i < polygon.Points.Length; j = i++)
            {
                area += (polygon.Points[j].X + polygon.Points[i].X) * (polygon.Points[j].Y
                    - polygon.Points[i].Y);
            }
            return 0.5f * area;
        }

        internal static double signed_area(Vector2d[] polygon)
        {
            double area = 0.0;

            int j = 1;
            for (int i = 0; i < polygon.Length; i++, j++)
            {
                j = j % polygon.Length;
                area += (polygon[j].X - polygon[i].X) * (polygon[j].Y + polygon[i].Y);
            }

            return area / 2.0;

        }
    }
}