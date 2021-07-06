using OpenTK;
using System;
using System.Collections.Generic;

namespace LiteCAD.Common
{
    public class Draft : IDrawable
    {
        public List<Vector3d> Points = new List<Vector3d>();

        public void Draw()
        {
            throw new NotImplementedException();
        }
    }

}