using LiteCAD.BRep.Editor;
using OpenTK;

namespace LiteCAD.Common
{
    public interface IDraftHelper : IDrawable
    {
        DraftConstraint Constraint { get; }
        Vector2d SnapPoint { get; set; }

        bool Enabled { get; set; }

        void Draw(IDrawingContext ctx);
    }

}