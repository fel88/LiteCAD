namespace LiteCAD.Common
{
    public interface IBREPPartContainer
    {
        int Id { get; }

        BREPPart Part { get; }
    }
}