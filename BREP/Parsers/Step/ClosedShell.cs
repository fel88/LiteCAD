using System.Collections.Generic;

namespace BREP.Parsers.Step
{
    public class Shell
    {
        public bool Closed;
        public List<AdvancedFace> Faces = new List<AdvancedFace>();
    }
}
