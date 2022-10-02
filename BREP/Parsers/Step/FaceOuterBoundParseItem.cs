using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class FaceOuterBoundParseItem : ParserItem
    {
        public override string Key => "FACE_OUTER_BOUND";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            FaceOuterBound ret = new FaceOuterBound();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var refs = spl.Skip(1).Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();

            //spl.Last()=="T"
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Loop = objs[0] as EdgeLoop;

            return ret;
        }
    }
}
