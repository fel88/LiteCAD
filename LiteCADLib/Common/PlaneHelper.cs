﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    public class PlaneHelper : AbstractDrawable, IEditFieldsContainer
    {
        public PlaneHelper()
        {

        }

        public PlaneHelper(XElement elem)
        {
            if (elem.Attribute("name") != null)
            {
                Name = elem.Attribute("name").Value;
            }
            var pos = elem.Attribute("pos").Value.Split(';').Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Position = new Vector3d(pos[0], pos[1], pos[2]); 
            var normal = elem.Attribute("normal").Value.Split(';').Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Normal = new Vector3d(normal[0], normal[1], normal[2]);
        }

        [EditField]
        public Vector3d Position { get; set; }

        [EditField]
        public Vector3d Normal { get; set; }

        [EditField]
        public int DrawSize { get; set; } = 10;

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<plane name=\"{Name}\" pos=\"{Position.X};{Position.Y};{Position.Z}\" normal=\"{Normal.X};{Normal.Y};{Normal.Z}\"/>");
        }

        public Vector3d[] GetBasis()
        {
            Vector3d[] shifts = new[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Vector3d axis1 = Vector3d.Zero;
            for (int i = 0; i < shifts.Length; i++)
            {
                var proj = ProjPoint(Position + shifts[i]);

                if (Vector3d.Distance(proj, Position) > 10e-6)
                {
                    axis1 = (proj - Position).Normalized();
                    break;
                }
            }
            var axis2 = Vector3d.Cross(Normal.Normalized(), axis1);

            return new[] { axis1, axis2 };
        }

        public Vector3d ProjPoint(Vector3d point)
        {
            var nrm = Normal.Normalized();
            var v = point - Position;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }

        public Line3D Intersect(PlaneHelper ps)
        {
            Line3D ret = new Line3D();

            var dir = Vector3d.Cross(ps.Normal, Normal);


            var k1 = ps.GetKoefs();
            var k2 = GetKoefs();
            var a1 = k1[0];
            var b1 = k1[1];
            var c1 = k1[2];
            var d1 = k1[3];

            var a2 = k2[0];
            var b2 = k2[1];
            var c2 = k2[2];
            var d2 = k2[3];



            var res1 = det2(new[] { a1, a2 }, new[] { b1, b2 }, new[] { -d1, -d2 });
            var res2 = det2(new[] { a1, a2 }, new[] { c1, c2 }, new[] { -d1, -d2 });
            var res3 = det2(new[] { b1, b2 }, new[] { c1, c2 }, new[] { -d1, -d2 });

            List<Vector3d> vvv = new List<Vector3d>();

            if (res1 != null)
            {
                Vector3d v1 = new Vector3d((float)res1[0], (float)res1[1], 0);
                vvv.Add(v1);

            }

            if (res2 != null)
            {
                Vector3d v1 = new Vector3d((float)res2[0], 0, (float)res2[1]);
                vvv.Add(v1);
            }
            if (res3 != null)
            {
                Vector3d v1 = new Vector3d(0, (float)res3[0], (float)res3[1]);
                vvv.Add(v1);
            }

            var pnt = vvv.OrderBy(z => z.Length).First();


            var r1 = IsOnPlane(pnt);
            var r2 = IsOnPlane(pnt);

            ret.Start = pnt;
            ret.End = pnt + dir * 100;
            return ret;
        }
        public bool IsOnPlane(Vector3d orig, Vector3d normal, Vector3d check, double tolerance = 10e-6)
        {
            return (Math.Abs(Vector3d.Dot(orig - check, normal)) < tolerance);
        }
        public bool IsOnPlane(Vector3d v)
        {
            return IsOnPlane(Position, Normal, v);
        }
        double[] det2(double[] a, double[] b, double[] c)
        {
            var d = a[0] * b[1] - a[1] * b[0];
            if (d == 0) return null;
            var d1 = c[0] * b[1] - c[1] * b[0];
            var d2 = a[0] * c[1] - a[1] * c[0];
            var x = d1 / d;
            var y = d2 / d;
            return new[] { x, y };
        }

        public bool Fill { get; set; }
        public double[] GetKoefs()
        {
            double[] ret = new double[4];
            ret[0] = Normal.X;
            ret[1] = Normal.Y;
            ret[2] = Normal.Z;
            ret[3] = -(ret[0] * Position.X + Position.Y * ret[1] + Position.Z * ret[2]);

            return ret;
        }

        public override void Draw()
        {
            if (!Visible) return;
            GL.Disable(EnableCap.Lighting);
            var basis = GetBasis();
            if (Fill)
            {
                GL.Color3(Color.LightGreen);

                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (-DrawSize));
                GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (DrawSize));
                GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (DrawSize));
                GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (-DrawSize));
                GL.End();
            }
            GL.Color3(Color.Blue);
            //if (Selected) GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (-DrawSize));
            GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (DrawSize));
            GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (DrawSize));
            GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (-DrawSize));
            GL.End();

            GL.Color3(Color.Orange);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Position);
            GL.Vertex3(Position + Normal.Normalized() * 10);

            GL.End();
        }

        public IName[] GetObjects()
        {
            List<IName> ret = new List<IName>();
            var fld = GetType().GetProperties();
            for (int i = 0; i < fld.Length; i++)
            {

                var at = fld[i].GetCustomAttributes(typeof(EditFieldAttribute), true);
                if (at != null && at.Length > 0)
                {
                    if (fld[i].PropertyType == typeof(Vector3d))
                    {
                        ret.Add(new VectorEditor(fld[i]) { Object = this });
                    }
                    if (fld[i].PropertyType == typeof(int))
                    {
                        ret.Add(new IntFieldEditor(fld[i]) { Object = this });
                    }
                }
            }
            return ret.ToArray();
        }
    }
    public class Line3D
    {
        public Vector3d Start;
        public Vector3d End;
        public Vector3d Dir
        {
            get
            {
                return (End - Start).Normalized();
            }
        }

        public bool IsPointOnLine(Vector3d pnt, float epsilon = 10e-6f)
        {
            float tolerance = 10e-6f;
            var d1 = pnt - Start;
            if (d1.Length < tolerance) return true;
            if ((End - Start).Length < tolerance) throw new Exception("degenerated 3d line");
            var crs = Vector3d.Cross(d1.Normalized(), (End - Start).Normalized());
            return Math.Abs(crs.Length) < epsilon;
        }
        public bool IsPointInsideSegment(Vector3d pnt, float epsilon = 10e-6f)
        {
            if (!IsPointOnLine(pnt, epsilon)) return false;
            var v0 = (End - Start).Normalized();
            var v1 = pnt - Start;
            var crs = Vector3d.Dot(v0, v1) / (End - Start).Length;
            return !(crs < 0 || crs > 1);
        }
        public bool IsSameLine(Line3D l)
        {
            return IsPointOnLine(l.Start) && IsPointOnLine(l.End);
        }

        public void Shift(Vector3d vector3)
        {
            Start += vector3;
            End += vector3;
        }
    }

}