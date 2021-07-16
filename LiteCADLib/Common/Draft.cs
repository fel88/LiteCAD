using OpenTK;
using System;
using System.Collections.Generic;

namespace LiteCAD.Common
{
    public class Draft : AbstractDrawable
    {
        public List<Vector3d> Points = new List<Vector3d>();

        public override void Draw()
        {
            throw new NotImplementedException();
        }
    }
}