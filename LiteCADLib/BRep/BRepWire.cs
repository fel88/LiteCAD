using System.Collections.Generic;

namespace LiteCAD.BRep
{
    public class BRepWire
    {
        public List<BRepEdge> Edges = new List<BRepEdge>();
        public bool IsOutter = false;
    }
}