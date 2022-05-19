using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    [XmlName(XmlName = "linearConstraint")]

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
        public LinearConstraint(XElement el, Draft parent)
        {
            Element1 = parent.Elements.OfType<DraftPoint>().First(z => z.Id == int.Parse(el.Attribute("p0").Value));
            Element2 = parent.Elements.OfType<DraftPoint>().First(z => z.Id == int.Parse(el.Attribute("p1").Value));
            Length = Helpers.ParseDecimal(el.Attribute("length").Value);            
        }
        public LinearConstraint(DraftElement draftPoint1, DraftElement draftPoint2, decimal len)
        {
            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
            Length = len;

        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            var dp0 = Element1 as DraftPoint;
            var dp1 = Element2 as DraftPoint;
            var diff = (dp1.Location - dp0.Location).Length;
            return Math.Abs(diff - (double)Length) < eps;
        }

        internal void Update()
        {
            var dp0 = Element1 as DraftPoint;
            var dp1 = Element2 as DraftPoint;
            var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.SetLocation( dp0.Location + diff * (double)Length);
        }
        public override void RandomUpdate()
        {
            var dp0 = Element1 as DraftPoint;
            var dp1 = Element2 as DraftPoint;
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
            var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.SetLocation(dp0.Location + diff * (double)Length);
        }
        public bool IsSame(LinearConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return Element1 == de || Element2 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<linearConstraint length=\"{Length}\" p0=\"{Element1.Id}\" p1=\"{Element2.Id}\"/>");
        }
    }

    public class ConstraintsException : Exception
    {
        public ConstraintsException(string msg) : base(msg) { }
    }
}