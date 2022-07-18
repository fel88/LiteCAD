using LiteCAD.Common;
using System;
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
                if (item.Name == "extrude")
                {
                    ExtrudeModifier d = new ExtrudeModifier(item);
                    Childs.Add(d);
                    d.Parent = this;
                }
                if (item.Name == "group")
                {
                    Group g = new Group(scene, item);
                    Childs.Add(g);
                    g.Parent = this;
                }
            }
            if (e.Attribute("id") != null)
            {
                Id = int.Parse(e.Attribute("id").Value);
                FactoryHelper.NewId = Math.Max(FactoryHelper.NewId, Id + 1);
            }
            else
            {
                Id = FactoryHelper.NewId++;
            }
        }
        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<group id=\"{Id}\" name=\"{Name}\" visible=\"{Visible}\">");
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