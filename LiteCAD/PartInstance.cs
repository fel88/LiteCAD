using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LiteCAD
{
    public class PartInstance : AbstractDrawable
    {
        public PartInstance(Part part)
        {
            Part = part;
        }
        public readonly Part Part;
        public Matrix4d Matrix;

        
        public override void Draw()
        {
            GL.PushMatrix();
            GL.MultMatrix(ref Matrix);
            Part.Draw();
            GL.PopMatrix();
        }
    }
}