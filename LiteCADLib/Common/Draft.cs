using OpenTK;
using System;
using System.Collections.Generic;

namespace LiteCAD.Common
{
    public class Draft : AbstractDrawable
    {
        public List<Vector3d> Points = new List<Vector3d>();

        public PlaneHelper Plane;
        
        public override void Draw()
        {
            
        }
    }
}