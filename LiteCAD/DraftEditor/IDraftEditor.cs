using LiteCAD.Common;
using LiteCAD.BRep.Editor;

namespace LiteCAD.DraftEditor
{
    public interface IDraftEditor
    {
        object nearest { get; }
        Draft Draft { get; }
        DrawingContext DrawingContext { get; }
        void Backup();

    }
}
