using System;
using System.Linq;

namespace LiteCADLib.Parsers.Step
{
    public class SurfaceCurveParseItem : ParserItem
    {
        public override string Key => "SURFACE_CURVE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            SurfaceCurve ret = new SurfaceCurve();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var refs = spl.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Geometry = objs[0] as Curve;
            ret.Curves = objs.Skip(1).OfType<PCurve>().ToList();

            return ret;
        }
    }
}
