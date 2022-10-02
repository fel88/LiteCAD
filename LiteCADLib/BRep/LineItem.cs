using BREP.BRep.Outlines;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace LiteCAD.BRep
{
    public class OutlineItemHelper : DrawItem
    {

        readonly LineItem _lineItem;
        public OutlineItemHelper(LineItem l)
        {
            _lineItem = l;
        }
        public override void Draw()
        {
            GL.Color3(Color.Blue);
            if (Selected)
            {
                GL.Color3(Color.Violet);
            }
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Start);
            GL.Vertex3(End);
            GL.End();
        }
        public Vector3d Start => _lineItem.Start;
        public Vector3d End => _lineItem.End;
    }
}