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
        DrawingContext dc = new DrawingContext();

        BRepFace face;
        public void Init(BRepFace tr)
        {
            face = tr;
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
            bmp = new Bitmap(Width, Height);
            gr = Graphics.FromImage(bmp);
            dc.gr = gr;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
            gr.DrawString("Projection mode: Cylinder", SystemFonts.DefaultFont, Brushes.Black, 0, 0);
            var ang = ((back.X / xxScale) * 180 / Math.PI) % 360;
            gr.DrawString($"X: {back.X}", SystemFonts.DefaultFont, Brushes.Black, 0, 15);
            gr.DrawString($"Y: {back.Y}", SystemFonts.DefaultFont, Brushes.Black, 0, 30);
            gr.DrawString($"Angle: {ang:N2}°", SystemFonts.DefaultFont, Brushes.Black, 0, 45);

            pictureBox1.Image = bmp;
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
    public interface IUndoItem
    {
        void Undo();
    }
    public class PointUndoItem : IUndoItem
    {
        public ProjectPolygon Polygon;
        public int PointIndex;
        public Vector2d Position;
        public void Undo()
        {
            Polygon.Points[PointIndex] = Position;
        }
    }
    public interface IDrawingContext
    {

    }
    public class DrawingContext : IDrawingContext
    {
        public Graphics gr;
        public float scale = 1;


        public float startx, starty;
        public float origsx, origsy;
        public bool isDrag = false;
        private bool isMiddleDrag = false;
        public bool MiddleDrag { get { return isMiddleDrag; } }
        public float sx, sy;
        public float zoom = 1;

        public Bitmap Bmp;

        public void UpdateDrag()
        {
            if (isDrag)
            {
                var p = PictureBox.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }
        public PointF GetCursor()
        {
            var p = PictureBox.PointToClient(Cursor.Position);
            var pn = BackTransform(p);
            return pn;
        }
        public void Init(PictureBox pb)
        {
            Init(new EventWrapperPictureBox(pb) { });
        }
        public void Init(EventWrapperPictureBox pb)
        {
            PictureBox = pb;
            pb.MouseWheelAction = PictureBox1_MouseWheel;
            pb.MouseUpAction = PictureBox1_MouseUp;
            pb.MouseDownAction = PictureBox1_MouseDown;

            pb.SizeChangedAction = Pb_SizeChanged;

            //pb.SizeChanged += Pb_SizeChanged;
            //pb.MouseWheel += PictureBox1_MouseWheel;
            //pb.MouseUp += PictureBox1_MouseUp;
            //pb.MouseDown += PictureBox1_MouseDown;
            //pb.MouseMove += PictureBox1_MouseMove;

            //Bmp = new Bitmap(pb.Control.Width, pb.Control.Height);
            //  gr = Graphics.FromImage(Bmp);
        }
        public float ZoomFactor = 1.5f;

        public virtual void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            //zoom *= Math.Sign(e.Delta) * 1.3f;
            //zoom += Math.Sign(e.Delta) * 0.31f;

            var pos = PictureBox.Control.PointToClient(Cursor.Position);
            if (!PictureBox.Control.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {
                return;
            }

            float zold = zoom;

            if (e.Delta > 0) { zoom *= ZoomFactor; } else { zoom /= ZoomFactor; }

            if (zoom < 0.0008) { zoom = 0.0008f; }
            if (zoom > 10000) { zoom = 10000f; }

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }

        public virtual void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == MouseButtons.Right)
            {
                isDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
            if (e.Button == MouseButtons.Middle)
            {
                isMiddleDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }

            var tt = BackTransform(e.X, e.Y);
            MouseDown?.Invoke(tt.X, tt.Y, e.Button);
        }

        internal void ResetView()
        {
            zoom = 1;
            sx = 0;
            sy = 0;
        }

        public virtual void Pb_SizeChanged(object sender, EventArgs e)
        {
            Bmp = new Bitmap(PictureBox.Control.Width, PictureBox.Control.Height);
            gr = Graphics.FromImage(Bmp);
        }
        public virtual void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
            isMiddleDrag = false;

            var tt = BackTransform(e.X, e.Y);
            MouseUp?.Invoke(tt.X, tt.Y, e.Button);
        }

        public event Action<float, float, MouseButtons> MouseUp;
        public event Action<float, float, MouseButtons> MouseDown;
        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);
        }
        public virtual PointF Transform(float x, float y)
        {
            return new PointF((x + sx) * zoom, -(y + sy) * zoom);
        }
        public virtual PointF Transform(double x, double y)
        {
            return new PointF((float)((x + sx) * zoom), (float)(-(y + sy) * zoom));
        }
        public virtual PointF Transform(Vector2d p1)
        {
            return new PointF((float)((p1.X + sx) * zoom), (float)(-(p1.Y + sy) * zoom));
        }

        public virtual PointF BackTransform(PointF p1)
        {
            var posx = (p1.X / zoom - sx);
            var posy = (-p1.Y / zoom - sy);
            return new PointF(posx, posy);
        }
        public virtual PointF BackTransform(float x, float y)
        {
            var posx = (x / zoom - sx);
            var posy = (-y / zoom - sy);
            return new PointF(posx, posy);
        }
        public EventWrapperPictureBox PictureBox;

        internal void FillEllipse(Brush black, float v1, float v2, float v3, float v4)
        {
            var pp = Transform(new PointF(v1, v2));
            gr.FillEllipse(black, pp.X, pp.Y, v3 * scale, v4 * scale);
        }

        internal void DrawLine(Pen black, PointF point, PointF point2)
        {
            var pp = Transform(point);
            var pp2 = Transform(point2);
            gr.DrawLine(black, pp, pp2);
        }

        public void FitToPoints(PointF[] points, int gap = 0)
        {
            var maxx = points.Max(z => z.X) + gap;
            var minx = points.Min(z => z.X) - gap;
            var maxy = points.Max(z => z.Y) + gap;
            var miny = points.Min(z => z.Y) - gap;

            var w = PictureBox.Control.Width;
            var h = PictureBox.Control.Height;

            var dx = maxx - minx;
            var kx = w / dx;
            var dy = maxy - miny;
            var ky = h / dy;

            var oz = zoom;
            var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
            var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
            zoom = kx;
            if (sz1.Width > w || sz1.Height > h) zoom = ky;

            var x = dx / 2 + minx;
            var y = dy / 2 + miny;

            sx = ((w / 2f) / zoom - x);
            sy = -((h / 2f) / zoom + y);

            var test = Transform(new PointF(x, y));

        }
    }
    public class EventWrapperPictureBox
    {
        public PictureBox Control;
        public EventWrapperPictureBox(PictureBox control)
        {
            Control = control;
            control.MouseUp += WrapGlControl_MouseUp;
            control.MouseDown += Control_MouseDown;
            control.KeyDown += Control_KeyDown;
            control.MouseMove += WrapGlControl_MouseMove;
            control.MouseWheel += Control_MouseWheel;
            control.KeyUp += Control_KeyUp;
            control.SizeChanged += Control_SizeChanged;
        }

        private void Control_SizeChanged(object sender, EventArgs e)
        {
            SizeChangedAction?.Invoke(sender, e);
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            KeyUpUpAction?.Invoke(sender, e);
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheelAction?.Invoke(sender, e);

        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDownAction?.Invoke(sender, e);
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownAction?.Invoke(sender, e);
        }

        private void WrapGlControl_MouseUp(object sender, MouseEventArgs e)
        {
            MouseUpAction?.Invoke(sender, e);

        }

        private void WrapGlControl_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMoveAction?.Invoke(sender, e);

        }
        public Action<object, EventArgs> SizeChangedAction;
        public Action<object, MouseEventArgs> MouseMoveAction;
        public Action<object, MouseEventArgs> MouseUpAction;
        public Action<object, MouseEventArgs> MouseDownAction;
        public Action<object, MouseEventArgs> MouseWheelAction;
        public Action<object, KeyEventArgs> KeyUpUpAction;
        public Action<object, KeyEventArgs> KeyDownAction;

        public Bitmap Image
        {
            get { return (Bitmap)Control.Image; }
            set { Control.Image = value; }
        }

        public Point PointToClient(Point position)
        {
            return Control.PointToClient(position);
        }
    }
}
