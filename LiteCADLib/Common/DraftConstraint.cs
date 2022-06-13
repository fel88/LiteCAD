using System;
using System.IO;

namespace LiteCAD.Common
{
    public abstract class DraftConstraint
    {
        public abstract bool IsSatisfied(float eps = 1e-6f);
        public abstract void RandomUpdate();
        public bool Enabled { get; set; } = true;
        public Action BeforeChanged;
        public abstract bool ContainsElement(DraftElement de);

        internal abstract void Store(TextWriter writer);        
    }
}