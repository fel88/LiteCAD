using LiteCAD.Common;
using System;

namespace LiteCAD
{
    public interface IEditor 
    {
        ISceneObject[] Parts { get; }
        ITool CurrentTool { get; }
        event Action<ITool> ToolChanged;
        IntersectInfo Pick { get; }
        void ObjectSelect(object nearest);
        void ResetTool();
        void Backup();
        void Undo();
    }
}