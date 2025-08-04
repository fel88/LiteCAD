using LiteCAD.Common;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Linq;

namespace LiteCAD
{
    public class ObjFile : MeshModel
    {
        public static ObjFile Parse(string data)
        {
            ObjFile ret = new ObjFile();
            var lines = data.Split(['\r', '\n'], System.StringSplitOptions.RemoveEmptyEntries).ToArray();
            foreach (var line in lines)
            {
                var spl = line.Split([' '], StringSplitOptions.RemoveEmptyEntries).ToArray();
                if (spl[0] == "v")
                {
                    var vals = spl.Skip(1).Select(z => z.ToDouble()).ToArray();
                    ret.Points.Add(new Vector3d(vals[0], vals[1], vals[2]));

                }
                if (spl[0] == "vn")
                {
                    var vals = spl.Skip(1).Select(z => z.ToDouble()).ToArray();
                    ret.Normals.Add(new Vector3d(vals[0], vals[1], vals[2]));

                }
                if (spl[0] == "f")
                {
                    var vals = spl.Skip(1).Select(int.Parse).ToArray();
                    ret.Faces.Add(new Face(ret, vals.Select(z => z - 1).ToArray()));

                }
            }
            return ret;
        }

    }
}