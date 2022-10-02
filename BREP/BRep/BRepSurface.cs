using OpenTK;
using System;

namespace BREP.BRep
{
    public abstract class BRepSurface
    {
        public abstract bool IsOnSurface(Vector3d v, double eps = 1e-8);

        public virtual BRepSurface Clone()
        {
            throw new NotImplementedException();
        }

        public virtual void Transform(Matrix4d mtr4)
        {
            throw new NotImplementedException();
        }
    }
}