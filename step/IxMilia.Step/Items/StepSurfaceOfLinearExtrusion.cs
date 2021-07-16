using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.Step.Syntax;

namespace IxMilia.Step.Items
{
    public class StepSurfaceOfLinearExtrusion : StepElementarySurface
    {
        public override StepItemType ItemType => StepItemType.SurfaceOfLinearExtrusion;


        internal override IEnumerable<StepSyntax> GetParameters(StepWriter writer)
        {
            foreach (var parameter in base.GetParameters(writer))
            {
                yield return parameter;
            }


        }
        private StepVector _vector;

        public StepVector Vector
        {
            get { return _vector; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _vector = value;
            }
        }

        public StepCurve Curve;
        
        internal static StepRepresentationItem CreateFromSyntaxList(StepBinder binder, StepSyntaxList syntaxList)
        {
            syntaxList.AssertListCount(3);
            var surface = new StepSurfaceOfLinearExtrusion();            
            surface.Name = syntaxList.Values[0].GetStringValue();
            binder.BindValue(syntaxList.Values[1], v => surface.Curve = v.AsType<StepCurve>());
            binder.BindValue(syntaxList.Values[2], v => surface.Vector = v.AsType<StepVector>());
            return surface;
        }

    }
}
