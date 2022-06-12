using OpenTK;
using LiteCAD.Common;
using LiteCAD.BRep.Editor;
using System.Drawing;

namespace LiteCAD.DraftEditor
{
    public class EqualsConstraintHelper : AbstractDrawable, IDraftHelper
    {
        public readonly EqualsConstraint constraint;
        public EqualsConstraintHelper(EqualsConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public void Draw(DrawingContext ctx)
        {
            var dp0 = constraint.TargetLine.Center;
            var perp = new Vector2d(-constraint.TargetLine.Dir.Y, constraint.TargetLine.Dir.X);

            SnapPoint = (dp0);
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            var gap2 = 4;
            //create bezier here
            ctx.gr.FillEllipse(Brushes.Green, tr0.X - gap, tr0.Y - gap, gap * 2, gap * 2);
            ctx.gr.DrawLine(new Pen(Brushes.Orange, 3), tr0.X - gap2, tr0.Y + 5, tr0.X - gap2, tr0.Y - 5);
            ctx.gr.DrawLine(new Pen(Brushes.Orange, 3), tr0.X+gap2, tr0.Y + 5, tr0.X + gap2, tr0.Y - 5);
        }



        public override void Draw()
        {

        }
    }
}
