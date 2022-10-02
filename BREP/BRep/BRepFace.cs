using BREP.BRep.Outlines;
using BREP.Common;
using BREP.Parsers.Step;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;

namespace BREP.BRep
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
        protected BRepFace()
        {
            Id = NewId++;
        }

        public Part Parent;
        public MeshNode Node;
        
        public virtual Line3D[] Get3DSegments(BRepEdge edge, double eps = 1e-8)
        {
            return new Line3D[] { };
        }
        public ProjectPolygon[] ProjectPolygons;

        public static bool SkipWireOnParseException = false;
        

        public List<OutlineItem> Outlines = new List<OutlineItem>();
        public List<OutlineItem> Items => Outlines;
        public BRepSurface Surface;
        public List<BRepWire> Wires = new List<BRepWire>();

        public BRepWire OutterWire;
        public bool Visible { get; set; } = true;
        public virtual void Load(AdvancedFace face)
        {
            checkWires();
        }
        public abstract MeshNode ExtractMesh();
        public virtual MeshNode UpdateMesh(ProjectPolygon[] polygons)
        {
            return ExtractMesh();
        }


        protected void checkWires()
        {
            foreach (var item in Wires)
            {
                if (!item.IsClosed())
                {
                    throw new BrepException("not closed wire detected");
                }
            }
        }

        public abstract BRepFace Clone();

        public virtual void Transform(Matrix4d mtr4)
        {

        }
    }

    
}