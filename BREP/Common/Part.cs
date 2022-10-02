﻿using BREP.BRep;
using BREP.BRep.Faces;
using BREP.BRep.Surfaces;
using BREP.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD.Common
{
    public class Part : IPartContainer, IMesh, IMeshNodesContainer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        Part IPartContainer.Part => this;

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
            //cylinder priority?
            //var cyls = Faces.OfType<BRepCylinderSurfaceFace>().ToArray();
            //  var faces = cyls.Union(Faces.Except(cyls)).ToArray();
            var faces = Faces.ToArray();
            for (int i = 0; i < faces.Length; i++)
            {
                BRepFace item = faces[i];
                float prog = (i / (float)faces.Length) * 100;
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
                

        public static bool AutoExtractMeshOnLoad = true;

        public IEnumerable<Vector3d> GetPoints()
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
            double eps1 = 1e-3;
            foreach (var item in Faces)
            {
                if (item.Surface is BRepPlane pl)
                {
                    int? sign = null;
                    bool good = true;
                    foreach (var pp in GetPoints())
                    {
                        var dot = Vector3d.Dot(pl.Normal, pp - pl.Location);
                        if (Math.Abs(dot) < eps1) continue;
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
                    if (rr is BRepLinearExtrusionFace) { calculated.Add(rr); break; }
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
               

        
    }
    public interface IMesh
    {
        IEnumerable<Vector3d> GetPoints();
    }
    public interface IMeshNodesContainer
    {
        MeshNode[] Nodes { get; }
    }
    
    public interface IPartContainer
    {
        int Id { get; }
        Part Part { get; }

    }
}