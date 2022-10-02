using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class PCurveParseItem : ParserItem
    {
        public override string Key => "PCURVE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Trim().StartsWith(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            PCurve ret = new PCurve();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Surface = objs[0] as Surface;


            if (ret.Surface == null)
            {
                throw new StepParserException($"empty surface");
            }
            return ret;
        }
    }
}
