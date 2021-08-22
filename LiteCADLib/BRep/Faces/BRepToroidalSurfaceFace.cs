using LiteCAD.BRep.Curves;
using LiteCAD.BRep.Surfaces;
using LiteCAD.Common;
using LiteCAD.Parsers.Step;
using OpenTK;
using System;
using System.Linq;

namespace LiteCAD.BRep.Faces
{
    public class BRepToroidalSurfaceFace : BRepFace
    {
        public BRepToroidalSurfaceFace(Part parent) : base(parent) { }

        public override MeshNode ExtractMesh()
        {
            throw new NotImplementedException();
        }

        
        public override void Load(AdvancedFace face)
        {
            throw new NotImplementedException();
        }
    }
}