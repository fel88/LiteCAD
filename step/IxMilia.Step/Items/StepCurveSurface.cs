using IxMilia.Step.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Step.Items
{
    public class StepCurveSurface : StepCurve
    {
        public StepCurveSurface() : base("") { }


        public StepCurveSurface(string str) : base(str)
        {

        }
        public object Item0;
        public override StepItemType ItemType => StepItemType.CurveSurface;
        private StepCurve _edgeGeometry;

        public StepCurve EdgeGeometry
        {
            get { return _edgeGeometry; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _edgeGeometry = value;
            }
        }

        public List<StepCurve> Curves { get; } = new List<StepCurve>();
        
        internal static StepRepresentationItem CreateFromSyntaxList(StepBinder binder, StepSyntaxList syntaxList)
        {
            syntaxList.AssertListCount(4);
            var surface = new StepCurveSurface();
            surface.Name = syntaxList.Values[0].GetStringValue();
            binder.BindValue(syntaxList.Values[1], v => surface.EdgeGeometry = v.AsType<StepCurve>());

            var boundsList = syntaxList.Values[2].GetValueList();

            surface.Curves.Clear();
            surface.Curves.AddRange(Enumerable.Range(0, boundsList.Values.Count).Select(_ => (StepCurve)null));
            for (int i = 0; i < boundsList.Values.Count; i++)
            {
                var j = i; // capture to avoid rebinding
                binder.BindValue(boundsList.Values[j], v => surface.Curves[j] = v.AsType<StepCurve>());
            }

            
            //binder.BindValue(syntaxList.Values[1], v => surface.Item0 = v.AsType<StepBoundItem>());
            //binder.BindValue(syntaxList.Values[1], v => surface.Item0 = v.AsType<StepBoundItem>());
            //binder.BindValue(syntaxList.Values[1], v => surface.Item0 = v.AsType<StepBoundItem>());

            return surface;
        }

    }
}
