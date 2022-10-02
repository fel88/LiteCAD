using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class SeamCurveParseItem : ParserItem
    {
        public override string Key => "SEAM_CURVE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            SeamCurve ret = new SeamCurve();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            if (objs.Any(z => z == null))
            {
                throw new StepParserException("empty curve");
            }
            ret.Curve = objs[0] as Curve;
            for (int i = 1; i < objs.Length; i++)
            {
                ret.Curves.Add(objs[i] as Curve);
            }

            return ret;
        }
    }
}
