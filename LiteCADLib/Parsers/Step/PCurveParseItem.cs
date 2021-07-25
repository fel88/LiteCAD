using System;
using System.Linq;

namespace LiteCADLib.Parsers.Step
{
    public class PCurveParseItem : ParserItem
    {
        public override string Key => "PCURVE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            PCurve ret = new PCurve();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Surface = objs[0] as Surface;
            

            return ret;
        }
    }
}
