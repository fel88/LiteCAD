using OpenTK;
using System;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class ColourRGBParseItem : ParserItem
    {
        public override string Key => "COLOUR_RGB";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            Vector3d ret = new Vector3d();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var zz = spl.Skip(1).Where(z => !z.Contains('\'')).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            ret.X = zz[0];
            ret.Y = zz[1];
            ret.Z = zz[2];
            return ret;
        }
    }

}
