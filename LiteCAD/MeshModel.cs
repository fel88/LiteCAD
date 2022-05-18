using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class MeshModel : AbstractDrawable
    {
        public List<MeshNode> Nodes = new List<MeshNode>();
        public void RestoreXml(XElement elem)
        {
            _matrix.RestoreXml(elem.Element("transform"));
            foreach (var item in elem.Element("nodes").Elements())
            {
                MeshNode mn = new MeshNode();
                Nodes.Add(mn.RestoreXml(item));
            }
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<mesh name=\"{Name}\">");
            writer.WriteLine("<transform>");
            _matrix.StoreXml(writer);
            writer.WriteLine("</transform>");
            writer.WriteLine("<nodes>");
            foreach (var item in Nodes)
            {
                item.StoreXml(writer);
            }
            writer.WriteLine("</nodes>");
            writer.WriteLine("</mesh>");
        }

        TransformationChain _matrix = new TransformationChain();

        public MeshModel()
        {

        }
        public MeshModel(LiteCADScene liteCADScene, XElement item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;
            RestoreXml(item);
        }

        public TransformationChain Matrix { get => _matrix; set => _matrix = value; }

        public bool Wireframe { get; set; }
        public bool Fill { get; set; } = true;

        public override void Draw()
        {
            if (!Visible) return;

            GL.PushMatrix();
            Matrix4d dd = _matrix.Calc();
            GL.MultMatrix(ref dd);


            if (Fill)
            {
                foreach (var item in Nodes)
                {

                    GL.Enable(EnableCap.Lighting);

                    /*if (Selected)
                    {
                        GL.Disable(EnableCap.Lighting);
                        GL.Color3(Color.LightGreen);
                    }*/
                    GL.Begin(PrimitiveType.Triangles);

                    foreach (var zitem in item.Triangles)
                    {
                        foreach (var vv in zitem.Vertices)
                        {
                            GL.Normal3(vv.Normal);
                            GL.Vertex3(vv.Position);
                        }
                    }
                    GL.End();

                }
            }
            if (Wireframe)
            {
                foreach (var item in Nodes)
                {
                    GL.Color3(Color.Black);

                    GL.Disable(EnableCap.Lighting);

                    /*if (Selected)
                    {
                        GL.Disable(EnableCap.Lighting);
                        GL.Color3(Color.LightGreen);
                    }*/
                    GL.Begin(PrimitiveType.Lines);

                    foreach (var zitem in item.Triangles)
                    {
                        for (int i = 0; i < zitem.Vertices.Length; i++)
                        {
                            VertexInfo vv = zitem.Vertices[i];
                            VertexInfo vv1 = zitem.Vertices[(1 + i) % zitem.Vertices.Length];
                            GL.Normal3(vv.Normal);
                            GL.Vertex3(vv.Position);
                            GL.Normal3(vv1.Normal);
                            GL.Vertex3(vv1.Position);
                        }
                    }
                    GL.End();

                }
            }
            GL.PopMatrix();

        }

        internal Line3D[] SplitPyPlane(PlaneHelper ph)
        {
            var mm = Matrix.Calc();            
            var ret = Nodes.SelectMany(z => z.SplitByPlane(mm,ph)).ToArray();
            mm = Matrix4d.Identity;
            for (int i = 0; i < ret.Length; i++)
            {

                var res1 = Vector3d.Transform(ret[i].Start, mm);
                var res2 = Vector3d.Transform(ret[i].End, mm);
                ret[i].Start = res1;
                ret[i].End = res2;
            }
            return ret;
        }
    }

    public class Group : AbstractDrawable
    {        
        public Group()
        {
            Name = "group";
        }
        public override void Draw()
        {
            foreach (var item in Childs)
            {
                item.Draw();
            }
        }
    }
}