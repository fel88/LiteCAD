using LiteCAD.BRep.Surfaces;
using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD.BRep
{
    public class MeshNode
    {
        public bool Visible { get; set; } = true;
        public BRepFace Parent;
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();

        public bool Contains(TriangleInfo tr)
        {
            return Triangles.Any(z => z.IsSame(tr));
        }

        public virtual void SwitchNormal()
        {
            //if (!(Parent.Surface is BRepPlane pl)) return;

            foreach (var item in Triangles)
            {
                foreach (var vv in item.Vertices)
                {
                    vv.Normal *= -1;
                }
            }
        }
        public Line3D[] SplitByPlane(Matrix4d matrix, PlaneHelper pl)
        {
            List<Line3D> vv = new List<Line3D>();
            List<TriangleInfo> trianglesModifed = new List<TriangleInfo>();
            foreach (var item in Triangles)
            {
                trianglesModifed.Add(item.Multiply(matrix));
            }
            foreach (var item in trianglesModifed)
            {
                vv.AddRange(item.SplitByPlane(pl));
            }
            return vv.ToArray();
        }

        public MeshNode RestoreXml(XElement mesh)
        {
            MeshNode ret = new MeshNode();
            foreach (var tr in mesh.Elements())
            {
                TriangleInfo tt = new TriangleInfo();
                tt.RestoreXml(tr);
                ret.Triangles.Add(tt);
            }
            return ret;
        }

        public void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<mesh>");
            foreach (var item in Triangles)
            {
                item.StoreXml(writer);
            }
            writer.WriteLine("</mesh>");
        }

        public bool Contains(TriangleInfo target, Matrix4d mtr1)
        {
            return Triangles.Any(z => z.Multiply(mtr1).IsSame(target));
        }
    }
}