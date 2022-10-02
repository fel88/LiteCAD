using System;

namespace BREP.Common
{
    public class BrepException : Exception
    {
        public BrepException(string str) : base(str) { }
    }
}