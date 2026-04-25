using BREP.BRep;
using FxEngine;
using LiteCAD.Common;
using netDxf;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;

namespace LiteCAD
{
    public class PolylineGpuMeshSceneObject : AbstractSceneObject, IMesh, IMeshNodesContainer
    {

        public void RestoreXml(XElement elem)
        {

        }

        public override void Store(TextWriter writer)
        {

        }

        protected PolylineGpuObject gpuObject;
        public PolylineGpuMeshSceneObject()
        {

        }
        public PolylineGpuMeshSceneObject(LiteCADScene liteCADScene, XElement item) : base(item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;

            RestoreXml(item);
        }

        public PolylineGpuMeshSceneObject(PolylineGpuObject gpuObject)
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

        public OpenTK.Mathematics.Matrix4 ToMatrix4(Matrix4d matrix4d)
        {
            OpenTK.Mathematics.Matrix4 matrix4 = new (
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row0.X, (float)matrix4d.Row0.Y, (float)matrix4d.Row0.Z, (float)matrix4d.Row0.W),
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row1.X, (float)matrix4d.Row1.Y, (float)matrix4d.Row1.Z, (float)matrix4d.Row1.W),
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row2.X, (float)matrix4d.Row2.Y, (float)matrix4d.Row2.Z, (float)matrix4d.Row2.W),
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row3.X, (float)matrix4d.Row3.Y, (float)matrix4d.Row3.Z, (float)matrix4d.Row3.W)
            );
            return matrix4;
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
            //ctx.ModelShader.setMat4("model", ToMatrix4(dd));

            gpuObject.Draw();

            GL.UseProgram(0);
            GL.Disable(EnableCap.Lighting);

            if (Fill)
            {


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
}
