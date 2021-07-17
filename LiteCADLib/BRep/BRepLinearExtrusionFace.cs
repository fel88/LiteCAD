using LiteCAD.Common;

namespace LiteCAD.BRep
{
    public class BRepLinearExtrusionFace : BRepFace
    {
        public BRepLinearExtrusionFace(Part p) : base(p) { }
        public override MeshNode ExtractMesh()
        {
            MeshNode ret = new MeshNode();
            return ret;
        }
    }
}