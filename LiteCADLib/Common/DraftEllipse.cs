using System;
using System.IO;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    public class DraftEllipse : DraftElement
    {
        public readonly DraftPoint Center;
        public double X { get => Center.Location.X; set => Center.Location.X = value; }
        public double Y { get => Center.Location.Y; set => Center.Location.Y = value; }
        public decimal Radius { get; set; }
        public DraftEllipse(DraftPoint center, decimal radius, Draft parent)
            : base(parent)
        {
            this.Center = center;
            this.Radius = radius;
        }
        public DraftEllipse(XElement elem, Draft parent)
          : base(parent)
        {
            var c = Helpers.ParseVector2(elem.Attribute("center").Value);
            Center = new DraftPoint(parent, c.X, c.Y);
            Radius = Helpers.ParseDecimal(elem.Attribute("radius").Value);
        }

        internal decimal CutLength()
        {
            return (2 * (decimal)Math.PI * Radius);
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<ellipse center=\"{Center.X}; {Center.Y}\" radius=\"{Radius}\">");
            writer.WriteLine("</ellipse>");
        }
    }
}