using System.Collections.Generic;
using IxMilia.Step.Syntax;

namespace IxMilia.Step.Items
{
    public class StepSurfaceOfRevolution : StepCurveSurface
    {
        public override StepItemType ItemType => StepItemType.SurfaceOfRevolution;

        


        private StepSurfaceOfRevolution()
            : base()
        {
        }


        internal override IEnumerable<StepSyntax> GetParameters(StepWriter writer)
        {
            foreach (var parameter in base.GetParameters(writer))
            {
                yield return parameter;
            }

            //yield return new StepRealSyntax(MajorRadius);
        }

        /*internal static StepRepresentationItem CreateFromSyntaxList(StepBinder binder, StepSyntaxList syntaxList)
        {
            syntaxList.AssertListCount(3);
            var surface = new StepSurfaceOfRevolution();
            surface.Name = syntaxList.Values[0].GetStringValue();
            binder.BindValue(syntaxList.Values[1], v => surface.Position = v.AsType<StepAxis2Placement3D>());            
            return surface;
        }*/
    }
}
