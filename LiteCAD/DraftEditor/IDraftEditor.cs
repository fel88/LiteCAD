using LiteCAD.Common;
using LiteCAD.BRep.Editor;

namespace LiteCAD.DraftEditor
{
    public interface IDraftEditor
    {
        object nearest { get; }
        Draft Draft { get; }
        IDrawingContext DrawingContext { get; }
        void Backup();
        void ResetTool();

    }
}
