using OpenTK;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace LiteCAD.Common
{
    public static class Helpers
    {
        public static double ParseDouble(string v)
        {
            return double.Parse(v.Replace(",", "."), CultureInfo.InvariantCulture);
        }
        public static decimal ParseDecimal(string v)
        {
            return decimal.Parse(v.Replace(",", "."), CultureInfo.InvariantCulture);
        }
        
        public static Vector2 ToVector2f(this Vector2d v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }
        public static PointF ToPointF(this Vector2d v)
        {
            return new PointF((float)v.X, (float)v.Y);
        }

        public static Vector3d ParseVector(string value)
        {
            Vector3d ret = new Vector3d();
            var spl = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(ParseDouble).ToArray();
            for (int i = 0; i < 3; i++)
            {
                ret[i] = spl[i];
            }
            return ret;
        }
    }

}