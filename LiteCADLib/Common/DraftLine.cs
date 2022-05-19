using OpenTK;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    public class DraftLine : DraftElement
    {
        public readonly DraftPoint V0;
        public readonly DraftPoint V1;
        public bool Dummy { get; set; }//dummy line. don't export
        public DraftLine(XElement el, Draft parent) : base(parent)
        {
            var v0Id = int.Parse(el.Attribute("v0").Value);
            var v1Id = int.Parse(el.Attribute("v1").Value);
            Dummy = bool.Parse(el.Attribute("dummy").Value);
            Id = int.Parse(el.Attribute("id").Value);
            V0 = parent.DraftPoints.First(z => z.Id == v0Id);
            V1 = parent.DraftPoints.First(z => z.Id == v1Id);
        }

        public DraftLine(DraftPoint last, DraftPoint draftPoint, Draft parent) : base(parent)
        {
            this.V0 = last;
            this.V1 = draftPoint;
        }

        public Vector2d Center => (V0.Location + V1.Location) / 2;
        public Vector2d Dir => (V1.Location - V0.Location).Normalized();
        public double Length => (V1.Location - V0.Location).Length;
        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<line id=\"{Id}\" dummy=\"{Dummy}\" v0=\"{V0.Id}\" v1=\"{V1.Id}\" />");
        }
    }
}