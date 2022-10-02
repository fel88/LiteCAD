namespace BREP.Parsers.Step
{
    public class StringTokenItem : ITokenItem
    {
        public string Token;
        public override string ToString()
        {
            return $"{GetType().Name}: {Token}";
        }
    }
}
