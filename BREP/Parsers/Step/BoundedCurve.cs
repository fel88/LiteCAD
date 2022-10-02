using OpenTK;
using System.Collections.Generic;

namespace BREP.Parsers.Step
{
    public class BoundedCurve : Curve
    {
        public List<Vector3d> Poles = new List<Vector3d>();
        public List<double> Weights = new List<double>();
        public List<double> Knots = new List<double>();
        public List<int> Multiplicities = new List<int>();
        public int Degree;
        public List<Curve> Curves = new List<Curve>();
    }
}
