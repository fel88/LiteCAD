using OpenTK;
using System.Collections.Generic;

namespace LiteCAD.Common
{
    public interface IMesh
    {
        IEnumerable<Vector3d> GetPoints();
    }
}