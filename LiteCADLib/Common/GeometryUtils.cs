﻿using LiteCAD.BRep;
using LiteCAD.BRep.Surfaces;
using OpenTK;
using System;
using System.Drawing;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace LiteCAD.Common
{
    public class GeometryUtils
    {
        public static Vector2d[][] TriangulateWithHoles(Vector2d[][] points, Vector2d[][] holes, bool checkArea = true)
        {
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

            while (true)
            {
                if (pnpoly(polygon.ToArray(), test.X, test.Y))
                {
                    break;
                }
                tx = rand.Next((int)(minx * 100), (int)(maxx * 100)) / 100f;
                ty = rand.Next((int)(miny * 100), (int)(maxy * 100)) / 100f;
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