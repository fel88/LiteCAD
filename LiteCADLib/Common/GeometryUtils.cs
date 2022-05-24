using LiteCAD.BRep;
using LiteCAD.BRep.Surfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace LiteCAD.Common
{
    public class GeometryUtils
    {
        public static Vector2d GetProjPoint(Vector2d point, Vector2d loc, Vector2d norm)
        {
            var v = point - loc;
            var dist = Vector2d.Dot(v, norm);
            //var proj = point - dist * norm;
            //return proj;
            return dist * norm + loc;
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
    }
}