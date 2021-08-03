using IxMilia.Step.Items;
using LiteCAD.Common;
using LiteCAD.Parsers.Step;
using System.Collections.Generic;

namespace LiteCAD.BRep
{
    public abstract class BRepFace
    {
        public static int NewId = 0;
        public int Id { get; set; }
        public BRepFace(Part parent)
        {
            Parent = parent;
            Id = NewId++;
        }
        public Part Parent;
        public MeshNode Node;
        bool _selected;
        public bool Selected
        {
            get => _selected; set
            {
                _selected = value;
                foreach (var item in Items)
                {
                    item.Selected = value;
                }
            }
        }

        public List<DrawItem> Items = new List<DrawItem>();
        public BRepSurface Surface;
        public List<BRepWire> Wires = new List<BRepWire>();
        public BRepWire OutterWire;
        public bool Visible { get; set; } = true;
        public abstract void Load(StepAdvancedFace face, StepSurface surf);
        public abstract void Load(AdvancedFace face);
        public abstract MeshNode ExtractMesh();

    }
}