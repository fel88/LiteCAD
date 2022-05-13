namespace LiteCAD.Common
{
    public class DraftLine : DraftElement
    {
        public readonly DraftPoint V0;
        public readonly DraftPoint V1;

        public DraftLine(DraftPoint last, DraftPoint draftPoint, Draft parent) : base(parent)
        {
            this.V0 = last;
            this.V1 = draftPoint;
        }
    }
}