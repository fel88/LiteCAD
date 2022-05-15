using LiteCAD.Common;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace LiteCAD
{
    public class PartAssembly : AbstractDrawable
    {
        public PartAssembly()
        {
            Name = "assembly";
        }

        public PartAssembly(LiteCADScene scene, XElement item)
        {
            Name = item.Attribute("name").Value;
            foreach (var xitem in item.Elements("instance"))
            {
                Parts.Add(new PartInstance(scene, xitem));
            }
        }

        public List<PartInstance> Parts = new List<PartInstance>();

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<assembly name=\"{Name}\">");
            foreach (var item in Parts)
            {
                item.Store(writer);
            }
            writer.WriteLine("</assembly>");
        }

        public override void Draw()
        {
            foreach (var item in Parts)
            {
                item.Draw();
            }
        }
    }
}