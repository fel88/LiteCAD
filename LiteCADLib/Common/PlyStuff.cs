using OpenTK;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LiteCAD.Common
{
    public class PlyStuff
    {
        public static void Save(string path, Vector3d[] pnts, Vector3[] colors = null, int[] indcs = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ply");
            sb.AppendLine("format ascii 1.0");
            sb.AppendLine("comment VCGLIB generated");
            if (indcs != null)
            {
                sb.AppendLine("element vertex " + indcs.Length);
            }
            else
                sb.AppendLine("element vertex " + pnts.Length);
            sb.AppendLine("property float x");
            sb.AppendLine("property float y");
            sb.AppendLine("property float z");
            if (colors != null)
            {
                sb.AppendLine("property uchar red");
                sb.AppendLine("property uchar green");
                sb.AppendLine("property uchar blue");
            }
            sb.AppendLine("element face 0");
            sb.AppendLine("property list uchar int vertex_indices");
            sb.AppendLine("end_header");
            HashSet<int> hash = new HashSet<int>();
            if (indcs != null)
                foreach (var item in indcs)
                {
                    hash.Add(item);
                }

            for (int i = 0; i < pnts.Length; i++)
            {
                if (indcs != null)
                {
                    if (!hash.Contains(i)) continue;
                }
                Vector3d p = pnts[i];
                if (colors != null)
                {
                    Vector3 clr = colors[i];
                    sb.AppendLine((p.X + " " + p.Y + " " + p.Z + " " + (int)(clr.X * 255) + " " + (int)(clr.Y * 255) + " " + (int)(clr.Z * 255)).Replace(",", "."));
                }
                else
                {
                    sb.AppendLine((p.X + " " + p.Y + " " + p.Z).Replace(",", "."));
                }
            }
            File.WriteAllText(path, sb.ToString());
        }
    }
}