using OpenTK;
using System;
using System.Linq;

namespace LiteCAD.Common
{
    public class PerpendicularConstraint : DraftConstraint
    {
        public DraftLine Element1;
        public DraftLine Element2;
        public DraftPoint CommonPoint;
        public PerpendicularConstraint(DraftLine draftPoint1, DraftLine draftPoint2)
        {
            var ar1 = new[] { draftPoint2.V0, draftPoint2.V1 };
            var ar2 = new[] { draftPoint1.V0, draftPoint1.V1 };
            if (ar1.Intersect(ar2).Count() != 1) throw new ArgumentException();
            CommonPoint = ar1.Intersect(ar2).First();
            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;

            return Math.Abs(Vector2d.Dot(dp0.Dir, dp1.Dir)) <= eps;
        }

        internal void Update()
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;
            /*var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.Location = dp0.Location + diff * (double)Length;*/
        }
        public override void RandomUpdate()
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;
            if (dp0.Frozen && dp1.Frozen)
            {
                throw new ConstraintsException("double frozen");
            }
            var ar = new[] { dp0, dp1 }.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
            dp0 = ar[0];
            dp1 = ar[1];
            if (dp1.Frozen)
            {
                var temp = dp1;
                dp1 = dp0;
                dp0 = temp;
            }

            if (dp1.V0 == CommonPoint)
            {
                var diff = dp1.Dir * dp1.Length;
                var projectV = new Vector2d(-dp0.Dir.Y, dp0.Dir.X);
                dp1.V1.Location = CommonPoint.Location + projectV * dp1.Length;
            }
            else
            {
                var diff = dp1.Dir * dp1.Length;
                var projectV = new Vector2d(-dp0.Dir.Y, dp0.Dir.X);
                dp1.V0.Location = CommonPoint.Location + projectV * dp1.Length;
            }
            /* var diff = (dp1.Location - dp0.Location).Normalized();
             dp1.Location = dp0.Location + diff * (double)Length;*/
        }
        public bool IsSame(PerpendicularConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }
    }
}