using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class RationalBSplineSurface : BSplineSurface
    {
        public double[] Weights;
        public void Parse(TokenList list)
        {
            var l1 = (list.Tokens.First(z => z is ListTokenItem) as ListTokenItem).List.Tokens.ToArray();
            var z1 = l1.Where(z => z is StringTokenItem).ToArray();

            Weights = z1.Select(z => z as StringTokenItem).Where(z => z.Token.Any(char.IsDigit)).Select(u => double.Parse(u.Token.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
        }
    }
    public class RationalBSplineCurve : Curve
    {
        public double[] Weights;
        public void Parse(TokenList list)
        {
            var l1 = (list.Tokens.First(z => z is ListTokenItem) as ListTokenItem).List.Tokens.ToArray();
            var z1 = l1.Where(z => z is StringTokenItem).ToArray();

            Weights = z1.Select(z => z as StringTokenItem).Where(z => z.Token.Any(char.IsDigit)).Select(u => double.Parse(u.Token.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
