using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.Parsers.Step
{
    public class BSplineSurfaceWithKnotsParseItem : ParserItem
    {
        public override string Key => "B_SPLINE_SURFACE_WITH_KNOTS";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Trim().StartsWith(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            BSplineSurfaceWithKnots ret = new BSplineSurfaceWithKnots();

            var tkns = item.Tokens();
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
            //var zz = spl.Skip(2).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            //var refs = spl.Skip(1).Take(1).Select(z => int.Parse(z.TrimStart('#'))).ToArray();            
            //var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();

            //var refs1 = lists.First(z => z.Tokens.All(u => u is StringTokenItem)).Tokens.Select(z => z as StringTokenItem).Where(z => z.Token.StartsWith("#")).Select(z => int.Parse(z.Token.TrimStart('#'))).ToArray();            
            //var objs1 = refs1.Select(z => ctx.GetRefObj(z)).ToArray();

            //ret.ControlPoints = objs1.OfType<Vector3d>().ToArray();

            return ret;
        }
    }

}
