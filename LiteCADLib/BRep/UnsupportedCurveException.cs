using LiteCAD.Parsers.Step;

namespace LiteCAD.BRep
{
    public class UnsupportedCurveException : StepParserException
    {
        public UnsupportedCurveException() { }
        public UnsupportedCurveException(string msg) : base(msg) { }
    }
}