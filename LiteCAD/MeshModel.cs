using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace LiteCAD
{
    public class MeshModel : AbstractDrawable
    {
        public List<MeshNode> Nodes = new List<MeshNode>();

        public override void Store(TextWriter writer)
        {
            writer.WriteLine("<mesh>");
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


    }

    public class TransformationChain
    {
        public void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<transformationChain>");
            foreach (var item in Items)
            {
                item.StoreXml(writer);
            }
            writer.WriteLine("</transformationChain>");
        }
        public List<TransformationChainItem> Items = new List<TransformationChainItem>();
        public Matrix4d Calc()
        {
            var r = Matrix4d.Identity;
            foreach (var item in Items)
            {
                r *= item.Matrix();
            }
            return r;
        }
    }

    public abstract class TransformationChainItem
    {
        public abstract Matrix4d Matrix();

        internal abstract void StoreXml(TextWriter writer);
    }

    public class TranslateTransformChainItem : TransformationChainItem
    {
        public Vector3d Vector;
        public override Matrix4d Matrix()
        {
            return Matrix4d.CreateTranslation(Vector);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<translate vec=\"{Vector.X};{Vector.Y};{Vector.Z}\"/>");
        }
    }
    public class ScaleTransformChainItem : TransformationChainItem
    {
        public Vector3d Vector;
        public override Matrix4d Matrix()
        {
            return Matrix4d.Scale(Vector);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<scale vec=\"{Vector.X};{Vector.Y};{Vector.Z}\"/>");

        }
    }
}