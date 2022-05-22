using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class PartInstance : AbstractDrawable, IPlaneSplittable, IMesh
    {
        /*public PartInstance(Part part)
        {
            Part = part;
            Name = Part.Name;
        }*/

        public PartInstance(IPartContainer part)
        {
            Part = part;
            Name = Part.Part.Name;
            if (part is IDrawable dd)
            {
                Name = dd.Name;
            }
        }

        public override IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            var ret = Childs.SelectMany(z => z.GetAll(p)).Union(new object[] { this, Part }).OfType<IDrawable>().Where(zz => p(zz)).ToArray();
            return ret;
        }

        public PartInstance(LiteCADScene scene, XElement xitem)
        {
            Name = xitem.Attribute("name").Value;
            _matrix.RestoreXml(xitem.Element("transform"));
            if (xitem.Attribute("color") != null)
            {
                var rgb = xitem.Attribute("color").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                Color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }

            var id = int.Parse(xitem.Attribute("id").Value);
            var ps = scene.GetAll(z => z is IPartContainer).OfType<IPartContainer>().First(z => z.Id == id);
            Part = ps;
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<instance id=\"{Part.Id}\" name=\"{Name}\" color=\"{Color.R};{Color.G};{Color.B}\">");
            writer.WriteLine("<transform>");
            _matrix.StoreXml(writer);
            writer.WriteLine("</transform>");
            writer.WriteLine("</instance>");
        }

        public readonly IPartContainer Part;
        public Color Color { get; set; } = Color.LightGray;
        public override void Draw()
        {
            if (!Visible) return;
            GL.Color3(Color);
            GL.Enable(EnableCap.ColorMaterial);
            GL.PushMatrix();
            Matrix4d dd = _matrix.Calc();
            GL.MultMatrix(ref dd);

            Part.Part.Draw();
            GL.PopMatrix();
            GL.Disable(EnableCap.ColorMaterial);
        }

        public Line3D[] SplitPyPlane(PlaneHelper ph)
        {
            //convert to mesh
            var tr = Part.Part.Nodes.SelectMany(z => z.SplitByPlane(Matrix.Calc(), ph)).ToArray();
            return tr;
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            if (!Visible)
                yield break;

            var mtrx = Matrix.Calc();
            foreach (var item in Part.Part.Nodes)
            {
                foreach (var t in item.Triangles)
                {
                    var tt = t.Multiply(mtrx);
                    foreach (var v in tt.Vertices)
                    {
                        yield return v.Position;
                    }
                }
            }
        }
    }
}