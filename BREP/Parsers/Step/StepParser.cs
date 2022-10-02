using BREP.Common;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BREP.Parsers.Step
{

    public static class StepParser
    {
        public static string[] Tokenize(string data)
        {
            List<string> ret = new List<string>();
            bool insideString = false;
            StringBuilder sb = new StringBuilder();
            char[] symbols = { '=', ';', '(', ')', ',' };
            for (int i = 0; i < data.Length; i++)
            {
                if (!insideString && (data[i] == '\n' || data[i] == '\r'))
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }
                if (data[i] == '\'')
                {
                    insideString = !insideString;
                    if (!insideString)
                    {
                        sb.Append('\'');
                        ret.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }
                if (!insideString)
                    if (symbols.Contains(data[i]))
                    {
                        if (sb.Length > 0)
                        {
                            ret.Add(sb.ToString());
                            sb.Clear();
                        }
                        ret.Add(data[i].ToString());
                        continue;
                    }

                sb.Append(data[i]);
            }
            return ret.Select(z => z.Trim()).Where(z => z.Length > 0).ToArray();
        }

        public static Part Parse(string filename)
        {
            var txt = File.ReadAllText(filename);
            var tkns = Tokenize(txt);

            StepParseContext ctx = new StepParseContext();

            StringBuilder accum = new StringBuilder();
            foreach (var token in tkns)
            {
                if (token == ";")
                {
                    var str = accum.ToString();
                    if (str.StartsWith("#"))
                    {
                        StepLineItem item = new StepLineItem();
                        var spl = str.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        item.Index = int.Parse(spl[0].Trim().TrimStart('#'));
                        item.Value = spl[1];
                        ctx.AddItem(item);
                    }
                    accum.Clear();
                    continue;
                }
                accum.Append(token);
            }

            ctx.ParseShells();
            var part = ctx.ToPart();
            part.Name = new FileInfo(filename).Name;
            if (Part.AutoExtractMeshOnLoad)
                part.ExtractMesh();
            try
            {
                //todo: make fix normals via topology (outter wire always has outside normals, inside wire always has inside normals) or cutting planes
            part.FixNormals();
            }
            catch (Exception ex)
            {
                DebugHelpers.Error("fix normals exception: " + ex.Message);
            }
            DebugHelpers.Progress(false, 0);
            return part;
        }
    }

    public class VertexPoint
    {
        public Vector3d Point;
    }
    public class EdgeLoop
    {
        public List<OrientedEdge> Edges = new List<OrientedEdge>();
    }

    public class FaceBound
    {
        public EdgeLoop Loop;
        public bool Orientation;
    }
    public class FaceOuterBound : FaceBound
    {

    }
}
