using BREP.BRep;

namespace LiteCAD.Common
{
    public interface ISelectManager
    {
        bool IsSelected(BRepFace face);
        void Select(BRepFace f);
        void Unselect(BRepFace f);
    }
}