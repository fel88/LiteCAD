namespace LiteCAD
{
    public interface IEconomicsDetail
    {
        decimal TotalCutLength { get; }
        decimal Volume { get; }
        ProduceOperation Operation { get; set; }
    }

    public enum ProduceOperation
    {
        NotProducable, //already existed detail
        LaserCutting, 
        FDMPrinting
    }
}
