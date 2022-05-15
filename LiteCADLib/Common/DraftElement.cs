using System;
using System.IO;

namespace LiteCAD.Common
{
    public abstract class DraftElement
    {
        public int Id { get; set; }
        public bool Frozen { get; set; }//can't be changed during constraints satisfaction
        public static int NewId { get; set; }
        public Draft Parent;

        protected DraftElement(Draft parent)
        {
            Parent = parent;
            Id = NewId++;
        }

        public abstract void Store(TextWriter writer);
        
    }
}