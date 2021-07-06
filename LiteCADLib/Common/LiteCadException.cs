using System;

namespace LiteCAD.Common
{
    public class LiteCadException : Exception
    {
        public LiteCadException(string str) : base(str) { }
    }
}