using LiteCAD.Common;
using LiteCAD.Parsers.Step;
using System;
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

        public static bool SkipWireOnParseException = false;
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
        public virtual void Load(AdvancedFace face)
        {
            checkWires();
        }
        public abstract MeshNode ExtractMesh();

        
        protected void checkWires()
        {
            foreach (var item in Wires)
            {
                if (!item.IsClosed())
                {
                    throw new LiteCadException("not closed wire detected");                    
                }
            }
        }
    }
}