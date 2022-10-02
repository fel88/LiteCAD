using BREP.Parsers.Step;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Linq;

namespace BREP.BRep.Faces
{
    public class BRepToroidalSurfaceFace : BRepFace
    {
        public BRepToroidalSurfaceFace(Part parent) : base(parent) { }

        public override BRepFace Clone()
        {
            throw new NotImplementedException();
        }

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