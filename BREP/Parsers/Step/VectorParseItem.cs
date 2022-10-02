using OpenTK;
using System;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class VectorParseItem : ParserItem
    {
        public override string Key => "VECTOR";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            Vector ret = new Vector();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            spl = spl.Where(z => !z.Contains('\'')).ToArray();
            var zz = spl.Skip(2).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            var refs = spl.Skip(1).Take(1).Select(z => int.Parse(z.TrimStart('#'))).ToArray();

            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();

            ret.Location = (Vector3d)objs[0];
            ret.Length = zz[0];

            return ret;
        }
    }
}
