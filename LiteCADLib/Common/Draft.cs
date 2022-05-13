using LiteCAD.BRep.Editor;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.Common
{
    public class Draft : AbstractDrawable
    {
        public List<Vector3d> Points3D = new List<Vector3d>();
        
        public void RecalcConstraints()
        {
            var lc = Constraints.OfType<LinearConstraint>();
            foreach (var item in lc)
            {
                item.Update();
            }
        }
        public List<DraftElement> Elements = new List<DraftElement>();
        public List<IDraftHelper> Helpers = new List<IDraftHelper>();
        public List<DraftConstraint> Constraints = new List<DraftConstraint>();
        public List<Vector2d> Childs = new List<Vector2d>();
        public PlaneHelper Plane;
        public Vector2d[] Points => DraftPoints.Select(z => z.Location).ToArray();
        public DraftPoint[] DraftPoints => Elements.OfType<DraftPoint>().ToArray();
        public DraftLine[] DraftLines => Elements.OfType<DraftLine>().ToArray();
        public void EndEdit()
        {
            //2d->3d
            var basis = Plane.GetBasis();
            Points3D.Clear();
            foreach (var item in DraftLines)
            {
                Points3D.Add(basis[0] * item.V0.X + basis[1] * item.V0.Y + Plane.Position);
                Points3D.Add(basis[0] * item.V1.X + basis[1] * item.V1.Y + Plane.Position);
            }
        }
        public override void Draw()
        {
            GL.Begin(PrimitiveType.Lines);
            foreach (var item in Points3D)
            {
                GL.Vertex3(item);
            }
            GL.End();
        }
    }
    public interface IDraftHelper
    {
        Vector2d SnapPoint { get; set; }

        void Draw(DrawingContext ctx);
    }

}