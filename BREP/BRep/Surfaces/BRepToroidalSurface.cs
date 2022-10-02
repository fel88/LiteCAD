using OpenTK;

namespace BREP.BRep.Surfaces
{
    public class BRepToroidalSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal; 
        public double MajorRadius { get; set; }
        public double MinorRadius { get; set; }

        public override bool IsOnSurface(Vector3d v, double eps = 1E-08)
        {
            throw new System.NotImplementedException();
        }
    }
}