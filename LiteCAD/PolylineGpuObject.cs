using FxEngine;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;

namespace LiteCAD
{
    public class PolylineGpuObject : GpuObject
    {
        public PolylineGpuObject(Vector3d[] verts, PrimitiveType? primitiveType = null) : base(verts, primitiveType ?? PrimitiveType.LineStrip) { }
    }
}