using OpenTK;

namespace LiteCAD.Common
{
    public class DraftPoint : DraftElement
    {

        public Vector2d Location;

        public double X
        {
            get => Location.X; 
            set
            {
                Parent.RecalcConstraints();
                Location.X = value;
            }
        }
        public double Y
        {
            get => Location.Y;
            set
            {
                Location.Y = value;
                Parent.RecalcConstraints();
            }
        }

        public DraftPoint(Draft parent, float x, float y) : base(parent)
        {

            Location = new Vector2d(x, y);
        }
    }
}