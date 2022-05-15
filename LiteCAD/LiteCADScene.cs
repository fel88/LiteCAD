using LiteCAD.Common;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace LiteCAD
{
    public class LiteCADScene
    {
        public List<IDrawable> Parts = new List<IDrawable>();

        internal void FromXml(string fileName)
        {
            var doc = XDocument.Load(fileName);
            var root = doc.Descendants("root");
            foreach (var item in root.Elements())
            {
                if (item.Name == "draft")
                {
                    Draft d = new Draft(item);
                    
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