using LiteCAD.Common;
using System.IO;
using System.Xml.Linq;

namespace LiteCAD
{
    public class Group : AbstractDrawable
    {        
        public Group()
        {
            Name = "group";
        }
        public override void Draw()
        {
            foreach (var item in Childs)
            {
                item.Draw();
            }
        }
        public Group(LiteCADScene scene, XElement e)
        {
            Name = e.Attribute("name").Value;
            Visible = bool.Parse(e.Attribute("visible").Value);
            _matrix.RestoreXml(e.Element("transform"));
            foreach (var item in e.Elements())
            {
                if (item.Name == "mesh")
                {
                    MeshModel d = new MeshModel(scene, item);
                    Childs.Add(d);
                    d.Parent = this;
                }
            }
        }
        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<group name=\"{Name}\" visible=\"{Visible}\">");
            writer.WriteLine("<transform>");
            _matrix.StoreXml(writer);
            writer.WriteLine("</transform>");
            foreach (var item in Childs)
            {
                item.Store(writer);
            }
            writer.WriteLine("</group>");
        }
    }
}