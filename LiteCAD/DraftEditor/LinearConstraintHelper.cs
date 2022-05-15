using System.Drawing;
using OpenTK;
using LiteCAD.Common;
using LiteCAD.BRep.Editor;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace LiteCAD.DraftEditor
{
    public class LinearConstraintHelper : AbstractDrawable, IDraftHelper
    {
        public readonly LinearConstraint constraint;
        public LinearConstraintHelper(LinearConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public void Draw(DrawingContext ctx)
        {
            var dp0 = constraint.Element1 as DraftPoint;
            var dp1 = constraint.Element2 as DraftPoint;
            //get perpencdicular
            var diff = (dp1.Location - dp0.Location).Normalized();
            var perp = new Vector2d(-diff.Y, diff.X);
            var tr0 = ctx.Transform(dp0.Location + perp * 10);
            var tr1 = ctx.Transform(dp1.Location + perp * 10);
            var tr2 = ctx.Transform(dp0.Location);
            var tr3 = ctx.Transform(dp1.Location);
            var text = (dp0.Location + perp * 10 + dp1.Location + perp * 10) / 2 + perp;
            var trt = ctx.Transform(text);
            ctx.gr.DrawString(constraint.Length.ToString(), SystemFonts.DefaultFont, Brushes.Black, trt);
            SnapPoint = text;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            Pen p = new Pen(Color.Red, 1);
            p.CustomEndCap = bigArrow;
            p.CustomStartCap = bigArrow;



            ctx.gr.DrawLine(p, tr0, tr1);

            ctx.gr.DrawLine(Pens.Red, tr0, tr2);
            ctx.gr.DrawLine(Pens.Red, tr1, tr3);
        }



        public override void Draw()
        {

        }
    }

}
