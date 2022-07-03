using OpenTK;
using LiteCAD.Common;
using LiteCAD.BRep.Editor;
using System.Drawing;

namespace LiteCAD.DraftEditor
{
    public class VerticalConstraintHelper : AbstractDrawable, IDraftHelper
    {
        public readonly VerticalConstraint constraint;
        public VerticalConstraintHelper(VerticalConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public void Draw(IDrawingContext ctx)
        {
            var dp0 = constraint.Line.Center;
            var perp = new Vector2d(-constraint.Line.Dir.Y, constraint.Line.Dir.X);

            SnapPoint = (dp0);
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            //create bezier here
            ctx.FillCircle(Brushes.Green, tr0.X , tr0.Y , gap);
            ctx.DrawLine(new Pen(Brushes.Orange, 3), tr0.X, tr0.Y + 5, tr0.X, tr0.Y - 5);
        }

        public override void Draw()
        {

        }
    }
}
