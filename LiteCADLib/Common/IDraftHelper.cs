using LiteCAD.BRep.Editor;

namespace LiteCAD.Common
{
    public interface IDraftHelper : IDrawable
    {

        Draft DraftParent { get; }
        bool Enabled { get; set; }

        void Draw(IDrawingContext ctx);

    }

}