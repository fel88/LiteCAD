using LiteCAD.Common;
using System.Collections.Generic;

namespace LiteCAD
{
    public class PartAssembly : AbstractDrawable
    {
        public PartAssembly()
        {
            Name = "assembly";
        }
        public List<PartInstance> Parts = new List<PartInstance>();
        public override void Draw()
        {
            foreach (var item in Parts)
            {
                item.Draw();
            }
        }
    }
}