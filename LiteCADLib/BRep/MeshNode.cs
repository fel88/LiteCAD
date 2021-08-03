using LiteCAD.BRep.Surfaces;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace LiteCAD.BRep
{
    public class MeshNode
    {

        public bool Visible { get; set; } = true;
        public BRepFace Parent;
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();

        public virtual void SwitchNormal()
        {
            if (!(Parent.Surface is BRepPlane pl)) return;

            foreach (var item in Triangles)
            {
                foreach (var vv in item.Vertices)
                {
                    vv.Normal *= -1;
                }
            }
        }

        
    }
}