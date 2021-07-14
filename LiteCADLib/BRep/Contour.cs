using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.BRep
{
    public class Contour
    {
        public BRepWire Wire;
        public List<Segment> Elements = new List<Segment>();

        public Vector2d Start
        {
            get
            {
                return Elements[0].Start;
            }
        }

        public Vector2d End
        {
            get
            {
                return Elements.Last().End;
            }
        }

        public Contour ConnectNext(Contour[] cntr)
        {
            if (Elements.Count == 0)
            {
                Wire = cntr[0].Wire;
                Elements.AddRange(cntr[0].Elements);
                return cntr[0];
            }

            var start = new Vector2d(Elements[0].Start.X, Elements[0].Start.Y);
            var end = new Vector2d(Elements.Last().End.X, Elements.Last().End.Y);
            float tol = 10e-6f;
            double mindist = double.MaxValue;
            Contour minsegm = null;
            bool reverse = false;
            foreach (var item in cntr)
            {
                var dist1 = Math.Abs((end - item.Start).Length);
                if (dist1 < mindist)
                {
                    mindist = dist1;
                    minsegm = item;
                    reverse = false;
                }
                var dist2 = Math.Abs((end - item.End).Length);

                if (dist2 < mindist)
                {
                    mindist = dist2;
                    minsegm = item;
                    reverse = true;
                }
            }
            if (minsegm != null)
            {
                var item = minsegm;
                if (reverse)
                {
                    item = new Contour() { /*End = minsegm.Start, Start = minsegm.End */};
                    foreach (var ritem in minsegm.Elements.ToArray().Reverse())
                    {
                        item.Elements.Add(new Segment() { Start = ritem.End, End = ritem.Start }); ;
                    }

                }

                Elements.AddRange(item.Elements);
                return minsegm;
            }

            return null;
        }


        public Segment ConnectNext(Segment[] segments)
        {
            if (Elements.Count == 0)
            {
                Elements.Add(segments[0]);
                return segments[0];
            }
            var start = new Vector2d(Elements[0].Start.X, Elements[0].Start.Y);
            var end = new Vector2d(Elements.Last().End.X, Elements.Last().End.Y);
            float tol = 10e-6f;
            double mindist = double.MaxValue;
            Segment minsegm = null;
            bool reverse = false;
            foreach (var item in segments)
            {
                var dist1 = Math.Abs((end - item.Start).Length);
                if (dist1 < mindist)
                {
                    mindist = dist1;
                    minsegm = item;
                    reverse = false;
                }
                var dist2 = Math.Abs((end - item.End).Length);

                if (dist2 < mindist)
                {
                    mindist = dist2;
                    minsegm = item;
                    reverse = true;
                }
            }
            if (minsegm != null)
            {
                var item = minsegm;
                if (reverse)
                {
                    item = new Segment() { End = minsegm.Start, Start = minsegm.End };
                }
                Elements.Add(item);
                return minsegm;
            }

            return null;
        }

        internal void Reduce(double eps = 1e-8)
        {
            Elements.RemoveAll(x => x.Length() < eps);
        }
    }
}