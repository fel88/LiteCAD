using OpenTK;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class BSplineSurfaceWithKnots : BSplineSurface
    {
        public int uDegree;
        public int vDegree;
        public Vector3d[][] ControlPoints;


        internal void Parse(TokenList list1)
        {
            var l1 = (list1.Tokens.First(z => z is ListTokenItem) as ListTokenItem).List.Tokens.ToArray();
            var l2 = (list1.Tokens.Where(z => z is ListTokenItem).Skip(1).First() as ListTokenItem).List.Tokens.ToArray();
            var z1 = l1.Where(z => z is StringTokenItem).ToArray();
            //Degree = z1.Select(z => z as StringTokenItem).Where(z => z.Token.All(char.IsDigit)).Select(z => int.Parse(z.Token)).ToArray();
            //degree==multiplicities
            var aa = l2.Select(z => z as StringTokenItem).Where(z => z.Token.Any(char.IsDigit)).Select(u => double.Parse(u.Token.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
         //  Param1 = aa[0];
         //   Param2 = aa[1];
            //knots=[param1, param2]
        }

    }
}
