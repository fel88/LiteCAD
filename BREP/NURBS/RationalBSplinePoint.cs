using OpenTK;

namespace LiteCADLib.NURBS
{
    public class RationalBSplinePoint
    {

        public RationalBSplinePoint(Vector3d myPoint, double weight)
        {
            MyPoint = myPoint;
            Weight = weight;
        }

        private Vector3d pMyPoint = Vector3d.Zero;

        public Vector3d MyPoint
        {
            get { return pMyPoint; }
            set { pMyPoint = value; }
        }

        private double pWeight = 1d;

        public double Weight
        {
            get { return pWeight; }
            set { pWeight = value; }
        }

    }
}
