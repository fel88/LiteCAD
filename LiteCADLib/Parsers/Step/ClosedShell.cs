using System.Collections.Generic;

namespace LiteCAD.Parsers.Step
{
    public class Shell
    {
        public bool Closed;
        public List<AdvancedFace> Faces = new List<AdvancedFace>();
    }
}
