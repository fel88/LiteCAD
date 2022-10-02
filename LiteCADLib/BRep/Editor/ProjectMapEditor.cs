using BREP.BRep;
using BREP.BRep.Surfaces;
using LiteCAD.Tools;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiteCAD.BRep.Editor
{
    public partial class ProjectMapEditor : Form
    {
        public ProjectMapEditor()
        {
            InitializeComponent();
            Recreate();
            SizeChanged += Form1_SizeChanged;
            dc.Init(pictureBox1);
            dc.MouseUp += Dc_MouseUp;
            dc.MouseDown += Dc_MouseDown;

            Load += Form1_Load;
            FormClosing += ProjectMapEditor_FormClosing;


            pet = new CylinderProjectionPointEditorToolPanel();
            pet.Set += Pet_Set;
            pet.PointChanged += Pet_PointChanged;
            tp.SetControl(pet);
            tp.Left = 10;
            tp.Top = pictureBox1.Height - tp.Height - 10;
            tp.Visible = false;
            pictureBox1.Controls.Add(tp);

        }

        private void Pet_Set()
        {
            tp.Visible = false;
        }

        private void Pet_PointChanged(Vector2d obj)
        {
            sp.Points[lastEditedPointIndex] = obj;
        }

        CylinderProjectionPointEditorToolPanel pet;
        ToolPanel tp = new ToolPanel() { Title = "Point" };
        private void ProjectMapEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<ProjectPolygon> pp = new List<ProjectPolygon>();
            foreach (var item in Polygons)
            {
                var poly = new ProjectPolygon();
                pp.Add(poly);
                poly.Points = item.Points.Select(z => z).ToList();
                for (int i = 0; i < poly.Points.Count; i++)
                {
                    var t1 = poly.Points[i];
                    poly.Points[i] = new Vector2d(t1.X / xxScale, t1.Y / 100);
                }
            }
            try
            {
                face.UpdateMesh(pp.ToArray());
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        List<ProjectPolygon> polygons = new List<ProjectPolygon>();
        public ProjectPolygon[] Polygons
        {
            get
            {
                return polygons.ToArray();
            }
        }
        float boundY(float y)
        {
            return y;
            return Math.Max(Math.Min(y, 100), 0);
        }
        Vector2d origPointPosition;
        private void Dc_MouseUp(float arg1, float arg2, MouseButtons b)
        {
            if (addPolygonMode && b == MouseButtons.Left)
                newPolygonPoints.Add(new Vector2(arg1, boundY(arg2)));
            if (captured && sp != null && sindex != -1)
            {
                undos.Add(new PointUndoItem() { Polygon = sp, PointIndex = sindex, Position = origPointPosition });
            }
            captured = false;

        }
        private void Dc_MouseDown(float arg1, float arg2, MouseButtons b)
        {
            if (sindex > -1 && b == MouseButtons.Left)
            {
                captured = true;
                tp.Visible = true;
                origPointPosition = sp.Points[sindex];
                pet.SetPoint(sp.Points[sindex]);
            }
        }

        public Exception Exception;
        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;
        IDrawingContext dc = new GdiDrawingContext();

        BRepFace face;
        public void Init(BRepFace tr)
        {
            face = tr;
            if (face.Surface is BRepCylinder cyl)
            {
                Mode = ProjectionModeEnum.Cylinder;
                xxScale = cyl.Radius;
            }

            if (face.ProjectPolygons != null)
            {
                polygons = face.ProjectPolygons.Select(z => z.Clone()).ToList();
                foreach (var item in polygons)
                {
                    //item.Scale(xxScale, yyScale);
                }
            }
            fitAll();

        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            Recreate();
        }
        Bitmap bmp;
        Graphics gr;
        void Recreate()
        {
            /*bmp = new Bitmap(Width, Height);
            gr = Graphics.FromImage(bmp);
            dc.gr = gr;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;*/
        }
        double xxScale = 20;
        double yyScale = 100;
        public int sindex = -1;
        public int lastEditedPointIndex = -1;
        public ProjectPolygon sp = null;
        public void UpdateNearest()
        {

            var pos = pictureBox1.PointToClient(Cursor.Position);
            var posx = (pos.X / dc.zoom - dc.sx);
            double posy = (-pos.Y / dc.zoom - dc.sy);

            foreach (var item in polygons)
            {

                var fr = item.Points.OrderBy(z => (z - new Vector2d(posx, posy)).Length).First();
                if (!captured)
                {
                    if (((fr - new Vector2d(posx, posy)).Length) < 10 / dc.zoom)
                    {
                        sp = item;
                        sindex = item.Points.IndexOf(fr);
                        break;
                    }
                    else
                    {
                        sindex = -1;
                    }
                }
            }

            if (sindex >= 0 && captured && sp != null)
            {
                //if (posy < 0) posy = 0;
                // if (posy > yyScale) posy = yyScale;
                sp.Points[sindex] = new Vector2d(posx, posy);
                pet.SetPoint(sp.Points[sindex]);
                lastEditedPointIndex = sindex;
            }

        }
        bool captured = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            dc.UpdateDrag();
            UpdateNearest();
            gr.Clear(Color.White);

            gr.DrawLine(Pens.Red, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(500, 0)));
            gr.DrawLine(Pens.Green, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(0, 500)));

            Pen p = new Pen(Color.Gray);
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
            p.DashPattern = new float[] { 4, 2, 2, 4 };
            gr.DrawLine(p, dc.Transform(-2000, 100), dc.Transform(2000, 100));
            gr.DrawLine(p, dc.Transform(-2000, 0), dc.Transform(2000, 0));

            for (int i = -10; i < 10; i++)
            {
                var x1 = (float)(i * Math.PI * 2 * xxScale);
                gr.DrawLine(p, dc.Transform(x1, 100), dc.Transform(x1, 0));
            }

            int w1 = 4;
            int w2 = 2;
            var temp1 = new List<ProjectPolygon>();
            temp1.AddRange(polygons);
            if (addPolygonMode)
            {
                temp1.Add(new ProjectPolygon() { Points = newPolygonPoints.Select(z => new Vector2d(z.X, z.Y)).ToList() });
            }


            foreach (var polygon in temp1)
            {
                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    Vector2d item = (Vector2d)polygon.Points[i];
                    var tr = dc.Transform(item.X, item.Y);
                    gr.FillRectangle((sp == polygon && i == sindex) ? Brushes.Red : Brushes.Blue, tr.X - w1, tr.Y - w1, w1 * 2, w1 * 2);
                    gr.FillRectangle(Brushes.LightBlue, tr.X - w2, tr.Y - w2, w2 * 2, w2 * 2);
                }
                if (polygon.Points.Count > 2)
                {
                    var pp = polygon.Points.Select(z =>
                     {
                         var t = dc.Transform(z.X, z.Y);
                         return new PointF(t.X, t.Y);
                     }).ToArray();
                    gr.FillPolygon(new SolidBrush(Color.FromArgb(64, Color.LightGreen)), pp);
                    Pen p2 = new Pen(Color.Blue, 1);
                    p2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    p2.DashPattern = new float[] { 10, 5 };
                    Pen p3 = new Pen(Color.Black);
                    p3.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    p3.DashPattern = new float[] { 5, 10 };
                    gr.DrawPolygon(p2, pp);
                    //gr.DrawPolygon(p3, pp);
                }
            }


            var pos = pictureBox1.PointToClient(Cursor.Position);
            var back = dc.BackTransform(pos);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(64, Color.LightBlue)), 0, 0, 150, 60);
            gr.DrawString($"Projection mode: {Mode}", SystemFonts.DefaultFont, Brushes.Black, 0, 0);
            var ang = ((back.X / xxScale) * 180 / Math.PI) % 360;
            gr.DrawString($"X: {back.X}", SystemFonts.DefaultFont, Brushes.Black, 0, 15);
            gr.DrawString($"Y: {back.Y}", SystemFonts.DefaultFont, Brushes.Black, 0, 30);
            gr.DrawString($"Angle: {ang:N2}°", SystemFonts.DefaultFont, Brushes.Black, 0, 45);

            if (Mode == ProjectionModeEnum.Cylinder)
            {
                var cylinder = face.Surface as BRepCylinder;
                gr.DrawString($"Radius: {cylinder.Radius:N2}", SystemFonts.DefaultFont, Brushes.Black, 0, 60);
            }
            pictureBox1.Image = bmp;
        }

        public ProjectionModeEnum Mode;

        public enum ProjectionModeEnum
        {
            Cylinder, Cone
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            dc.ResetView();
        }

        bool addPolygonMode = false;
        List<Vector2> newPolygonPoints = new List<Vector2>();
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            addPolygonMode = true;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (addPolygonMode && newPolygonPoints.Any())
            {
                addPolygonMode = false;
                polygons.Add(new ProjectPolygon() { Points = newPolygonPoints.Select(z => new Vector2d(z.X, z.Y)).ToList() });
                newPolygonPoints.Clear();
            }
        }

        void fitAll()
        {
            List<PointF> pp = new List<PointF>();
            foreach (var item in polygons)
            {
                var rect = item.BoundingBox();
                if (rect == null) continue;
                pp.Add(rect.Value.Location);
                pp.Add(new PointF(rect.Value.Right, rect.Value.Bottom));
            }
            if (pp.Count == 0) return;
            dc.FitToPoints(pp.ToArray(), 5);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            fitAll();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            polygons.Clear();
        }

        List<IUndoItem> undos = new List<IUndoItem>();
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (undos.Count == 0) return;
            undos.Last().Undo();
            undos.RemoveAt(undos.Count - 1);
        }
    }
}
