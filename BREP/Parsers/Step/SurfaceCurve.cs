using System.Collections.Generic;

namespace BREP.Parsers.Step
{
    public class SurfaceCurve : Curve
    {
        public Curve Geometry;
        public List<PCurve> Curves = new List<PCurve>();
    }
}
