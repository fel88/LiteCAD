using System;
using System.Collections.Generic;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class BoundedSurfaceParseItem : ParserItem
    {
        public override string Key => "BOUNDED_SURFACE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Trim().TrimStart('(').StartsWith(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            BoundedSurface ret = new BoundedSurface();
            var tkns = item.Tokens();
            var degs = tkns.Where(z => z.All(char.IsDigit)).Take(2).Select(int.Parse).ToArray();
            int uDegree = degs[0];
            int vDegree = degs[1];
            if (vDegree == 1 && uDegree == 1)
            {
                PlaneSurface p = new PlaneSurface();
                throw new NotImplementedException();
            }


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

            var topt = (top.Tokens[0] as ListTokenItem).List.Tokens;
            for (int j = 0; j < topt.Count; j++)
            {
                ITokenItem xx = topt[j];
                if (xx is StringTokenItem sti)
                    if (sti.Token == "B_SPLINE_SURFACE_WITH_KNOTS")
                    {
                        var z1 = topt[j + 1];
                        var list1 = (z1 as ListTokenItem);
                        var k = new BSplineSurfaceWithKnots();
                        k.Parse(list1.List);
                        ret.Surfaces.Add(k);
                    }
                    else if (sti.Token == "RATIONAL_B_SPLINE_SURFACE")
                    {
                        var z1 = topt[j + 1];
                        var list1 = (z1 as ListTokenItem);
                        var k = new RationalBSplineSurface();
                        k.Parse(list1.List);
                        ret.Surfaces.Add(k);
                    }
                    else if (sti.Token == "B_SPLINE_SURFACE")
                    {
                        var z1 = topt[j + 1];
                        var list1 = (z1 as ListTokenItem);
                        var k = new BSplineSurface();
                        k.Parse(ctx, list1.List);
                        ret.Surfaces.Add(k);
                    }
            }


            spl = spl.Where(z => !z.Contains('\'')).ToArray();
            //var zz = spl.Skip(2).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            //var refs = spl.Skip(1).Take(1).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var refs1 = lists.First(z => z.Tokens.All(u => u is StringTokenItem)).Tokens.Select(z => z as StringTokenItem).Where(z => z.Token.StartsWith("#")).Select(z => int.Parse(z.Token.TrimStart('#'))).ToArray();
            //var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            var objs1 = refs1.Select(z => ctx.GetRefObj(z)).ToArray();



            return ret;
        }
    }
}
