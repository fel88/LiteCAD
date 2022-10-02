using OpenTK;
using System;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class Axis2Placement3dParseItem : ParserItem
    {
        public override string Key => "AXIS2_PLACEMENT_3D";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            Axis2Placement3d ret = new Axis2Placement3d();
            var spl = item.Value.Split(new char[] { '\'', ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var aa = spl.Skip(1).Select(z => z.Trim()).ToArray();

            var refs = aa.Where(z => z.StartsWith("#")).Select(z => int.Parse(z.TrimStart('#'))).ToArray();            
            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();
            ret.Location = (Vector3d)objs[0];
            ret.Dir1 = (Vector3d)objs[1];
            ret.Dir2 = (Vector3d)objs[2];


            return ret;
        }
    }
}
