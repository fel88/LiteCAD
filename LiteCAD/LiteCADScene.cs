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
            var t = Parts.SelectMany(z=>z.GetAll(p)).ToArray();
            return t;
        }

        internal void FromXml(string fileName)
        {
            FactoryHelper.NewId = 0;
            DraftElement.NewId = 0;
            var doc = XDocument.Load(fileName);
            var root = doc.Descendants("root");

            foreach (var item in root.Elements())
            {
                if (item.Name == "draft")
                {
                    Draft d = new Draft(item);
                   
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
        }

        public void SaveToXml(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("<?xml version=\"1.0\"?>");
                    writer.WriteLine("<root>");
                    foreach (var item in Parts)
                    {
                        item.Store(writer);
                    }
                    writer.WriteLine("</root>");
                }
            }
        }
    }

    
}