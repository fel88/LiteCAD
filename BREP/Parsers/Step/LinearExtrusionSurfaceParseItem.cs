using System;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class LinearExtrusionSurfaceParseItem : ParserItem
    {
        public override string Key => "SURFACE_OF_LINEAR_EXTRUSION";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            LinearExtrusionSurface ret = new LinearExtrusionSurface();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var refs = spl.Skip(1).Where(z => !z.Contains('\'')).Select(z => int.Parse(z.TrimStart('#'))).ToArray();

            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();

            ret.Curve = (Curve)objs[0];
            ret.Vector = (Vector)objs[1];
            return ret;
        }
    }
}
