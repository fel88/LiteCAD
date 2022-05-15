using OpenTK;
using System;
using System.Linq;

namespace LiteCAD.Common
{
    public class ParallelConstraint : DraftConstraint
    {
        public DraftLine Element1;
        public DraftLine Element2;

        public ParallelConstraint(DraftLine draftPoint1, DraftLine draftPoint2)
        {

            var ar1 = new[] { draftPoint2.V0, draftPoint2.V1 };
            var ar2 = new[] { draftPoint1.V0, draftPoint1.V1 };
            if (ar1.Intersect(ar2).Count() > 0) throw new ArgumentException();

            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;

            return Math.Abs((Math.Abs(Vector2d.Dot(dp0.Dir, dp1.Dir)) - 1)) <= eps;
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
            var ar = new[] { dp0, dp1 }.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
            dp0 = ar[0];
            dp1 = ar[1];

            if (dp1.Frozen)
            {
                var temp = dp1;
                dp1 = dp0;
                dp0 = temp;
            }

            var dir = dp0.Dir;
            var len = dp1.Length;
            var center = dp1.Center;
            dp1.V0.Location = center + dir * len / 2;
            dp1.V1.Location = center - dir * len / 2;
        }

        public bool IsSame(ParallelConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }
    }
}