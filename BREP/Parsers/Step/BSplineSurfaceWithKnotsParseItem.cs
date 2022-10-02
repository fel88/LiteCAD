using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BREP.Parsers.Step
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
            TokenList top = new TokenList();
            stack.Push(top);
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

            var top1 = (top.Tokens[1] as ListTokenItem).List;
            var dg = top1.Tokens.Where(z => z is StringTokenItem ss && ss.Token.All(char.IsDigit)).Select(z => z as StringTokenItem).ToArray();
            ret.uDegree = int.Parse(dg[0].Token);
            ret.vDegree = int.Parse(dg[1].Token);

            var cp = (top1.Tokens.First(z => z is ListTokenItem) as ListTokenItem).List.Tokens;
            List<Vector3d[]> list = new List<Vector3d[]>();
            foreach (var sub1 in cp.Where(z => z is ListTokenItem))
            {
                var ar1 = (sub1 as ListTokenItem).List.Tokens.Where(z => z is StringTokenItem ss && ss.Token.Trim().StartsWith("#")).Select(z => z as StringTokenItem).Select(z => z.Token).ToArray();
                var refs1 = ar1.Select(z => ctx.GetRefObj(int.Parse(z.Trim().TrimStart('#'))));
                list.Add(refs1.Select(z=>(Vector3d)z).ToArray());
            }
            ret.ControlPoints = list.ToArray();
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
