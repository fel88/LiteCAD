using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace LiteCAD.BRep
{
    public class BRepFace
    {
        public List<DrawItem> Items = new List<DrawItem>();
        public BRepSurface Surface;
        public List<BRepWire> Wires = new List<BRepWire>();
        public BRepWire OutterWire;

        public void ExtractMesh()
        {
            if (Surface is BRepPlane pl)
            {
                MeshItem m = new MeshItem();
                //var lns = Items.Where(z => z is LineItem).OfType<LineItem>().ToArray();
                //lns[0].
                // GeometryUtils.TriangulateWithHoles()
                // Items.Add(m);
            }
        }
    }
    public class DrawItem
    {
        public virtual void Draw() { }
    }

    public class BRepSurface
    {

    }
    public class BRepPlane : BRepSurface
    {
        public Vector3d Position;
        public Vector3d Normal;
    }

    public class LineItem : DrawItem
    {
        public override void Draw()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Start);
            GL.Vertex3(End);
            GL.End();
        }
        public Vector3d Start;
        public Vector3d End;
    }

    public class TriangleInfo
    {
        public Vector3d[] Verticies;
    }
    public class MeshItem : DrawItem
    {
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();
        public override void Draw()
        {
            GL.Begin(PrimitiveType.Triangles);
            foreach (var item in Triangles)
            {
                foreach (var vv in item.Verticies)
                {
                    GL.Vertex3(vv);
                }
            }
            GL.End();

        }
    }
    public class PlaneItem : DrawItem
    {
        public Vector3d Dir;
        public Vector3d Position;
        public override void Draw()
        {
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Position);
            GL.Vertex3(Position + Dir * 100);
            GL.End();
            GL.Color3(Color.Blue);
        }
    }
    public class PointItem : DrawItem
    {

        public Vector3d Position;
        public override void Draw()
        {
            GL.Color3(Color.Yellow);
            GL.PointSize(4);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(Position);
            GL.End();
            GL.Color3(Color.Blue);
        }
    }
    

}