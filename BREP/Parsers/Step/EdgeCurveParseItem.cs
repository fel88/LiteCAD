using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class EdgeCurveParseItem : ParserItem
    {
        public override string Key => "EDGE_CURVE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            EdgeCurve ret = new EdgeCurve();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Start = objs[0] as VertexPoint;
            ret.End = objs[1] as VertexPoint;
            ret.EdgeGeometry = objs[2] as Curve;
            ret.SameSense = spl.Last().Contains("T.");
            if (ret.EdgeGeometry == null)
            {
                throw new StepParserException("empty geomtery");
            }
            return ret;
        }
    }

}
