using System;

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

        internal decimal CutLength()
        {
            return (2 * (decimal)Math.PI * Radius);
        }
    }
}