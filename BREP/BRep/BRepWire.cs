using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BREP.BRep
{
    public class BRepWire
    {
        public List<BRepEdge> Edges = new List<BRepEdge>();
        public bool IsOutter = false;

                
        public bool IsClosed(double eps = 1e-8)
        {
            if (Edges.Count == 0) return true;
            BRepEdge p = Edges[0];

            for (int i = 1; i < Edges.Count; i++)
            {
                BRepEdge item = Edges[i];
                if (!((item.Start - p.Start).Length < eps || (item.Start - p.End).Length < eps || (item.End - p.Start).Length < eps || (item.End - p.End).Length < eps)) return false;
                p = item;
            }
            return true;
        }

        public BRepWire Clone()
        {
            BRepWire ret = new BRepWire();
            foreach (var item in Edges)
            {
                ret.Edges.Add(item.Clone());
            }
            return ret;
        }

        public void Transform(Matrix4d mtr4)
        {
            foreach (var item in Edges)
            {
                item.Transform(mtr4);
            }
        }
    }
}