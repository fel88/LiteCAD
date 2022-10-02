using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class PlaneSurfaceParseItem : ParserItem
    {
        public override string Key => "PLANE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            Plane ret = new Plane();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var aa = spl.Skip(1).Select(z => z.Trim()).ToArray();            
            var refs = aa.Where(z=>z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();

            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Axis = (Axis2Placement3d)objs[0];

            return ret;
        }
    }
}
