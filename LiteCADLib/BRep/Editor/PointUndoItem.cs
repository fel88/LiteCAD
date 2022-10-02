using BREP.BRep;
using OpenTK;

namespace LiteCAD.BRep.Editor
{
    public class PointUndoItem : IUndoItem
    {
        public ProjectPolygon Polygon;
        public int PointIndex;
        public Vector2d Position;
        public void Undo()
        {
            Polygon.Points[PointIndex] = Position;
        }
    }
}
