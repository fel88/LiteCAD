using System;
using System.Collections.Generic;
using System.Text;

namespace LiteCAD.CSP
{
    public class CSPTask
    {
        public List<CSPVar> Vars = new List<CSPVar>();
        public List<CSPConstr> Constrs = new List<CSPConstr>();

        internal string Dump()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Vars)
            {
                sb.AppendLine("var " + item.Name);
            }

            foreach (var item in Constrs)
            {
                if (item is CSPConstrEqualTwoVars tv)
                {
                    sb.AppendLine(tv.Var1.Name + "=" + tv.Var2.Name);
                }
                if (item is CSPConstrEqualExpression expr)
                {
                    sb.AppendLine(expr.Expression);
                }
                if (item is CSPConstrEqualVarValue vv)
                {
                    sb.AppendLine(vv.Var1.Name + "=" + vv.Value);
                }
            }
            return sb.ToString();
        }
    }
}
