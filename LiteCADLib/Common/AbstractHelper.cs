using System.Collections.Generic;
using System.IO;

namespace LiteCAD.Common
{
    public abstract class AbstractDrawable : IDrawable
    {

        public AbstractDrawable()
        {
            Id = FactoryHelper.NewId++;
        }
        public string Name { get; set; }

        public abstract void Draw();

        public virtual void RemoveChild(IDrawable dd)
        {
            Childs.Remove(dd);
        }

        public virtual void Store(TextWriter writer)
        {

        }

        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }

        public List<IDrawable> Childs { get; set; } = new List<IDrawable>();

        public IDrawable Parent { get; set; }
        public int Id { get; set; }

        protected TransformationChain _matrix = new TransformationChain();
        public TransformationChain Matrix { get => _matrix; set => _matrix = value; }

    }

    public static class FactoryHelper
    {

        public static int NewId;
    }
}