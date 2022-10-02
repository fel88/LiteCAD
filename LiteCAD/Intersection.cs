using BREP.BRep;
using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCAD
{
    public static class Intersection
    {
        public static IntersectInfo[] AllRayIntersect(IMeshNodesContainer mesh, MouseRay mr)
        {
            List<IntersectInfo> ret = new List<IntersectInfo>();
            foreach (var bindMesh in mesh.Nodes)
            {
                var dd = CheckIntersect(mr, bindMesh.Triangles.ToArray());

                if (dd != null)
                {
                    dd.Parent = bindMesh;
                    /*if (bindMesh.Tag != null)
                    {
                        dd.Parent = bindMesh.Tag;
                    }*/
                    var dist = dd.Distance;
                    //dd.Model = bindMesh.Parent.Parent.Parent;
                    ret.Add(dd);
                }
            }


            return ret.ToArray();
        }
        public static IntersectInfo RayIntersect(MeshNode[] array, MouseRay mr)
        {
            var direction = mr.End - mr.Start;
            //object minObj = null;
            IntersectInfo mininfo = null;
            double mdist = double.MaxValue;

            foreach (var bindMesh in array)
            {

                var dd = CheckIntersect(mr, bindMesh.Triangles.ToArray());

                if (dd != null)
                {
                    dd.Parent = bindMesh;
                    /*if (bindMesh.Tag != null)
                    {
                        dd.Parent = bindMesh.Tag;
                    }*/
                    var dist = dd.Distance;
                    if (dist < mdist)
                    {
                        mininfo = dd;
                        mdist = dist;

                        // minObj = bindMesh;
                    }
                }
            }


            if (mdist > 10e4)
            {
                //  selectedMesh = null;
                //   selectedEntity = null;
                //  textBox1.Text = "";
                return null;
            }

            return mininfo;

        }

        public static IntersectInfo CheckIntersect(MouseRay ray, TriangleInfo[] poly)
        {
            List<IntersectInfo> intersects = new List<IntersectInfo>();
            if (poly != null)
            {
                foreach (var polygon in poly)
                {

                    var vv = polygon.Vertices.Select(z => z.Position).ToArray();
                    var res = CheckIntersect(ray, vv);
                    if (res.HasValue)
                    {
                        intersects.Add(new IntersectInfo()
                        {
                            Distance = (res.Value.ToVector3() - ray.Start).Length,
                            Target = polygon,
                            Point = res.Value
                        });
                    }
                }
            }
            if (intersects.Any())
            {
                return intersects.OrderBy(z => z.Distance).First();
            }
            return null;
        }
        public static OpenTK.Vector3d? InstersectPlaneWithRay(PlaneDataTransferObject plane, MouseRay ray)
        {

            //OpenTK.Vector3d l = ray.End - ray.Start;
            var l = ray.End - ray.Start;
            l.Normalize();

            //check point exists 
            var n = plane.Normal;
            var d = l.X * n.X + l.Y * n.Y + n.Z * l.Z;
            var r0n = ray.Start.X * n.X + ray.Start.Y * n.Y + ray.Start.Z * n.Z;
            if (Math.Abs(d) > 1e-4)
            {
                var t0 = -((r0n) + plane.W) / d;
                if (t0 >= 0)
                {
                    return (ray.Start + l * (float)t0).ToVector3d();
                }
            }
            return null;
        }


        public static OpenTK.Vector3d? CheckIntersect(MouseRay ray, OpenTK.Vector3d[] triangle)
        {
            //calc parametric equation.
            var dir = ray.End - ray.Start;
            dir.Normalize();
            //get plane of trianlge 
            var a = triangle[0];
            var b = triangle[1];
            var c = triangle[2];
            var plane = PlaneDataTransferObject.FromPoints(new Vector3d(a.X, a.Y, a.Z),
                new Vector3d(b.X, b.Y, b.Z),
                new Vector3d(c.X, c.Y, c.Z));


            plane.W = -plane.W;
            var s = InstersectPlaneWithRay(plane, ray);

            if (s != null)
            {
                //check if point inside triangle!!! raycast
                //triple cross same sign check
                var ss = s.Value;
                var v1 = triangle[1] - triangle[0];
                var v2 = triangle[2] - triangle[1];
                var v3 = triangle[0] - triangle[2];
                var crs1 = OpenTK.Vector3d.Cross(ss - triangle[0], v1);
                var crs2 = OpenTK.Vector3d.Cross(ss - triangle[1], v2);
                var crs3 = OpenTK.Vector3d.Cross(ss - triangle[2], v3);
                var up = OpenTK.Vector3d.Cross(v1, triangle[2] - triangle[0]);
                //find dot
                var dot1 = OpenTK.Vector3d.Dot(crs1, up);
                var dot2 = OpenTK.Vector3d.Dot(crs2, up);
                var dot3 = OpenTK.Vector3d.Dot(crs3, up);
                //if (Math.Sign(crs1.Z) == Math.Sign(crs2.Z) && Math.Sign(crs2.Z) == Math.Sign(crs3.Z) && Math.Sign(crs2.Z) == -1)
                if (Math.Sign(dot1) == Math.Sign(dot2) && Math.Sign(dot2) == Math.Sign(dot3) /*&& Math.Sign(dot1) == 1*/)
                {
                    //calc dist s to ray.start
                    return (ss);
                }
            }
            return null;


        }

    }

}