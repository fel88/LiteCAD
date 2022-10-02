using BREP.Parsers.Step;

namespace BREP.BRep
{
    public class UnsupportedCurveException : StepParserException
    {
        public UnsupportedCurveException() { }
        public UnsupportedCurveException(string msg) : base(msg) { }
    }
}