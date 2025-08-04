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
    public class GpuMeshSceneObject : AbstractSceneObject
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

        public bool Wireframe { get; set; }
        public bool Fill { get; set; } = true;

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
            
            }
            GL.PopMatrix();

        }        
    }
}