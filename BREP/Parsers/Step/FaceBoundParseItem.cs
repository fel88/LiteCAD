using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class FaceBoundParseItem : ParserItem
    {
        public override string Key => "FACE_BOUND";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            FaceBound ret = new FaceBound();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var refs = spl.Skip(1).Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();

            //spl.Last()=="T"
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Loop = objs[0] as EdgeLoop;
            ret.Orientation = spl.Last().Contains("T.");

            return ret;
        }
    }
}
