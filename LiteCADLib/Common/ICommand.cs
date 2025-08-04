using System;

namespace LiteCAD.Common
{
    public interface ICommand
    {
        string Name { get; }
        Action<IDrawable, IEditor> Process { get; }
    }
}