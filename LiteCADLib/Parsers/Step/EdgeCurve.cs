using System.Collections.Generic;

namespace LiteCAD.Parsers.Step
{
    public class EdgeCurve
    {
        public VertexPoint Start;
        public VertexPoint End;
        public Curve EdgeGeometry;
        public bool SameSense;
    }
    public class SeamCurve : Curve
    {
        public List<Curve> Curves = new List<Curve>();
        public Curve Curve;
    }
}
