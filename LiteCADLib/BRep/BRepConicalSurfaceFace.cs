using IxMilia.Step.Items;
using LiteCAD.Common;
using LiteCAD.Parsers.Step;
using System;

namespace LiteCAD.BRep
{
    public class BRepConicalSurfaceFace : BRepFace
    {
        public BRepConicalSurfaceFace(Part parent) : base(parent) { }

        public override MeshNode ExtractMesh()
        {
            throw new NotImplementedException();
        }

        public override void Load(StepAdvancedFace face, StepSurface _cyl)
        {
            throw new NotImplementedException();

        }

        public override void Load(AdvancedFace face)
        {
            throw new NotImplementedException();
        }
    }
}