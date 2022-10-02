using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class ClosedShellParseItem : ParserItem
    {
        public override string Key => "CLOSED_SHELL";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            Shell ret = new Shell() { Closed = true };
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var refs = spl.Skip(1).Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Faces = objs.OfType<AdvancedFace>().ToList();

            return ret;
        }
    }
}
