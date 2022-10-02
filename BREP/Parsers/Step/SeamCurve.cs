using System.Collections.Generic;

namespace BREP.Parsers.Step
{
    public class SeamCurve : Curve
    {
        public List<Curve> Curves = new List<Curve>();
        public Curve Curve;
    }
}
