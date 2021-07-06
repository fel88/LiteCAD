using IxMilia.Step.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Step.Items
{
    public class StepPCurve : StepCurve
    {
        public StepPCurve() : base("") { }


        public StepPCurve(string str) : base(str)
        {

        }

        public StepSurface Surface;
        public override StepItemType ItemType => StepItemType.CurveSurface;
        internal static StepRepresentationItem CreateFromSyntaxList(StepBinder binder, StepSyntaxList syntaxList)
        {
            syntaxList.AssertListCount(3);
            var curve = new StepPCurve();
            curve.Name = syntaxList.Values[0].GetStringValue();
            binder.BindValue(syntaxList.Values[1], v =>
            {
                curve.Surface = v.AsType<StepSurface>();
                if (curve.Surface == null)
                {

                }
            });

            return curve;
        }
    }
}
