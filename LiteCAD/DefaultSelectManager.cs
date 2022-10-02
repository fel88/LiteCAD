using BREP.BRep;
using LiteCAD.Common;
using System.Collections.Generic;

namespace LiteCAD
{
    public class DefaultSelectManager : ISelectManager
    {
        HashSet<BRepFace> selected = new HashSet<BRepFace>();
        public bool IsSelected(BRepFace face)
        {
            return selected.Contains(face);
        }

        public void Select(BRepFace f)
        {
            selected.Add(f);
        }

        public void Unselect(BRepFace f)
        {
            selected.Remove(f);
        }
    }
}