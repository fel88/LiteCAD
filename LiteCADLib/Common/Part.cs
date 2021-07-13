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
                    DebugHelpers.Error($"mesh extract erroe #{item.Id}: {ex.Message}");

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
                                var pface = new BRepCylinderSurfaceFace();
                                var loc = cyl.Position.Location;
                                var loc2 = new Vector3d(loc.X, loc.Y, loc.Z);
                                var nrm = cyl.Position.Axis;
                                var nrm2 = new Vector3d(nrm.X, nrm.Y, nrm.Z);
                                pface.Surface = new BRepCylinder()
                                {
                                    Location = loc2,
                                    Radius = cyl.Radius,
                                    Axis = nrm2
                                };
                                ret.Faces.Add(pface);

                                var rad = cyl.Radius;
                                foreach (var bitem in face.Bounds)
                                {
                                    BRepWire wire = new BRepWire();
                                    pface.Wires.Add(wire);
                                    var loop = bitem.Bound as StepEdgeLoop;
                                    foreach (var litem in loop.EdgeList)
                                    {
                                        StepEdgeCurve crv = litem.EdgeElement as StepEdgeCurve;

                                        var strt = (crv.EdgeStart as StepVertexPoint).Location;
                                        var end = (crv.EdgeEnd as StepVertexPoint).Location;
                                        var start = new Vector3d(strt.X, strt.Y, strt.Z);
                                        var end1 = new Vector3d(end.X, end.Y, end.Z);


                                        if (crv.EdgeGeometry is StepCircle circ)
                                        {
                                            var edge = new BRepEdge();
                                            wire.Edges.Add(edge);
                                            var pos = new Vector3d(circ.Position.Location.X,
                                                   circ.Position.Location.Y,
                                                   circ.Position.Location.Z);
                                            edge.Curve = new BRepCircleCurve() { Radius = circ.Radius, Location = pos };
                                            edge.Start = start;
                                            edge.End = end1;

                                        }
                                        else if (crv.EdgeGeometry is StepLine lin)
                                        {
                                            var edge = new BRepEdge();
                                            wire.Edges.Add(edge);

                                            edge.Curve = new BRepLineCurve() { };
                                            edge.Start = start;
                                            edge.End = end1;

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
                                            var edge = new BRepEdge();
                                            wire.Edges.Add(edge);

                                            edge.Start = start;
                                            edge.End = end1;

                                            if (csurf.EdgeGeometry is StepCircle circ2)
                                            {
                                                var loc0 = circ2.Position.Location;
                                                var loc1 = new Vector3d(loc0.X, loc0.Y, loc0.Z);
                                                edge.Curve = new BRepCircleCurve() { Location = loc1, Radius = circ2.Radius };
                                            }
                                            else if (csurf.EdgeGeometry is StepLine lin2)
                                            {
                                                edge.Curve = new BRepLineCurve() { };
                                            }
                                            else
                                            {

                                            }
                                        }
                                        else if (crv.EdgeGeometry is StepSeamCurve seam)
                                        {
                                            var edge = new BRepEdge();
                                            wire.Edges.Add(edge);
                                            edge.Curve = new BRepSeamCurve();
                                            edge.Start = start;
                                            edge.End = end1;
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
                                                cc.Location = pos;
                                                cc.SweepAngle = ang2;
                                                if ((start - end1).Length < 1e-8)
                                                {
                                                    cc.SweepAngle = Math.PI * 2;
                                                }
                                                for (int i = 0; i < ang2; i++)
                                                {
                                                    var mtr4 = Matrix4d.CreateFromAxisAngle(axis, (float)(i * Math.PI / 180f));
                                                    var res = Vector4d.Transform(new Vector4d(dir1), mtr4);
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
            ret.FixNormals();
            return ret;
        }

        IEnumerable<Vector3d> getPoints()
        {
            foreach (var item in Nodes)
            {
                foreach (var t in item.Triangles)
                {
                    foreach (var v in t.Vertices)
                    {
                        yield return v.Position;
                    }
                }
            }
        }

        public void FixNormals()
        {
            List<BRepFace> calculated = new List<BRepFace>();
            //1 phase
            foreach (var item in Faces)
            {
                if (item.Surface is BRepPlane pl)
                {
                    int? sign = null;
                    bool good = true;
                    foreach (var pp in getPoints())
                    {
                        var dot = Vector3d.Dot(pl.Normal, pp - pl.Location);
                        if (Math.Abs(dot) < 1e-8) continue;
                        if (sign == null) { sign = Math.Sign(dot); }
                        else
                        {
                            if (sign != Math.Sign(dot)) { good = false; break; }
                        }
                    }

                    if (!good) continue;

                    calculated.Add(item);
                    if (sign.HasValue && sign.Value < 0)
                    {
                        pl.Normal *= -1;
                        var nf = Nodes.FirstOrDefault(z => z.Parent == item);
                        if (nf == null) continue;
                        foreach (var tr in nf.Triangles)
                        {
                            foreach (var vv in tr.Vertices)
                            {
                                vv.Normal *= -1;
                            }
                        }
                    }
                }
                else if (item.Surface is BRepCylinder cyl)
                {
                    int? sign = null;
                    bool good = true;
                    var face = Nodes.FirstOrDefault(z => z.Parent == item);
                    if (face == null) continue;
                    var pl0 = face.Triangles[0];
                    var v0 = pl0.Vertices[1].Position - pl0.Vertices[0].Position;
                    var v1 = pl0.Vertices[2].Position - pl0.Vertices[0].Position;
                    var crs = Vector3d.Cross(v0, v1);
                    foreach (var pp in getPoints())
                    {
                        var dot = Vector3d.Dot(crs, pp - pl0.Vertices[0].Position);
                        if (Math.Abs(dot) < 1e-8) continue;
                        if (sign == null) { sign = Math.Sign(dot); }
                        else
                        {
                            if (sign != Math.Sign(dot)) { good = false; break; }
                        }
                    }

                    if (!good) continue;

                    calculated.Add(item);
                }
                else
                {

                }
            }

            //2 phase
            do
            {
                var remain = Faces.Except(calculated).ToArray();
                if (remain.Length == 0) break;
                foreach (var rr in remain)
                {
                    var edges = rr.Wires.SelectMany(z => z.Edges);
                    bool exit = false;
                    foreach (var item in calculated)
                    {
                        var edges1 = item.Wires.SelectMany(z => z.Edges);

                        foreach (var e1 in edges1)
                        {
                            foreach (var e0 in edges)
                            {
                                if (e1.IsSame(e0))
                                {
                                    var nd = Nodes.FirstOrDefault(z => z.Parent == item);
                                    if (nd == null) continue;
                                    var nm = nd.Triangles[0].Vertices[0].Normal;
                                    var nd2 = Nodes.FirstOrDefault(z => z.Parent == rr);
                                    if (nd2 == null) continue;
                                    var nm2 = nd2.Triangles[0].Vertices[0].Normal;
                                    var point0 = nd.Triangles.First(z => z.Contains(e1.Start) && z.Contains(e1.End)).Center();
                                    var point1 = nd2.Triangles.First(z => z.Contains(e1.Start) && z.Contains(e1.End)).Center();
                                    var nrm = GeometryUtils.CalcConjugateNormal(nm, point0, point1, new Segment3d() { Start = e1.Start, End = e1.End });
                                    foreach (var item1 in nd2.Triangles)
                                    {
                                        foreach (var vv in item1.Vertices)
                                        {
                                            vv.Normal = nrm;
                                        }
                                    }
                                    calculated.Add(rr);
                                    exit = true;
                                    break;
                                }
                            }
                            if (exit) break;
                        }
                        if (exit) break;
                    }
                    if (exit) break;
                }
            } while (true);
        }

        public bool ShowNormals = false;
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
            if (ShowNormals)
            {
                GL.Disable(EnableCap.Lighting);

                foreach (var item in Nodes)
                {


                    GL.Begin(PrimitiveType.Lines);
                    foreach (var tr in item.Triangles)
                    {
                        var c = tr.Center();
                        //foreach (var vv in tr.Vertices)
                        {
                            GL.Vertex3(c);
                            GL.Vertex3(c + tr.Vertices[0].Normal);
                        }
                    }
                    GL.End();
                }
            }
        }
    }


}