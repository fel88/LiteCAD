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
            _matrix.RestoreXml(xitem.Element("transform"));

            var id = int.Parse(xitem.Attribute("id").Value);
            var ps = scene.Parts.OfType<IPartContainer>().First(z => z.Part.Id == id);
            Part = ps.Part;
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<instance id=\"{Part.Id}\" name=\"{Name}\">");
            writer.WriteLine("<transform>");
            _matrix.StoreXml(writer);
            writer.WriteLine("</transform>");
            writer.WriteLine("</instance>");
        }

        public readonly Part Part;
      
        public override void Draw()
        {
            GL.PushMatrix();
            Matrix4d dd = _matrix.Calc();
            GL.MultMatrix(ref dd);

            Part.Draw();
            GL.PopMatrix();
        }
    }
}