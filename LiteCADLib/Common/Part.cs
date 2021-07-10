using IxMilia.Step;
using IxMilia.Step.Items;
using LiteCAD.BRep;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LiteCAD.Common
{
    public class Part : IDrawable
    {
        public string Name { get; set; }

        public List<BRepFace> Faces = new List<BRepFace>();
        public List<MeshNode> Nodes = new List<MeshNode>();
        public void ExtractMesh()
        {
            foreach (var item in Faces)
            {
                try
                {
                    var nd = item.ExtractMesh();
                    if (nd != null)
                    {
                        Nodes.Add(nd);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public bool Visible { get; set; } = true;
        public static Part FromStep(string fileName)
        {
            Part ret = new Part() { Name = new FileInfo(fileName).Name };

            //------------------------------------------------------------ read from a file
            StepFile stepFile;
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                stepFile = StepFile.Load(fs);
            }

            foreach (StepRepresentationItem item in stepFile.Items)
            {
                switch (item.ItemType)
                {
                    case StepItemType.AdvancedFace:
                        {
                            StepAdvancedFace face = (StepAdvancedFace)item;
                            var geom = face.FaceGeometry;
                            if (geom is StepCylindricalSurface cyl)
                            {
                                var rad = cyl.Radius;
                                foreach (var bitem in face.Bounds)
                                {
                                    var loop = bitem.Bound as StepEdgeLoop;
                                    foreach (var litem in loop.EdgeList)
                                    {

                                        StepEdgeCurve crv = litem.EdgeElement as StepEdgeCurve;
                                        if (crv.EdgeGeometry is StepCircle circ)
                                        {

                                        }
                                        else if (crv.EdgeGeometry is StepLine lin)
                                        {
                                            /*var c = crv.EdgeStart as StepVertexPoint;
                                            var c0 = c.Location;
                                            var c1 = crv.EdgeEnd as StepVertexPoint;
                                            var c01 = c1.Location;

                                            Items.Add(new LineItem()
                                            {
                                                Start = new Vector3d(
                                                    c0.X, c0.Y, c0.Z
                                                ),
                                                End = new Vector3d(
                                                    c01.X, c01.Y, c01.Z
                                                )
                                            });*/
                                        }
                                        else if (crv.EdgeGeometry is StepCurveSurface csurf)
                                        {

                                        }
                                        else
                                        {

                                        }

                                    }
                                }
                            }
                            else if (geom is StepPlane pl)
                            {
                                var pface = new BRepFace() { };
                                var loc = pl.Position.Location;
                                var loc2 = new Vector3d(loc.X, loc.Y, loc.Z);
                                var nrm = pl.Position.Axis;
                                var nrm2 = new Vector3d(nrm.X, nrm.Y, nrm.Z);
                                pface.Surface = new BRepPlane()
                                {
                                    Location = loc2,
                                    Normal = nrm2
                                };
                                ret.Faces.Add(pface);
                                foreach (var bitem in face.Bounds)
                                {
                                    var loop = bitem.Bound as StepEdgeLoop;
                                    BRepWire wire = new BRepWire();
                                    pface.Wires.Add(wire);
                                    foreach (var litem in loop.EdgeList)
                                    {
                                        StepEdgeCurve crv = litem.EdgeElement as StepEdgeCurve;
                                        var strt = (crv.EdgeStart as StepVertexPoint).Location;
                                        var end = (crv.EdgeEnd as StepVertexPoint).Location;
                                        var start = new Vector3d(strt.X, strt.Y, strt.Z);
                                        var end1 = new Vector3d(end.X, end.Y, end.Z);
                                        if (crv.EdgeGeometry is StepCircle circ)
                                        {
                                            var rad = circ.Radius;
                                            var axis3d = circ.Position as StepAxis2Placement3D;
                                            var axis = new Vector3d(axis3d.Axis.X, axis3d.Axis.Y, axis3d.Axis.Z);
                                            var refdir = new Vector3d(axis3d.RefDirection.X, axis3d.RefDirection.Y, axis3d.RefDirection.Z);
                                            var pos = new Vector3d(circ.Position.Location.X,
                                                circ.Position.Location.Y,
                                                circ.Position.Location.Z);
                                            var dir1 = start - pos;
                                            var dir2 = end1 - pos;
                                            List<Vector3d> pnts = new List<Vector3d>();



                                            var dot = Vector3d.Dot(dir2, dir1);

                                            var ang2 = (Math.Acos(dot / dir2.Length / dir1.Length)) / Math.PI * 180f;

                                            pnts.Add(pos + dir1);
                                            for (int i = 0; i < ang2; i++)
                                            {
                                                var mtr4 = Matrix4d.CreateFromAxisAngle(axis, (float)(i * Math.PI / 180f));
                                                var res = Vector4d.Transform(new Vector4d(dir1), mtr4);
                                                //var rot = new Vector4d(dir1) * mtr4;
                                                pnts.Add(pos + res.Xyz);
                                            }
                                            pnts.Add(pos + dir2);
                                            for (int j = 1; j < pnts.Count; j++)
                                            {
                                                var p0 = pnts[j - 1];
                                                var p1 = pnts[j];
                                                pface.Items.Add(new LineItem() { Start = p0, End = p1 });
                                            }

                                        }
                                        else if (crv.EdgeGeometry is StepLine lin)
                                        {
                                            BRepEdge edge = new BRepEdge();
                                            edge.Curve = new BRepLineCurve() { };
                                            wire.Edges.Add(edge);

                                            var vec = new Vector3d(lin.Vector.Direction.X,
                                                lin.Vector.Direction.Y,
                                                lin.Vector.Direction.Z);
                                            edge.Start = start;
                                            edge.End = end1;
                                            pface.Items.Add(new LineItem()
                                            {
                                                Start
                                                                                           //= new Vector3d(lin.Point.X, lin.Point.Y, lin.Point.Z)
                                                                                           = start
                                                                                           ,
                                                End = end1
                                            });
                                        }
                                        else if (crv.EdgeGeometry is StepCurveSurface csurf)
                                        {
                                            if (csurf.EdgeGeometry is StepLine ln)
                                            {
                                                BRepEdge edge = new BRepEdge();
                                                edge.Curve = new BRepLineCurve() { };
                                                wire.Edges.Add(edge);

                                                edge.Start = start;
                                                edge.End = end1;

                                                pface.Items.Add(new LineItem()
                                                {
                                                    Start = start,
                                                    End = end1
                                                });
                                            }
                                            else if (csurf.EdgeGeometry is StepCircle circ2)
                                            {
                                                BRepEdge edge = new BRepEdge();

                                                wire.Edges.Add(edge);

                                                var rad = circ2.Radius;
                                                var cc = new BRepCircleCurve();
                                                edge.Curve = cc;
                                                cc.Radius = rad;

                                                edge.Start = start;
                                                edge.End = end1;
                                                

                                                var axis3d = circ2.Position as StepAxis2Placement3D;
                                                var axis = new Vector3d(axis3d.Axis.X, axis3d.Axis.Y, axis3d.Axis.Z);
                                                var refdir = new Vector3d(axis3d.RefDirection.X, axis3d.RefDirection.Y, axis3d.RefDirection.Z);
                                                var pos = new Vector3d(circ2.Position.Location.X,
                                                    circ2.Position.Location.Y,
                                                    circ2.Position.Location.Z);
                                                var dir1 = start - pos;
                                                var dir2 = end1 - pos;
                                                List<Vector3d> pnts = new List<Vector3d>();

                                                var crs = Vector3d.Cross(dir2, dir1);
                                                var dot = Vector3d.Dot(dir2, dir1);

                                                var ang2 = (Math.Acos(dot / dir2.Length / dir1.Length)) / Math.PI * 180f;

                                                pnts.Add(pos + dir1);
                                                cc.Axis = axis;
                                                cc.Dir = dir1;
                                                cc.SweepAngle = ang2;
                                                for (int i = 0; i < ang2; i++)
                                                {
                                                    var mtr4 = Matrix4d.CreateFromAxisAngle(axis, (float)(i * Math.PI / 180f));
                                                    var res = Vector4d.Transform(new Vector4d(dir1), mtr4);
                                                    //var rot = new Vector4d(dir1) * mtr4;
                                                    pnts.Add(pos + res.Xyz);
                                                }
                                                pnts.Add(pos + dir2);
                                                for (int j = 1; j < pnts.Count; j++)
                                                {
                                                    var p0 = pnts[j - 1];
                                                    var p1 = pnts[j];
                                                    pface.Items.Add(new LineItem() { Start = p0, End = p1 });
                                                }
                                            }
                                            else
                                            {

                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                        break;
                }
            }
            ret.ExtractMesh();
            return ret;
        }

        public void Draw()
        {
            if (!Visible) return;
            GL.Disable(EnableCap.Lighting);
            foreach (var item in Faces)
            {
                foreach (var pitem in item.Items)
                {
                    pitem.Draw();
                }
            }
            GL.Enable(EnableCap.Lighting);

            foreach (var item in Nodes)
            {
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
    }
}