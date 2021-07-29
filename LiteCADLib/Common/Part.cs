using IxMilia.Step;
using IxMilia.Step.Items;
using LiteCAD.BRep;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace LiteCAD.Common
{
    public class Part : AbstractDrawable
    {
        public string Name { get; set; }

        public List<BRepFace> Faces = new List<BRepFace>();
        public MeshNode[] Nodes
        {
            get
            {
                return Faces.Select(z => z.Node).Where(z => z != null).ToArray();
            }
        }

        public void ExtractMesh()
        {
            for (int i = 0; i < Faces.Count; i++)
            {
                BRepFace item = Faces[i];
                float prog = (i / (float)Faces.Count) * 100;
                DebugHelpers.Progress(true, prog);                
                try
                {
                    var nd = item.ExtractMesh();
                }
                catch (Exception ex)
                {
                    DebugHelpers.Error($"mesh extract error #{item.Id}: {ex.Message}");
                }
            }
            DebugHelpers.Progress(true, 100);
        }

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
                                var pface = new BRepCylinderSurfaceFace(ret);
                                pface.Load(face, geom);
                                ret.Faces.Add(pface);                                
                            }
                            else if (geom is StepPlane pl)
                            {
                                var pface = new BRepPlaneFace(ret) { };
                                pface.Load(face, pl);
                                ret.Faces.Add(pface);
                            }
                            else if (geom is StepToroidalSurface tor)
                            {
                                var pface = new BRepToroidalSurfaceFace(ret) { };
                                pface.Load(face, tor);
                                ret.Faces.Add(pface);                                
                            }
                            else if (geom is StepSurfaceOfLinearExtrusion ext)
                            {
                                var pface = new BRepLinearExtrusionFace(ret) { };
                                pface.Load(face, ext);
                                ret.Faces.Add(pface);                             
                            }
                            else
                            {
                                DebugHelpers.Warning($"unsupported surface: {geom.ToString()}");
                            }
                        }
                        break;
                }
            }
            if (AutoExtractMeshOnLoad)
                ret.ExtractMesh();
            ret.FixNormals();
            DebugHelpers.Progress(false, 0);
            return ret;
        }

        public static bool AutoExtractMeshOnLoad = true;

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
                    /*int? sign = null;
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

                    calculated.Add(item);*/
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
                int before = calculated.Count;
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

                                    var _point0 = nd.Triangles.FirstOrDefault(z => z.Contains(e1.Start) && z.Contains(e1.End));
                                    if (_point0 == null) continue;
                                    var point0 = _point0.Center();
                                    var _point1 = nd2.Triangles.FirstOrDefault(z => z.Contains(e1.Start) && z.Contains(e1.End));
                                    if (_point1 == null) continue;

                                    var point1 = _point1.Center();

                                    var nrm = GeometryUtils.CalcConjugateNormal(nm, point0, point1, new Segment3d() { Start = e1.Start, End = e1.End });
                                    if (rr is BRepCylinderSurfaceFace)
                                    {
                                        (nd2 as CylinderMeshNode).SetNormal(nd2.Triangles[0], nrm);
                                    }
                                    else
                                    {
                                        foreach (var item1 in nd2.Triangles)
                                        {
                                            foreach (var vv in item1.Vertices)
                                            {
                                                vv.Normal = nrm;
                                            }
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
                if (calculated.Count == before)
                {
                    DebugHelpers.Error("normals restore failed");
                    break;
                }
            } while (true);
        }

        public bool ShowNormals = false;
        public override void Draw()
        {
            if (!Visible) return;
            GL.Disable(EnableCap.Lighting);
            foreach (var item in Faces)
            {
                if (!item.Visible) continue;
                foreach (var pitem in item.Items)
                {
                    pitem.Draw();
                }
            }
            GL.Enable(EnableCap.Lighting);

            foreach (var item in Nodes)
            {

                if (!item.Parent.Visible) continue;
                GL.Enable(EnableCap.Lighting);

                if (item.Parent.Selected)
                {
                    GL.Disable(EnableCap.Lighting);
                    GL.Color3(Color.LightGreen);
                }
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
                    if (!item.Parent.Visible) continue;
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