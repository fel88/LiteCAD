using System;
using System.Linq;

namespace BREP.Parsers.Step
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
            if (ret.Geometry == null)
            {
                throw new StepParserException($"empty geometry: {item.Value}");
            }
            ret.Curves = objs.Skip(1).OfType<PCurve>().ToList();

            return ret;
        }
    }
}
