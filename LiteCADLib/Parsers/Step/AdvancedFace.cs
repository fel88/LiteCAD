﻿using System.Collections.Generic;

namespace LiteCAD.Parsers.Step
{
    public class AdvancedFace
    {
        public List<FaceBound> Bounds = new List<FaceBound>();
        public Surface Surface;
        public bool Flag;
    }
}
