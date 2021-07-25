using System.Collections.Generic;

namespace LiteCADLib.Parsers.Step
{
    public class SurfaceCurve : Curve
    {
        public Curve Geometry;
        public List<PCurve> Curves = new List<PCurve>();
    }
}
