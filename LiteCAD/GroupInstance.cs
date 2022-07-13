using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Linq;

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