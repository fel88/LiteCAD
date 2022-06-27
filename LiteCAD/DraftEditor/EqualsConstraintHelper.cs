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
            var editor = ctx.Tag as IDraftEditor;


            var hovered = editor.nearest == this;

            var dp0 = constraint.TargetLine.Center;
            var perp = new Vector2d(-constraint.TargetLine.Dir.Y, constraint.TargetLine.Dir.X);

            SnapPoint = (dp0) + constraint.TargetLine.Dir * 10 / ctx.zoom;
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            var gap2 = 6;
            var offset = 25;
            //create bezier here
            var shiftX = (int)(constraint.TargetLine.Dir.X * offset);
            var shiftY = (int)(constraint.TargetLine.Dir.Y * offset);

            ctx.gr.FillEllipse(hovered ? Brushes.Blue : Brushes.Green, tr0.X - gap + shiftX, tr0.Y - gap + shiftY, gap * 2, gap * 2);
            ctx.gr.DrawLine(new Pen(Brushes.Violet, 3), tr0.X - gap2 + shiftX, tr0.Y - 4 + shiftY, tr0.X + gap2 + shiftX, tr0.Y - 4 + shiftY);
            ctx.gr.DrawLine(new Pen(Brushes.Violet, 3), tr0.X - gap2 + shiftX, tr0.Y + 4 + shiftY, tr0.X + gap2 + shiftX, tr0.Y + 4 + shiftY);

            if (hovered)
            {
                var tr00 = ctx.Transform(constraint.SourceLine.V0.Location);
                var tr1 = ctx.Transform(constraint.SourceLine.V1.Location);
                var tr2 = ctx.Transform(constraint.TargetLine.V0.Location);
                var tr3 = ctx.Transform(constraint.TargetLine.V1.Location);
                ctx.gr.FillEllipse(Brushes.Red, tr2.X - 5, tr2.Y - 5, 10, 10);
                ctx.gr.FillEllipse(Brushes.Red, tr3.X - 5, tr3.Y - 5, 10, 10);

                ctx.gr.FillEllipse(Brushes.Red, tr00.X - 5, tr00.Y - 5, 10, 10);
                ctx.gr.FillEllipse(Brushes.Red, tr1.X - 5, tr1.Y - 5, 10, 10);
            }
        }

        public override void Draw()
        {

        }
    }
}
