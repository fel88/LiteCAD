using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class AdvancedFaceParseItem : ParserItem
    {
        public override string Key => "ADVANCED_FACE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            AdvancedFace ret = new AdvancedFace();

            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();

            ret.Bounds = objs.Except(new[] { objs.Last() }).OfType<FaceBound>().ToList();
                
            ret.Surface = (Surface)objs.Last();
            if(ret.Surface == null)
            {
                throw new StepParserException("empty surface");
            }
            return ret;
        }
    }
}
