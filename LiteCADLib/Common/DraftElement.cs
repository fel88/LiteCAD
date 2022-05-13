namespace LiteCAD.Common
{
    public abstract class DraftElement
    {
        public Draft Parent;

        protected DraftElement(Draft parent)
        {
            Parent = parent;
        }
    }
}