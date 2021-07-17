using OpenTK;

namespace LiteCAD.BRep
{
    public class BRepPlane : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        public Vector2d GetUVProjPoint(Vector3d point, Vector3d axis1, Vector3d axis2)
        {
            var p = GetProjPoint(point) - Location;
            var p1 = Vector3d.Dot(p, axis1);
            var p2 = Vector3d.Dot(p, axis2);
            return new Vector2d(p1, p2);
        }
        public Vector3d GetProjPoint(Vector3d point)
        {
            var v = point - Location;
            var nrm = Normal;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }
    }
    public class BRepLinearExtrusionSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal;
        public double Length;
        public Vector3d Vector;

    }
    public class BRepToroidalSurface : BRepSurface
    {
        public Vector3d Location;
        public Vector3d Normal; 
        public double MajorRadius { get; set; }
        public double MinorRadius { get; set; }

    }
}