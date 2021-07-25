namespace LiteCADLib.Parsers.Step
{
    public class StepLineItem
    {
        public override string ToString()
        {
            return $"#{Index} = {Value}";
        }
        public int Index;
        public string Value;

        public object Tag;
    }
}
