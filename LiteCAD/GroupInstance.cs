using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class GroupInstance : AbstractDrawable
    {
        public Group Group;
        public GroupInstance(Group group)
        {
            Group = group;
            Name = group.Name;
        }
        public Color Color { get; set; } = Color.LightGray;
        public override IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            var ret = Childs.SelectMany(z => z.GetAll(p)).OfType<IDrawable>().Where(zz => p(zz)).ToArray();
            return ret;
        }
        public GroupInstance(LiteCADScene scene, XElement xitem) : base(xitem)
        {
            Name = xitem.Attribute("name").Value;
            _matrix.RestoreXml(xitem.Element("transform"));
            if (xitem.Attribute("color") != null)
            {
                var rgb = xitem.Attribute("color").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                Color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }

            var groupId = int.Parse(xitem.Attribute("groupId").Value);
            var all = scene.GetAll(z => true).ToArray();
            var ps = scene.GetAll(z => true).OfType<Group>().First(z => z.Id == groupId);
            //var ps = scene.Parts.OfType<Group>().First(z => z.Id == groupId);
            Group = ps;
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<instance id=\"{Id}\" groupId=\"{Group.Id}\" name=\"{Name}\" color=\"{Color.R};{Color.G};{Color.B}\" >");
            writer.WriteLine("<transform>");
            _matrix.StoreXml(writer);
            writer.WriteLine("</transform>");
            writer.WriteLine("</instance>");
        }
        public override void Draw()
        {
            if (!Visible) return;
            GL.Color3(Color);
            GL.Enable(EnableCap.ColorMaterial);
            GL.PushMatrix();
            Matrix4d dd = _matrix.Calc();
            GL.MultMatrix(ref dd);
            foreach (var item in Group.Childs)
            {
                var v = item.Visible;
                item.Visible = true;
                item.Draw();
                item.Visible = v;
            }
            GL.PopMatrix();
            GL.Disable(EnableCap.ColorMaterial);
        }
    }
}