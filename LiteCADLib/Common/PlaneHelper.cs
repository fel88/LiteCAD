using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace LiteCAD.Common
{
    public class PlaneHelper : AbstractHelper, IEditFieldsContainer
    {
        public string Name { get; set; }

        [EditField]
        public Vector3d Position;
        [EditField]
        public Vector3d Normal;
        public int DrawSize { get; set; } = 10;
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
            var fld = GetType().GetFields();
            for (int i = 0; i < fld.Length; i++)
            {

                var at = fld[i].GetCustomAttributes(typeof(EditFieldAttribute), true);
                if (at != null && at.Length > 0)
                {
                    if (fld[i].FieldType == typeof(Vector3d))
                    {
                        ret.Add(new VectorEditor(fld[i]) { Object = this });
                    }
                }
            }
            return ret.ToArray();
        }
    }

    public class VectorEditor : IName
    {
        public VectorEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public double X
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).X;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.X = value;
                Field.SetValue(Object, v);
            }
        }

        public double Y
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).Y;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.Y = value;
                Field.SetValue(Object, v);
            }
        }

        public double Z
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).Z;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.Z = value;
                Field.SetValue(Object, v);
            }
        }
    }
    public class StringFieldEditor : IName
    {
        public StringFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public string Value
        {
            get
            {
                return ((string)Field.GetValue(Object));
            }
            set
            {
                Field.SetValue(Object, value);
            }
        }

    }
    public class IntFieldEditor : IName
    {
        public IntFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public int Value
        {
            get
            {
                return ((int)Field.GetValue(Object));
            }
            set
            {
                Field.SetValue(Object, value);
            }
        }

    }

    public interface IEditFieldsContainer
    {
        IName[] GetObjects();
    }

    public interface IName
    {
        string Name { get; set; }
    }
    public class EditFieldAttribute : Attribute
    {

    }
}