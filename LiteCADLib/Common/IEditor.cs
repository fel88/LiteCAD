using LiteCAD.Graphics;
using System;
using System.Collections;

namespace LiteCAD.Common
{
    public interface IEditor
    {
        ISceneObject[] Parts { get; }
        IList SelectedObjects { get; }
        void UpdateList();
        void AddPart(ISceneObject part);
        ITool CurrentTool { get; }
        event Action<ITool> ToolChanged;
        IntersectInfo Pick { get; }
        void ObjectSelect(object nearest);
        void ResetTool();
        void Backup();
        void Undo();
    }
}