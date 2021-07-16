using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace LiteCAD
{
    public class Camera
    {

        public Vector3 CamFrom = new Vector3(250, 250, 250);
        public Vector3 CamTo = new Vector3(0, 0, 0);
        public Vector3 CamUp = new Vector3(0, 0, 1);

        public Vector3 Dir
        {
            get { return (CamFrom - CamTo).Normalized(); }
        }
        public float DirLen
        {
            get { return (CamFrom - CamTo).Length; }
        }

        public Vector3 CameraFrom
        {
            get { return CamFrom; }
        }
        public Vector3 CameraTo
        {
            get { return CamTo; }
        }
        public Vector3 CameraUp
        {
            get { return CamUp; }
        }

        public Matrix4 ProjectionMatrix { get; set; }
        public Matrix4 ViewMatrix { get; set; }
        public int[] viewport = new int[4];
        public void MoveForw(float ang)
        {

            var vect = CamFrom - CamTo;
            CamTo += new Vector3(ang, 0, 0);
            CamFrom = vect + CamTo;
        }
        public void RotateFromZ(float ang)
        {
            var vect = CamFrom - CamTo;
            var m = Matrix4.CreateFromAxisAngle(CamUp, ang);
            CamFrom = ((m * new Vector4(vect, 1)).Xyz + CamTo);
            CamUp = ((m * new Vector4(CamUp, 1)).Xyz);
        }
        public void RotateFromX(float ang)
        {
            var vect = CamFrom - CamTo;
            var m = Matrix4.CreateFromAxisAngle(Vector3.UnitX, ang);

            CamUp = ((m * new Vector4(CamUp, 1)).Xyz);
            CamFrom = ((m * new Vector4(vect, 1)).Xyz + CamTo);
        }
        public void RotateFromY(float ang)
        {
            var vect = CamFrom - CamTo;

            var cross1 = Vector3.Cross(vect, CamUp);
            var m = Matrix4.CreateFromAxisAngle(cross1, ang);
            //var m = Matrix4.CreateRotationY(ang);
            CamUp = ((m * new Vector4(CamUp, 1)).Xyz);
            CamFrom = ((m * new Vector4(vect, 1)).Xyz + CamTo);
        }

        public float zoom = 1;

        public float ZNear = -25e3f;
        public float ZFar = 25e3f;

        public bool IsOrtho { get; set; } = false;
        public float OrthoWidth { get; set; } = 1000;
        public float Fov { get; set; } = 60;

        public void UpdateMatricies(GLControl glControl)
        {

            viewport[0] = 0;
            viewport[1] = 0;
            viewport[2] = glControl.Width;
            viewport[3] = glControl.Height;
            var aspect = glControl.Width / (float)glControl.Height;
            var o = Matrix4.CreateOrthographic(OrthoWidth, OrthoWidth / aspect, ZNear, ZFar);

            Matrix4 mp = Matrix4.CreatePerspectiveFieldOfView((float)(Fov * Math.PI / 180) * zoom,
                glControl.Width / (float)glControl.Height, 1, 25e4f);


            if (IsOrtho)
            {
                ProjectionMatrix = o;

            }
            else
            {
                ProjectionMatrix = mp;

            }

            Matrix4 modelview = Matrix4.LookAt(CamFrom, CamTo, CamUp);
            ViewMatrix = modelview;
        }
        public void Setup(GLControl glControl)
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            viewport[0] = 0;
            viewport[1] = 0;
            viewport[2] = glControl.Width;
            viewport[3] = glControl.Height;
            var aspect = glControl.Width / (float)glControl.Height;
            var o = Matrix4.CreateOrthographic(OrthoWidth, OrthoWidth / aspect, ZNear, ZFar);

            Matrix4 mp = Matrix4.CreatePerspectiveFieldOfView((float)(Fov * Math.PI / 180) * zoom,
                glControl.Width / (float)glControl.Height, 1, 25e4f);

            GL.MatrixMode(MatrixMode.Projection);
            if (IsOrtho)
            {
                ProjectionMatrix = o;
                GL.LoadMatrix(ref o);
            }
            else
            {
                ProjectionMatrix = mp;
                GL.LoadMatrix(ref mp);
            }

            Matrix4 modelview = Matrix4.LookAt(CamFrom, CamTo, CamUp);
            //modelview = WorldMatrix * modelview;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            ViewMatrix = modelview;

            GL.MultMatrix(ref WorldMatrix);
        }

        public Matrix4 WorldMatrix = Matrix4.Identity;

        public void Shift(Vector3 vector3)
        {
            CamFrom += vector3;
            CamTo += vector3;
        }

        public Vector3 GetSide()
        {
            var dirr = CamFrom - CamTo;
            var forw = new Vector3(dirr.X, dirr.Y, 0);
            forw.Normalize();
            var crs = Vector3.Cross(forw, CamUp);
            var side = new Vector3(crs.X, crs.Y, 0);
            side.Normalize();
            return side;
        }
    }
}

