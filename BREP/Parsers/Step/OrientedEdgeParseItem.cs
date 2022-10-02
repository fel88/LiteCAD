using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class OrientedEdgeParseItem : ParserItem
    {
        public override string Key => "ORIENTED_EDGE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            OrientedEdge ret = new OrientedEdge();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Curve = objs[0] as EdgeCurve;
            ret.Orientation = spl.Last().Contains("T.");

            return ret;
        }
    }
}
