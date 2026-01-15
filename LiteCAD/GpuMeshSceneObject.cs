using BREP.BRep;
using BREP.Common;
using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class GpuMeshSceneObject : AbstractSceneObject, IMesh, IMeshNodesContainer
    {
        
        public void RestoreXml(XElement elem)
        {
            
        }

        public override void Store(TextWriter writer)
        {
            
        }

        protected GpuObject gpuObject;
        public GpuMeshSceneObject()
        {

        }
        public GpuMeshSceneObject(LiteCADScene liteCADScene, XElement item) : base(item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;

            RestoreXml(item);
        }

        public GpuMeshSceneObject(GpuObject gpuObject)
        {
            this.gpuObject = gpuObject;            
        }
        public IEnumerable<Vector3d> GetPoints()
        {
            var mtrx = Matrix.Calc();
            foreach (var item in Nodes)
            {
                foreach (var t in item.Triangles)
                {
                    var tt = t.Multiply(mtrx);
                    foreach (var v in tt.Vertices)
                    {
                        yield return v.Position;
                    }
                }
            }
        }
        public GpuMeshSceneObject(MeshModel model)
        {
            this.gpuObject = model.ToGpuObject();
            var node = new MeshNode() { };
            Nodes.Add(node);
            foreach (var item in model.Faces)
            {
                TriangleInfo t = new TriangleInfo();
                List<VertexInfo> vv = new List<VertexInfo>();
                foreach (var p in item.Points)
                {
                    vv.Add(new VertexInfo() { Position = p });
                }
                t.Vertices = vv.ToArray();
                node.Triangles.Add(t);
            }
        }

        public List<MeshNode> Nodes = new List<MeshNode>();
        public bool Wireframe { get; set; }
        public bool Fill { get; set; } = true;
        MeshNode[] IMeshNodesContainer.Nodes
        {
            get
            {
                var mtr = Matrix.Calc();
                List<MeshNode> ret = new List<MeshNode>();
                foreach (var item in Nodes)
                {
                    var mn = new MeshNode();
                    foreach (var titem in item.Triangles)
                    {
                        mn.Triangles.Add(titem.Multiply(mtr));
                    }
                    ret.Add(mn);
                }
                return ret.ToArray();
            }
        }

        public override void Draw(GpuDrawingContext ctx)
        {
            if (!Visible)
                return;

            GL.PushMatrix();
            if (Parent != null)
            {
                var dd2 = Parent.Matrix.Calc();
                GL.MultMatrix(ref dd2);
            }

            Matrix4d dd = _matrix.Calc();
            GL.MultMatrix(ref dd);
            ctx.SetModelShader();

            gpuObject.Draw();

            GL.UseProgram(0);
            GL.Disable(EnableCap.Lighting);

            if (Fill)
            {

               
            }
            if (Wireframe) {


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
}