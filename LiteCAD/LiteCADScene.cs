using LiteCAD.Common;
using LiteCAD.DraftEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class LiteCADScene
    {
        public List<IDrawable> Parts = new List<IDrawable>();

        public IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            var t = Parts.Union(Parts.SelectMany(z => z.GetAll(p))).ToArray();
            return t;
        }
       public void Restore(XElement el)
        {
            FactoryHelper.NewId = 0;

            var root = el;

            int fId = 0;

            foreach (var item in root.Elements())
            {
                if (item.Name == "draft")
                {
                    Draft d = new Draft(item);
                    if (d.Elements.Any())
                        fId = Math.Max(d.Elements.Max(z => z.Id), fId);
                    Parts.Add(d);
                }
                if (item.Name == "extrude")
                {
                    ExtrudeModifier d = new ExtrudeModifier(item);

                    Parts.Add(d);
                }
                if (item.Name == "assembly")
                {
                    // PartAssembly d = new PartAssembly(this, item);
                    // Parts.Add(d);
                }
                if (item.Name == "mesh")
                {
                    MeshModel d = new MeshModel(this, item);
                    Parts.Add(d);
                }
                if (item.Name == "plane")
                {
                    PlaneHelper d = new PlaneHelper(item);
                    Parts.Add(d);
                }
                if (item.Name == "group")
                {
                    Group d = new Group(this, item);
                    Parts.Add(d);
                }
            }
            foreach (var item in root.Elements())
            {
                if (item.Name == "assembly")
                {
                    PartAssembly d = new PartAssembly(this, item);
                    Parts.Add(d);
                }
            }

            FactoryHelper.NewId = Math.Max(fId, GetAll(z => true).Max(z => z.Id) + 1);
            if (root.Attribute("newId") != null)
            {
                FactoryHelper.NewId = int.Parse(root.Attribute("newId").Value);
            }
        }

        internal void FromXml(string fileName)
        {
            FactoryHelper.NewId = 0;
            var doc = XDocument.Load(fileName);
            var root = doc.Descendants("root").First();
            Restore(root);
        }

        public void Store(StringWriter writer)
        {
            writer.WriteLine("<?xml version=\"1.0\"?>");
            writer.WriteLine($"<root newId=\"{FactoryHelper.NewId}\">");
            foreach (var item in Parts)
            {
                item.Store(writer);
            }
            writer.WriteLine("</root>");
        }

        public void SaveToXml(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    var sw = new StringWriter();
                    Store(sw);
                    writer.Write(sw.ToString());
                }
            }
        }
    }
}