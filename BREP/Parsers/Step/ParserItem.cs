namespace BREP.Parsers.Step
{
    public abstract class ParserItem
    {
        public abstract bool IsApplicable(StepLineItem item);
        public abstract string Key { get; }
        public abstract object Parse(StepParseContext ctx, StepLineItem item);
    }
}
