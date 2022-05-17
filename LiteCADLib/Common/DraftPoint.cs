using OpenTK;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    public class DraftPoint : DraftElement
    {

        public Vector2d Location;
        

        public double X
        {
            get => Location.X;
            set
            {
                Location.X = value;
                Parent.RecalcConstraints();                
            }
        }
        public double Y
        {
            get => Location.Y;
            set
            {
                Location.Y = value;
                Parent.RecalcConstraints();
            }
        }

        public DraftPoint(Draft parent, float x, float y) : base(parent)
        {

            Location = new Vector2d(x, y);
        }
        public DraftPoint(Draft parent, double x, double y) : base(parent)
        {
            Location = new Vector2d(x, y);
        }

        public DraftPoint(XElement item2, Draft d):base(d)
        {
            Id = int.Parse(item2.Attribute("id").Value);
            X = double.Parse(item2.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            Y = double.Parse(item2.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<point id=\"{Id}\" x=\"{X}\" y=\"{Y}\" />");
        }
    }
}