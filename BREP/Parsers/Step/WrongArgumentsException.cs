namespace BREP.Parsers.Step
{
    public class WrongArgumentsException : StepParserException
    {
        public WrongArgumentsException(string text) : base(text) { }
        public WrongArgumentsException() : base() { }
    }
}
