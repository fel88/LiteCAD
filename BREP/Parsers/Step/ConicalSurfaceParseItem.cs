﻿using System;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class ConicalSurfaceParseItem : ParserItem
    {
        public override string Key => "CONICAL_SURFACE";

        public override bool IsApplicable(StepLineItem item)
        {
            return item.Value.Contains(Key);
        }

        public override object Parse(StepParseContext ctx, StepLineItem item)
        {
            ConicalSurface ret = new ConicalSurface();
            var spl = item.Value.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var zz = spl.Skip(2).Where(z => !z.Contains('\'') && !z.Trim().StartsWith("#")).Select(z => double.Parse(z, CultureInfo.InvariantCulture)).ToArray();
            var refs = spl.Skip(1).Where(z => !z.Contains('\'')).Take(1).Select(z => int.Parse(z.TrimStart('#'))).ToArray();

            var objs = refs.Select(z => ctx.GetRefObj(z)).ToArray();


            ret.Axis = (Axis2Placement3d)objs[0];
            ret.Radius = zz[0];
            ret.SemiAngle = zz[1];

            return ret;
        }
    }
}
