using System.Collections.Generic;
using System.IO;

namespace LiteCAD.Common
{
    public interface IDrawable
    {
        IDrawable Parent { get; set; }
        List<IDrawable> Childs { get; }
        string Name { get; set; }
        bool Visible { get; set; }
        void Draw();
        bool Selected { get; set; }

        void RemoveChild(IDrawable dd);
        void Store(TextWriter writer);
    }
}