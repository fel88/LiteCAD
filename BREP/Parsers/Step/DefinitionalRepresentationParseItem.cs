using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class DefinitionalRepresentationParseItem : ParserItem
    {
        public override string Key => "DEFINITIONAL_REPRESENTATION";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            DefinitionalRepresentation ret = new DefinitionalRepresentation();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Curves = objs.OfType<Curve>().ToList();

            return ret;
        }
    }
}
