using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class PartInstance : AbstractDrawable
    {
        public PartInstance(Part part)
        {
            Part = part;
            Name = Part.Name;
        }

        public PartInstance(IPartContainer part)
        {
            Part = part.Part;
            Name = Part.Name;
            if (part is IDrawable dd)
            {
                Name = dd.Name;
            }
        }

        public PartInstance(LiteCADScene scene, XElement xitem)
        {
            Name = xitem.Attribute("name").Value;
            X = Helpers.ParseDouble(xitem.Attribute("x").Value);
            Y = Helpers.ParseDouble(xitem.Attribute("y").Value);
            Z = Helpers.ParseDouble(xitem.Attribute("z").Value);
            RotateZ = Helpers.ParseDouble(xitem.Attribute("rotZ").Value);
            var id = int.Parse(xitem.Attribute("id").Value);
            var ps = scene.Parts.OfType<IPartContainer>().First(z => z.Part.Id == id);
            Part = ps.Part;
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<instance id=\"{Part.Id}\" name=\"{Name}\" x=\"{X}\" y=\"{Y}\" z=\"{Z}\" rotZ=\"{RotateZ}\"/>");
        }

        public readonly Part Part;
        public Matrix4d Matrix = Matrix4d.Identity;

        public double X { get => Position.X; set => _position.X = value; }
        public double Y { get => Position.Y; set => _position.Y = value; }
        public double Z { get => Position.Z; set => _position.Z = value; }
        Vector3d _position;


        public Vector3d Position { get => _position; set => _position = value; }
        public double RotateZ { get; set; }

        public override void Draw()
        {
            GL.PushMatrix();
            GL.MultMatrix(ref Matrix);
            GL.Rotate(RotateZ, Vector3d.UnitZ);

            GL.Translate(Position);
            Part.Draw();
            GL.PopMatrix();
        }
    }
}