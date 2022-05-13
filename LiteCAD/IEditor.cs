using System;

namespace LiteCAD
{
    public interface IEditor
    {
        ITool CurrentTool { get; }
        event Action<ITool> ToolChanged;

        void ObjectSelect(object nearest);
        void ResetTool();
    }
}