using OpenTK;
using OpenTK.Graphics.OpenGL;
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
            writer.WriteLine($"<plane pos=\"{Position.X};{Position.Y};{Position.Z}\" normal=\"{Normal.X};{Normal.Y};{Normal.Z}\"/>");
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
}