using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LiteCAD
{
    public partial class Form1 : Form
    {
        GLControl glControl;
        private EventWrapperGlControl evwrapper;
        Camera camera1 = new Camera() { IsOrtho = true };
        public CameraViewManager ViewManager;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            //if (!loaded)
            //  return;
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }


            Redraw();

        }
        void Redraw()
        {
            ViewManager.Update();

            GL.ClearColor(Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);



            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, 1, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);



            GL.Enable(EnableCap.DepthTest);

            float zz = -500;
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.LightBlue);
            GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Color3(Color.AliceBlue);
            GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
            GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
            GL.End();

            GL.PushMatrix();
            GL.Translate(camera1.viewport[2] / 2 - 50, -camera1.viewport[3] / 2 + 50, 0);
            GL.Scale(0.5, 0.5, 0.5);

            var mtr = camera1.ViewMatrix;
            var q = mtr.ExtractRotation();
            var mtr3 = Matrix4.CreateFromQuaternion(q);
            GL.MultMatrix(ref mtr3);
            GL.LineWidth(2);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.End();

            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
            GL.PopMatrix();
            camera1.Setup(glControl);

            GL.PushMatrix();

            GL.LineWidth(2);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.End();

            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
            GL.PopMatrix();

            GL.Color3(Color.Blue);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            GL.ShadeModel(ShadingModel.Smooth);

            foreach (var item in Parts)
            {
                if (checkBox1.Checked && item is LineItem) continue;
                item.Draw();
            }
            GL.Disable(EnableCap.Lighting);


            glControl.SwapBuffers();
        }

        public Form1()
        {
            InitializeComponent();

            glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));


            if (glControl.Context.GraphicsMode.Samples == 0)
            {
                glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            }
            evwrapper = new EventWrapperGlControl(glControl);

            glControl.Paint += Gl_Paint;
            ViewManager = new DefaultCameraViewManager();
            ViewManager.Attach(evwrapper, camera1);

            panel2.Controls.Add(glControl);
            panel2.Controls.Add(infoPanel);
            infoPanel.BringToFront();
            infoPanel.Dock = DockStyle.Bottom;

            glControl.Dock = DockStyle.Fill;
            DebugHelpers.Error = (str) =>
            {
                infoPanel.Invoke((Action)(() => { infoPanel.AddError(str); }));
            };
        }

        InfoPanel infoPanel = new InfoPanel();

        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3.Zero;
            camera1.CamFrom = new Vector3(-10, 0, 0);
            camera1.CamUp = Vector3.UnitZ;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3.Zero;
            camera1.CamFrom = new Vector3(0, -10, 0);
            camera1.CamUp = Vector3.UnitZ;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3.Zero;
            camera1.CamFrom = new Vector3(0, 0, -10);
            camera1.CamUp = Vector3.UnitY;
        }

        public List<IDrawable> Parts = new List<IDrawable>();

        bool loaded = false;
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loaded = false;
                Thread th = new Thread(() =>
                {
                    try
                    {
                        var prt = Part.FromStep(ofd.FileName);
                        var fi = new FileInfo(ofd.FileName);
                        loaded = true;
                        Parts.Add(prt);
                        infoPanel.AddInfo($"model loaded succesfully: {fi.Name}");
                        treeListView1.SetObjects(Parts);
                        fitAll();
                    }
                    catch (Exception ex)
                    {
                        loaded = true;
                        DebugHelpers.Error(ex.Message);
                    }
                });
                Thread th2 = new Thread(() =>
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    while (true)
                    {
                        if (!Debugger.IsAttached && sw.Elapsed.TotalSeconds > 5)
                        {
                            th.Abort();
                            DebugHelpers.Error("load timeout");
                            break;
                        }
                        Thread.Sleep(1000);
                        if (loaded) break;
                    }
                });
                if (!Debugger.IsAttached)
                {
                    th2.IsBackground = true;
                    th2.Start();
                }
                th.IsBackground = true;
                th.Start();
            }
        }

        private void planeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaneHelper ph = new PlaneHelper() { Normal = Vector3d.UnitZ };
            Parts.Add(ph);
            ph.Name = "plane01";
            treeListView1.SetObjects(Parts);
        }

        private void treeListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (treeListView1.SelectedObject == null) return;
            var tag = treeListView1.SelectedObject as AbstractHelper;
            propertyGrid1.SelectedObject = tag;

            if (tag is IEditFieldsContainer c)
            {
                var objs = c.GetObjects();
                listView1.Items.Clear();
                foreach (var item in objs)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            propertyGrid1.SelectedObject = listView1.SelectedItems[0].Tag;
        }

        public void FitToPoints(Vector3d[] pnts, Camera cam)
        {
            List<Vector2d> vv = new List<Vector2d>();
            foreach (var vertex in pnts)
            {
                var p = MouseRay.Project(vertex.ToVector3(), cam.ProjectionMatrix, cam.ViewMatrix, cam.WorldMatrix, camera1.viewport);
                vv.Add(p.Xy.ToVector2d());
            }

            //prjs->xy coords
            var minx = vv.Min(z => z.X);
            var maxx = vv.Max(z => z.X);
            var miny = vv.Min(z => z.Y);
            var maxy = vv.Max(z => z.Y);

            var dx = (maxx - minx);
            var dy = (maxy - miny);

            var cx = dx / 2;
            var cy = dy / 2;
            var dir = cam.CamTo - cam.CamFrom;
            //center back to 3d

            var mr = new MouseRay((float)(cx + minx), (float)(cy + miny), cam);
            var v0 = mr.Start;

            cam.CamFrom = v0;
            cam.CamTo = cam.CamFrom + dir;

            var aspect = glControl.Width / (float)(glControl.Height);

            dx /= glControl.Width;
            dx *= camera1.OrthoWidth;
            dy /= glControl.Height;
            dy *= camera1.OrthoWidth;

            cam.OrthoWidth = (float)Math.Max(dx, dy);
        }

        void fitAll()
        {
            List<Vector3d> vv = new List<Vector3d>();
            foreach (var item in Parts)
            {
                if (!(item is Part p)) continue;
                var nn = p.Nodes.SelectMany(z => z.Triangles.SelectMany(u => u.Vertices.Select(zz => zz.Position))).ToArray();
                vv.AddRange(nn);

            }
            if (vv.Count == 0) return;
            FitToPoints(vv.ToArray(), camera1);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fitAll();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeListView1.SelectedObjects.Count <= 0) return;
            if (Helpers.ShowQuestion($"Are you sure to delete {treeListView1.SelectedObjects.Count} items?", Text) != DialogResult.Yes) return;
            foreach (var item in treeListView1.SelectedObjects)
            {
                Parts.Remove(item as IDrawable);
            }
            treeListView1.SetObjects(Parts);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var item in Parts.OfType<Part>())
            {
                item.ShowNormals = checkBox2.Checked;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!(treeListView1.SelectedObject is Part pp)) return;            
            pp.FixNormals();
        }
    }
}