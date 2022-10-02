using BREP.BRep;
using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LiteCAD
{
    public static class STLLoader
    {
        public static MeshModel LoadFromFile(string path)
        {
            MeshModel mm = null;
            mm = new MeshModel() { Name = Path.GetFileNameWithoutExtension(path) };
            MeshNode node = new MeshNode();
            mm.Nodes.Add(node);
            var txt = File.ReadAllLines(path);
            if (txt[0].StartsWith("solid"))
            {
                //text format
                TriangleInfo tr = null;
                Vector3d normal = Vector3d.Zero;

                List<VertexInfo> verts = new List<VertexInfo>();
                for (int i = 1; i < txt.Length; i++)
                {
                    var line = txt[i].Trim().ToLower();
                    if (line.StartsWith("facet"))
                    {
                        var spl = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        var db = spl.Skip(2).Select(z => Helpers.ParseDouble(z)).ToArray();
                        normal = new Vector3d(db[0], db[1], db[2]);
                        tr = new TriangleInfo();
                        node.Triangles.Add(tr);
                    }
                    if (line.StartsWith("endfacet"))
                    {
                        tr.Vertices = verts.ToArray();
                        verts.Clear();
                    }
                    if (line.StartsWith("vertex"))
                    {
                        var spl = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        var db = spl.Skip(1).Select(z => Helpers.ParseDouble(z)).ToArray();
                        verts.Add(new VertexInfo()
                        {
                            Normal = normal,
                            Position = new Vector3d(db[0], db[1], db[2])
                        });

                    }
                }
            }
            else
            {
                var bts = File.ReadAllBytes(path);
                var cnt = BitConverter.ToInt32(bts, 80);




                for (int i = 0; i < cnt; i++)
                {
                    TriangleInfo tr = new TriangleInfo();
                    node.Triangles.Add(tr);

                    tr.Vertices = new VertexInfo[3];
                    Vector3d normal = new Vector3d();
                    Vector3d v1 = new Vector3d();
                    Vector3d v2 = new Vector3d();
                    Vector3d v3 = new Vector3d();

                    for (int j = 0; j < 3; j++)
                    {
                        var fl = BitConverter.ToSingle(bts, 84 + j * 4 + i * 50);
                        normal[j] = fl;
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        var fl = BitConverter.ToSingle(bts, 84 + 12 + j * 4 + i * 50);
                        v1[j] = fl;
                    }

                    for (int j = 0; j < 3; j++)
                    {
                        var fl = BitConverter.ToSingle(bts, 84 + 24 + j * 4 + i * 50);
                        v2[j] = fl;
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        var fl = BitConverter.ToSingle(bts, 84 + 36 + j * 4 + i * 50);
                        v3[j] = fl;
                    }
                    tr.Vertices[0] = new VertexInfo() { Position = v1, Normal = normal };
                    tr.Vertices[1] = new VertexInfo() { Position = v2, Normal = normal };
                    tr.Vertices[2] = new VertexInfo() { Position = v3, Normal = normal };


                }
            }
            return mm;
        }
    }
}