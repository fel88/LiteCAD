using LiteCAD.Common;
using System.Collections.Generic;

namespace LiteCAD
{
    public class ExtrudeModifier : IDrawable, IEconomicsDetail
    {

        public ExtrudeModifier(Draft draft)
        {
            Source = draft;
            Source.Parent = this;
            CreatePart();
            Childs.Add(draft);
        }

        private void CreatePart()
        {
            //draft->brep
        }

        public decimal Height { get; set; }
        public bool Visible { get; set; }
        public bool Selected { get; set; }
        public string Name { get; set; } = "extrude";

        public List<IDrawable> Childs { get; set; } = new List<IDrawable>();

        public IDrawable Parent { get; set; }

        public decimal TotalCutLength => Source.CalcTotalCutLength();

        public decimal Volume => Source.CalcArea() * Height;

        Draft Source;
        Part Part;

        public void Draw()
        {
            if (Part == null) return;
            Part.Draw();
        }

        public void RemoveChild(IDrawable dd)
        {
            if (Source != dd) return;
            Childs.Remove(dd);
            Source = null;
            Part = null;

        }
    }
}