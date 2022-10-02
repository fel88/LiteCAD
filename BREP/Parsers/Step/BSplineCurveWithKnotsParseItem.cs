using OpenTK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class BSplineCurveWithKnotsParseItem : ParserItem
    {
        public override string Key => "B_SPLINE_CURVE_WITH_KNOTS";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Trim().StartsWith(Key);
        }



        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            BSplineCurveWithKnots ret = new BSplineCurveWithKnots();

            var tkns = item.Tokens();

            ret.Degree = new int[] { int.Parse(tkns.First(z => z.All(char.IsDigit))) };
            var spl = item.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            List<TokenList> lists = new List<TokenList>();
            Stack<TokenList> stack = new Stack<TokenList>();
            stack.Push(new TokenList());
            for (int i = 0; i < tkns.Length; i++)
            {
                if (tkns[i] == "(")
                {
                    TokenList l = new TokenList();
                    stack.Peek().Tokens.Add(new ListTokenItem() { List = l }); stack.Push(l); continue;
                }
                if (tkns[i] == ")") { lists.Add(stack.Pop()); continue; }
                if (stack.Any())
                {
                    stack.Peek().Tokens.Add(new StringTokenItem() { Token = tkns[i] });
                }
            }

            spl = spl.Where(z => !z.Contains('\'')).ToArray();

            var refs1 = lists.First(z => z.Tokens.All(u => u is StringTokenItem)).Tokens.Select(z => z as StringTokenItem).Where(z => z.Token.StartsWith("#")).Select(z => int.Parse(z.Token.TrimStart('#'))).ToArray();
            var refs2 = lists.Where(z => z.Tokens.All(u => u is StringTokenItem)).Skip(1).First().Tokens.Select(z => z as StringTokenItem).Where(z => z.Token.All(char.IsDigit)).Select(z => int.Parse(z.Token)).ToArray();
            var refs3 = lists.Where(z => z.Tokens.All(u => u is StringTokenItem)).Skip(2).First().Tokens.Select(z => z as StringTokenItem).Where(z => z.Token.Any(char.IsDigit)).Select(z => double.Parse(z.Token.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();

            var objs1 = refs1.Select(z => ctx.GetRefObj(z)).ToArray();


            ret.ControlPoints = objs1.OfType<Vector3d>().ToArray();
            ret.Multiplicities = refs2.OfType<int>().ToList();
            ret.Knots = refs3.OfType<double>().ToList();

            ret.Weights = ret.ControlPoints.Select(z => 1.0).ToList();

            ret.Param1 = ret.Knots[0];
            ret.Param2 = ret.Knots.Last();            

            return ret;
        }
    }
}
