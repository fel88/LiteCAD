using OpenTK;

namespace LiteCAD.Common
{
    public class PlaneDataTransferObject
    {
        public Vector3d Normal;
        public double W;
        public PlaneDataTransferObject(Vector3d normal, double w)
        {
            Normal = normal;
            W = w;
        }

        public static PlaneDataTransferObject FromPoints(Vector3d a, Vector3d b, Vector3d c)
        {
            var n = Vector3d.Cross((b - a), (c - a)).Normalized();
            return new PlaneDataTransferObject(n, Vector3d.Dot(n, a));
        }
    }
}