using OpenTK;
using System;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class LineParseItem : ParserItem
    {
        public override string Key => "LINE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Trim().StartsWith(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            Line ret = new Line();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var refs = spl.Skip(1).Where(z => !z.Contains('\'')).Select(z => int.Parse(z.TrimStart('#'))).ToArray();

            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();

            ret.Point = (Vector3d)objs[0];
            ret.Vector = objs[1] as Vector;

            return ret;
        }
    }
}
