using System.Collections.Generic;

namespace LiteCADLib.Parsers.Step
{
    public class EdgeCurve
    {
        public VertexPoint Start;
        public VertexPoint End;
        public Curve EdgeGeometry;
    }
    public class SeamCurve : Curve
    {
        public List<Curve> Curves = new List<Curve>();
        public Curve Curve;
    }
}
