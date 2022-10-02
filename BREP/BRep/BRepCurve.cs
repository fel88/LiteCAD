using OpenTK;
using System;

namespace BREP.BRep
{
    public class BRepCurve
    {
        public virtual BRepCurve Clone()
        {
            throw new NotImplementedException();
        }

        public virtual void Transform(Matrix4d mtr4)
        {
            throw new NotImplementedException();
        }
    }
}