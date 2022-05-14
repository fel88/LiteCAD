using System;
using System.Linq;

namespace LiteCAD.Common
{
    public class LinearConstraint : DraftConstraint
    {
        public DraftElement Element1;
        public DraftElement Element2;

        decimal _length;
        public decimal Length
        {
            get => _length; set
            {
                _length = value;
                Element1.Parent.RecalcConstraints();
            }
        }

        public LinearConstraint(DraftElement draftPoint1, DraftElement draftPoint2, decimal len)
        {
            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
            Length = len;

        }

        internal void Update()
        {
            var dp0 = Element1 as DraftPoint;
            var dp1 = Element2 as DraftPoint;
            var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.Location = dp0.Location + diff * (double)Length;
        }

        public bool IsSame(LinearConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }
    }
}