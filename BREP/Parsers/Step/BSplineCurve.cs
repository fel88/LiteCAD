using OpenTK;
using System.Globalization;
using System.Linq;

namespace BREP.Parsers.Step
{
    public class BSplineCurve : Curve
    {
        public Vector3d[] Poles;
        public int Degree;

        internal void Parse(StepParseContext ctx, TokenList list1)
        {
            Degree = int.Parse((list1.Tokens.First(z => z is StringTokenItem ss && ss.Token.All(char.IsDigit)) as StringTokenItem).Token);
            var l1 = (list1.Tokens.First(z => z is ListTokenItem) as ListTokenItem).List.Tokens.ToArray();
            var a1 = l1.Where(z => z is StringTokenItem).Select(z => z as StringTokenItem).Where(z => z.Token.StartsWith("#")).ToArray();

            var zz = a1.Select(z => ctx.GetRefObj(int.Parse(z.Token.TrimStart('#')))).ToArray();
            Poles = zz.Select(z => (Vector3d)z).ToArray();

        }
    }
}
