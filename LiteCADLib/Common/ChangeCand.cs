using OpenTK;
using OpenTK.Mathematics;

namespace LiteCAD.Common
{
    public class ChangeCand
    {
        public DraftPoint Point;
        public Vector2d Position;
        public void Apply()
        {
            Point.SetLocation(Position);
        }

    }

}