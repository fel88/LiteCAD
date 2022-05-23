using System.Drawing;
using OpenTK;
using LiteCAD.Common;
using LiteCAD.BRep.Editor;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.DraftEditor
{
    public class LinearConstraintHelper : AbstractDrawable, IDraftHelper
    {
        public readonly LinearConstraint constraint;
        public LinearConstraintHelper(LinearConstraint c)
        {
            constraint = c;
        }

        public decimal Length { get => constraint.Length; set => constraint.Length = value; }
        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;
        public int Shift { get; set; } = 10;
        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public void Draw(DrawingContext ctx)
        {
            var elems = new[] { constraint.Element1, constraint.Element2 };
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            Pen p = new Pen(Color.Red, 1);
            p.CustomEndCap = bigArrow;
            p.CustomStartCap = bigArrow;
            if (constraint.Element1 is DraftPoint dp0 && constraint.Element2 is DraftPoint dp1)
            {
            //get perpencdicular
            var diff = (dp1.Location - dp0.Location).Normalized();
            var perp = new Vector2d(-diff.Y, diff.X);
            var tr0 = ctx.Transform(dp0.Location + perp * Shift);
            var tr1 = ctx.Transform(dp1.Location + perp * Shift);
            var tr2 = ctx.Transform(dp0.Location);
            var tr3 = ctx.Transform(dp1.Location);
            var text = (dp0.Location + perp * Shift + dp1.Location + perp * Shift) / 2 + perp;
            var trt = ctx.Transform(text);
            ctx.gr.DrawString(constraint.Length.ToString(), SystemFonts.DefaultFont, Brushes.Black, trt);
            SnapPoint = text;

                ctx.gr.DrawLine(p, tr0, tr1);

                ctx.gr.DrawLine(Pens.Red, tr0, tr2);
                ctx.gr.DrawLine(Pens.Red, tr1, tr3);
            }
            if (elems.Any(z => z is DraftLine) && elems.Any(z => z is DraftPoint))
            {
                var dp = elems.OfType<DraftPoint>().First();
                var dl = elems.OfType<DraftLine>().First();
                var pp = GeometryUtils.GetProjPoint(dp.Location, dl.V0.Location, dl.Dir);

                var diff = (dp.Location - pp).Normalized();
                var perp = new Vector2d(-diff.Y, diff.X);
                var tr0 = ctx.Transform(dp.Location + perp * Shift);
                var tr1 = ctx.Transform(pp + perp * Shift);
                var text = (dp.Location + perp * Shift + pp + perp * Shift) / 2 + perp;
                var trt = ctx.Transform(text);
                ctx.gr.DrawString(constraint.Length.ToString(), SystemFonts.DefaultFont, Brushes.Black, trt);
                SnapPoint = text;
                //get proj of point to line
                //var diff = (pp - dp.Location).Length;
            ctx.gr.DrawLine(p, tr0, tr1);
                var tr2 = ctx.Transform(dp.Location);
                var tr3 = ctx.Transform(pp);

            ctx.gr.DrawLine(Pens.Red, tr0, tr2);
            ctx.gr.DrawLine(Pens.Red, tr1, tr3);
        }
        }

        public override void Draw()
        {

        }
    }
}