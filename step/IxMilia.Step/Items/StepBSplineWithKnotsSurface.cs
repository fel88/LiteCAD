using System.Collections.Generic;
using IxMilia.Step.Syntax;

namespace IxMilia.Step.Items
{
    public class StepBSplineWithKnotsSurface : StepElementarySurface
    {
        public override StepItemType ItemType => StepItemType.BSplineWithKnotsSurface;       


        private StepBSplineWithKnotsSurface()
            : base()
        {
        }


        internal override IEnumerable<StepSyntax> GetParameters(StepWriter writer)
        {
            foreach (var parameter in base.GetParameters(writer))
            {
                yield return parameter;
            }

            
        }

        internal static StepRepresentationItem CreateFromSyntaxList(StepBinder binder, StepSyntaxList syntaxList)
        {
            syntaxList.AssertListCount(4);
            var surface = new StepBSplineWithKnotsSurface();
            surface.Name = syntaxList.Values[0].GetStringValue();
            binder.BindValue(syntaxList.Values[1], v => surface.Position = v.AsType<StepAxis2Placement3D>());
            
            return surface;
        }
    }
}
