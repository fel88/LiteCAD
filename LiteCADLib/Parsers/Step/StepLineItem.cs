using System.Collections.Generic;

namespace LiteCAD.Parsers.Step
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
        public string[] Tokens()
        {
            return StepParser.Tokenize(Value);
        }
    }
    public class StringTokenItem : ITokenItem
    {
        public string Token;
        public override string ToString()
        {
            return $"{GetType().Name}: {Token}";
        }
    }
    public class ListTokenItem : ITokenItem
    {
        public TokenList List;
    }
    public interface ITokenItem
    {

    }
    public class TokenList
    {
        public List<ITokenItem> Tokens = new List<ITokenItem>();
    }
}
