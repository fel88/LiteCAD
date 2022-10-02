using BREP.Common;
using LiteCAD.Common;
using OpenTK;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BREP.BRep
{
    public class ProjectPolygon
    {
        public List<Vector2d> Points = new List<Vector2d>();

        public ProjectPolygon Clone()
        {
            ProjectPolygon ret = new ProjectPolygon();
            ret.Points = Points.ToList();
            return ret;
        }
        public void Scale(double x, double y)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new Vector2d(Points[i].X * x, Points[i].Y * y);
            }
        }
        public RectangleF? BoundingBox()
        {
            var pnt = Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();

            var maxx = pnt.Max(z => z.X);
            var maxy = pnt.Max(z => z.Y);
            var minx = pnt.Min(z => z.X);
            var miny = pnt.Min(z => z.Y);

            return new RectangleF((float)(minx), (float)(miny), maxx - minx, maxy - miny);
        }

        internal double Area()
        {
            return GeometryUtils.CalculateArea(Points.ToArray());
        }
    }
}
