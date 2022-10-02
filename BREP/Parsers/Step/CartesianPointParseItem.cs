using OpenTK;
using System;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class CartesianPointParseItem : ParserItem
    {
        public override string Key => "CARTESIAN_POINT";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var zz = spl.Skip(1).Where(z => !z.Contains('\'')).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            if (zz.Length < 1 || zz.Length > 3)
            {
                throw new WrongArgumentsException();
            }
            Vector3d ret = new Vector3d();
            if (zz.Length >= 1)
                ret.X = zz[0];
            if (zz.Length >= 2)
                ret.Y = zz[1];
            if (zz.Length >= 3)
                ret.Z = zz[2];

            return ret;
        }
    }
}
