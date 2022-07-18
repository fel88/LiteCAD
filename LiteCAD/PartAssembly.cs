using LiteCAD.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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

            if (item.Element("transform") != null)
                _matrix.RestoreXml(item.Element("transform"));

            foreach (var xitem in item.Elements("instance"))
            {
                if (xitem.Attribute("partId") != null)
                    AddPart(new PartInstance(scene, xitem));
                if (xitem.Attribute("groupId") != null)
                    AddGroup(new GroupInstance(scene, xitem));
            }
        }

        public PartInstance[] Parts => Childs.OfType<PartInstance>().ToArray();
        public GroupInstance[] Groups => Childs.OfType<GroupInstance>().ToArray();

        public override IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            List<IDrawable> ret = new List<IDrawable>();
            ret.Add(this);
            foreach (var item in Parts)
            {
                var rr1 = item.GetAll(p);
                ret.AddRange(rr1);
            }
            foreach (var item in Groups)
            {
                var rr1 = item.GetAll(p);
                ret.AddRange(rr1);
            }
            //var ret = Parts.SelectMany(z => z.GetAll(p)).Union(new[] { this }).ToArray();
            return ret.ToArray();
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<assembly id=\"{Id}\" name=\"{Name}\">");
            writer.WriteLine("<transform>");
            _matrix.StoreXml(writer);
            writer.WriteLine("</transform>");
            foreach (var item in Parts)
            {
                item.Store(writer);
            }
            foreach (var item in Groups)
            {
                item.Store(writer);
            }
            writer.WriteLine("</assembly>");
        }

        public override void Draw()
        {
            GL.PushMatrix();
            Matrix4d dd = _matrix.Calc();
            GL.MultMatrix(ref dd);
            foreach (var item in Parts)
            {
                item.Draw();
            }
            foreach (var item in Groups)
            {
                item.Draw();
            }
            GL.PopMatrix();
        }

        internal void AddPart(PartInstance partInstance)
        {
            Childs.Add(partInstance);
            partInstance.Parent = this;
        }
        internal void AddGroup(GroupInstance d)
        {
            Childs.Add(d);
            d.Parent = this;
        }
    }
}