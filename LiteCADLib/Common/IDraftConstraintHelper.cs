using OpenTK;
using OpenTK.Mathematics;

namespace LiteCAD.Common
{
    public interface IDraftConstraintHelper : IDraftHelper
    {
        DraftConstraint Constraint { get; }
        Vector2d SnapPoint { get; set; }


    }

}