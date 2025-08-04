namespace LiteCAD.Common
{
    public interface ICommandsContainer
    {
        ICommand[] Commands { get; }
    }
}