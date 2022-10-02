using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class EdgeLoopParseItem : ParserItem
    {
        public override string Key => "EDGE_LOOP";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            EdgeLoop ret = new EdgeLoop();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var refs = spl.Skip(1).Where(z => !z.Contains('\'')).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Edges = objs.Select(z => z as OrientedEdge).ToList();

            return ret;
        }
    }
}