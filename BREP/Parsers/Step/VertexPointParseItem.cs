using OpenTK;
using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class VertexPointParseItem : ParserItem
    {
        public override string Key => "VERTEX_POINT";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            VertexPoint ret = new VertexPoint();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var refs = spl.Skip(1).Where(z => !z.Contains('\'')).Take(1).Select(z => int.Parse(z.TrimStart('#'))).ToArray();
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Point = (Vector3d)objs[0];

            return ret;

        }
    }
}
