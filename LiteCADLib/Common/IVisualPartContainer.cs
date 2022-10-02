namespace LiteCAD.Common
{
    public interface IVisualPartContainer
    {
        int Id { get; }

        VisualPart Part { get; }
    }
}