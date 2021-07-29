using System;

namespace LiteCAD.Parsers.Step
{
    public class StepParserException : Exception
    {
        public StepParserException(string text) : base(text) { }
        public StepParserException() : base() { }
    }
}
