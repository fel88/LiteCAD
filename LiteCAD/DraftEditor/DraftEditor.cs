using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using LiteCAD.Common;
using LiteCAD.Tools;
using System.Linq;
using LiteCAD.BRep.Editor;
using System.IO;
using System.Xml.Linq;

namespace LiteCAD.DraftEditor
{
    public partial class DraftEditorControl : UserControl, IDraftEditor
    {
        public DraftEditorControl()
        {
            InitializeComponent();
            ctx.Init(pictureBox1);
            ctx.InitGraphics();
            ctx.MouseDown += Ctx_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
        }

        private void Ctx_MouseDown(float arg1, float arg2, MouseButtons e)
        {
            //var pos = ctx.PictureBox.Control.PointToClient(Cursor.Position);




            if (e == MouseButtons.Left)
            {
                foreach (var item in selected)
                {
                    if (item is IDrawable dd)
                    {
                        dd.Selected = false;
                    }
                }
                selected = new[] { nearest };
                Form1.Form.SetStatus($"selected: {selected.Count()} objects");
                foreach (var item in selected)
                {
                    if (item is IDrawable dd)
                    {
                        dd.Selected = true;
                    }
                }
            }

            if (e == MouseButtons.Middle)
            {
                ctx.isMiddleDrag = true;
                if (nearest is DraftPoint pp)
                {
                    startMiddleDragNearest = nearest;
                    var tr = ctx.Transform(pp.Location);
                    ctx.startx = (float)tr.X;
                    ctx.starty = (float)tr.Y;
                }
            }
            if (e == MouseButtons.Left && editor.CurrentTool is SelectionTool)
            {
                ctx.isLeftDrag = true;
                if (nearest is DraftPoint pp)
                {
                    var tr = ctx.Transform(pp.Location);
                    ctx.startx = (float)tr.X;
                    ctx.starty = (float)tr.Y;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }
        DraftPoint lastDraftPoint = null;
        List<DraftElement> queue = new List<DraftElement>();
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (editor.CurrentTool is SelectionTool && e.Button == MouseButtons.Left)
            {
                //if (nearest is LinearConstraintHelper lh)
                {
                    //   editor.ObjectSelect(lh.constraint);
                }
                //  else
                {
                    editor.ObjectSelect(nearest);
                }
            }
            if (editor.CurrentTool is PerpendicularConstraintTool && e.Button == MouseButtons.Left)
            {
                if (nearest is DraftLine)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftLine);
                }
                if (queue.Count > 1)
                {
                    var cc = new PerpendicularConstraint(queue[0] as DraftLine, queue[1] as DraftLine);
                    if (!_draft.Constraints.OfType<PerpendicularConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new PerpendicularConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    editor.ResetTool();
                }
            }
            if (editor.CurrentTool is ParallelConstraintTool && e.Button == MouseButtons.Left)
            {
                if (nearest is DraftLine)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftLine);
                }
                if (queue.Count > 1)
                {
                    var cc = new ParallelConstraint(queue[0] as DraftLine, queue[1] as DraftLine);

                    if (!_draft.Constraints.OfType<ParallelConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new ParallelConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    editor.ResetTool();

                }
            }
            /*if (editor.CurrentTool is HorizontalConstraintTool && e.Button == MouseButtons.Left)
            {
                if (nearest is DraftLine dl)
               
                {
                    var cc = new HorizontalConstraint(dl);

                    if (!_draft.Constraints.OfType<HorizontalConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        //_draft.AddHelper(new ParallelConstraintHelper(cc));
                        //_draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    editor.ResetTool();

                }
            }*/
            if (editor.CurrentTool is AbstractTool at)
            {
                at.Editor = this;
            }
            editor.CurrentTool.MouseDown(e);

            if (editor.CurrentTool is LinearConstraintTool && e.Button == MouseButtons.Left)
            {
                if (nearest is DraftPoint)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftPoint);
                }
                if (nearest is DraftLine dl)
                {
                    if (queue.Count > 0 && queue[0] is DraftPoint dp1)
                    {
                        LinearConstraintLengthDialog lcd = new LinearConstraintLengthDialog();
                        lcd.Init(dl.Length);
                        lcd.ShowDialog();
                        var cc = new LinearConstraint(dl, dp1, lcd.Length);
                        if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                        {
                            _draft.AddConstraint(cc);
                            _draft.AddHelper(new LinearConstraintHelper(cc));
                            _draft.Childs.Add(_draft.Helpers.Last());
                        }
                        else
                        {
                            GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                        }
                        queue.Clear();
                        //editor.ResetTool();
                    }
                    else
                    {
                    LinearConstraintLengthDialog lcd = new LinearConstraintLengthDialog();
                    lcd.Init(dl.Length);
                    lcd.ShowDialog();
                    var cc = new LinearConstraint(dl.V0, dl.V1, lcd.Length);
                    if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new LinearConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    //editor.ResetTool();
                }
                    return;

                }
                if (queue.Count > 1)
                {
                    LinearConstraintLengthDialog lcd = new LinearConstraintLengthDialog();
                    lcd.Init(((queue[0] as DraftPoint).Location - (queue[1] as DraftPoint).Location).Length);
                    lcd.ShowDialog();
                    var cc = new LinearConstraint(queue[0], queue[1], lcd.Length);
                    if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new LinearConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    //editor.ResetTool();
                }
            }
            if (editor.CurrentTool is DraftLineTool && e.Button == MouseButtons.Left)
            {
                var p = (ctx.GetCursor());
                DraftPoint target = null;
                if (nearest is DraftPoint dp)
                {
                    target = dp;
                }
                else
                {
                    target = new DraftPoint(_draft, p.X, p.Y);
                    _draft.Elements.Add(target);
                }

                if (lastDraftPoint != null)
                {
                    _draft.Elements.Add(new DraftLine(lastDraftPoint, target, _draft));
                }
                lastDraftPoint = target;
            }
            if (editor.CurrentTool is RectDraftTool && e.Button == MouseButtons.Left)
            {
                var p = (ctx.GetCursor());
                if (firstClick == null)
                {

                    firstClick = p;
                }
                else
                {

                    var p0 = new DraftPoint(_draft, p.X, p.Y);
                    var p1 = new DraftPoint(_draft, firstClick.Value.X, p.Y);
                    var p2 = new DraftPoint(_draft, firstClick.Value.X, firstClick.Value.Y);
                    var p3 = new DraftPoint(_draft, p.X, firstClick.Value.Y);
                    firstClick = null;
                    editor.ResetTool();
                    _draft.AddElement(p0);
                    _draft.AddElement(p1);
                    _draft.AddElement(p2);
                    _draft.AddElement(p3);

                    var line1 = new DraftLine(p0, p1, _draft);
                    var line2 = new DraftLine(p1, p2, _draft);
                    var line3 = new DraftLine(p2, p3, _draft);
                    var line4 = new DraftLine(p3, p0, _draft);
                    _draft.AddElement(line1);
                    _draft.AddElement(line2);
                    _draft.AddElement(line3);
                    _draft.AddElement(line4);

                    _draft.AddConstraint(new HorizontalConstraint(line1));
                    _draft.AddConstraint(new VerticalConstraint(line2));
                    _draft.AddConstraint(new HorizontalConstraint(line3));
                    _draft.AddConstraint(new VerticalConstraint(line4));

                    foreach (var item in _draft.Constraints.OfType<VerticalConstraint>())
                    {
                        if (_draft.Helpers.Any(z => z.Constraint == item)) continue;
                        _draft.AddHelper(new VerticalConstraintHelper(item));
                    }
                    foreach (var item in _draft.Constraints.OfType<HorizontalConstraint>())
                    {
                        if (_draft.Helpers.Any(z => z.Constraint == item)) continue;
                        _draft.AddHelper(new HorizontalConstraintHelper(item));
                    }
                }
            }
            if (editor.CurrentTool is DraftEllipseTool && e.Button == MouseButtons.Left)
            {
                var p = (ctx.GetCursor());
                if (firstClick == null)
                {

                    firstClick = p;
                }
                else
                {

                    var p0 = new DraftPoint(_draft, p.X, p.Y);

                    var p2 = new DraftPoint(_draft, firstClick.Value.X, firstClick.Value.Y);
                    var c = new DraftPoint(_draft, (firstClick.Value.X + p.X) / 2, (firstClick.Value.Y + p.Y) / 2);

                    editor.ResetTool();


                    _draft.AddElement(new DraftEllipse(c, (decimal)Math.Abs(firstClick.Value.X - p.X) / 2, _draft));
                    firstClick = null;
                }
            }
        }

        PointF? firstClick;

        bool isPressed = false;
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isPressed = false;
            var cp = Cursor.Position;
            var pos = ctx.PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == MouseButtons.Right)
            {
                var dx = Math.Abs(ctx.startx - pos.X);
                var dy = Math.Abs(ctx.starty - pos.Y);
                float eps = 1;
                if ((dx + dy) < eps)
                {
                    contextMenuStrip1.Show(cp);
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                var gcur = ctx.GetCursor();
                var t = ctx.BackTransform(new PointF(ctx.startx, ctx.starty));
                var rxm = Math.Min(t.X, gcur.X);
                var rym = Math.Min(t.Y, gcur.Y);
                var rdx = Math.Abs(t.X - gcur.X);
                var rdy = Math.Abs(t.Y - gcur.Y);
                var rect = new RectangleF(rxm, rym, rdx, rdy);
                if (rect.Width > 1 && rect.Height > 1)
                {
                    var tt = _draft.DraftPoints.Where(z => rect.Contains((float)z.Location.X, (float)z.Location.Y)).ToArray();
                    selected = tt;
                    Form1.Form.SetStatus($"selected: {tt.Count()} points");
                }
            }
        }

        public List<string> Undos = new List<string>();
        public void Undo()
        {
            if (Undos.Count == 0) return;
            var el = XElement.Parse(Undos.Last());
            _draft.Restore(el);
            Undos.RemoveAt(Undos.Count - 1);
        }

        public void Backup()
        {
            StringWriter sw = new StringWriter();
            _draft.Store(sw);
            Undos.Add(sw.ToString());
        }

        public void FitAll()
        {
            if (_draft == null || _draft.DraftPoints.Count() == 0) return;

            var t = _draft.DraftPoints.Select(z => z.Location).ToArray();
            ctx.FitToPoints(t.Select(z => z.ToPointF()).ToArray(), 5);
        }

        DrawingContext ctx = new DrawingContext() { DragButton = MouseButtons.Right };
        public object nearest { get; private set; }
        public object startMiddleDragNearest;
        object[] selected = new object[] { };
        void updateNearest()
        {
            var pos = ctx.GetCursor();
            var _pos = new Vector2d(pos.X, pos.Y);
            double minl = double.MaxValue;
            object minp = null;
            double maxDist = 10 / ctx.zoom;
            foreach (var item in _draft.DraftPoints)
            {
                var d = (item.Location - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }
            }
            foreach (var item in _draft.DraftEllipses)
            {
                var d = (item.Center.Location - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item.Center;
                }

                d = Math.Abs((item.Center.Location - _pos).Length - (double)item.Radius);
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }

            }
            foreach (var item in _draft.DraftLines)
            {
                var loc = (item.V0.Location + item.V1.Location) / 2;
                var d = (loc - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }
            }
            foreach (var item in _draft.Helpers)
            {
                var d = (item.SnapPoint - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }
            }
            if (minl < maxDist)
            {
                nearest = minp;
            }
            else
                nearest = null;
        }

        public enum SubSnapTypeEnum
        {
            None, Point, PointOnLine, CenterLine, Perpendicular, PointOnCircle
        }

        SubSnapTypeEnum subSnapType;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Visible) return;
            ctx.gr.Clear(Color.White);
            ctx.UpdateDrag();
            subSnapType = SubSnapTypeEnum.None;
            updateNearest();

            ctx.DrawLine(Pens.Blue, new PointF(0, 0), new PointF(0, 100));
            ctx.DrawLine(Pens.Red, new PointF(0, 0), new PointF(100, 0));

            if (_draft != null)
            {
                for (int i = 0; i < _draft.DraftPoints.Length; i++)
                {
                    var item = _draft.DraftPoints[i];
                    float gp = 5;
                    var tr = ctx.Transform(item.X, item.Y);

                    if (nearest == item || selected.Contains(item))
                    {
                        ctx.gr.FillRectangle(Brushes.Blue, tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                    }
                    ctx.gr.DrawRectangle(Pens.Black, tr.X - gp, tr.Y - gp, gp * 2, gp * 2);

                }

                for (int i = 0; i < _draft.DraftLines.Length; i++)
                {
                    var el = _draft.DraftLines[i];

                    Vector2d item0 = _draft.DraftLines[i].V0.Location;
                    Vector2d item = _draft.DraftLines[i].V1.Location;
                    var tr = ctx.Transform(item0.X, item0.Y);
                    var tr11 = ctx.Transform(item.X, item.Y);
                    Pen p = new Pen(selected.Contains(el) ? Color.Blue : Color.Black);

                    if (el.Dummy)
                        p.DashPattern = new float[] { 10, 10 };

                    ctx.gr.DrawLine(p, tr, tr11);
                }

                for (int i = 0; i < _draft.DraftEllipses.Length; i++)
                {
                    var el = _draft.DraftEllipses[i];
                    Vector2d item0 = _draft.DraftEllipses[i].Center.Location;
                    var rad = (float)el.Radius * ctx.zoom;
                    var tr = ctx.Transform(item0.X, item0.Y);

                    ctx.gr.DrawEllipse(selected.Contains(el) ? Pens.Blue : Pens.Black, tr.X - rad, tr.Y - rad, rad * 2, rad * 2);

                    float gp = 5;
                    tr = ctx.Transform(el.Center.X, el.Center.Y);

                    if (nearest == el.Center)
                    {
                        ctx.gr.FillRectangle(Brushes.Blue, tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                    }
                    ctx.gr.DrawRectangle(Pens.Black, tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                }

                foreach (var item in _draft.Helpers)
                {
                    if (!item.Visible) continue;

                    item.Draw(ctx);
                }
            }
            if (ctx.MiddleDrag)//measure tool
            {
                Pen pen = new Pen(Color.Blue, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };

                var gcur = ctx.GetCursor();
                var curp = ctx.Transform(gcur);
                double maxSnapDist = 10 / ctx.zoom;

                //check perpendicular of lines?
                foreach (var item in Draft.DraftLines)
                {
                    //get projpoint                     
                    var proj = GeometryUtils.GetProjPoint(gcur.ToVector2d(), item.V0.Location, item.Dir);
                    var sx = ctx.BackTransform(ctx.startx, ctx.starty);
                    var proj2 = GeometryUtils.GetProjPoint(new Vector2d(sx.X, sx.Y), item.V0.Location, item.Dir);
                    if (!item.ContainsPoint(proj))
                        continue;

                    var len = (proj - gcur.ToVector2d()).Length;
                    var len2 = (proj2 - gcur.ToVector2d()).Length;
                    if (len < maxSnapDist)
                    {
                        //sub nearest = projpoint
                        curp = ctx.Transform(proj);
                        gcur = proj.ToPointF();
                        subSnapType = SubSnapTypeEnum.PointOnLine;
                    }
                    if (len2 < maxSnapDist)
                    {
                        //sub nearest = projpoint
                        curp = ctx.Transform(proj2);
                        gcur = proj2.ToPointF();
                        subSnapType = SubSnapTypeEnum.Perpendicular;
                    }
                }

                if (nearest is DraftPoint dp)
                {
                    curp = ctx.Transform(dp.Location);
                    gcur = dp.Location.ToPointF();
                }
                if (nearest is DraftLine dl)
                {
                    var len = (dl.Center - gcur.ToVector2d()).Length;
                    if (len < maxSnapDist)
                    {
                        curp = ctx.Transform(dl.Center);
                        gcur = dl.Center.ToPointF();
                        subSnapType = SubSnapTypeEnum.CenterLine;
                    }
                }
                if (nearest is DraftEllipse de)
                {
                    var diff = (de.Center.Location - new Vector2d(gcur.X, gcur.Y)).Normalized();
                    var onEl = de.Center.Location - diff * (double)de.Radius;
                    //get point on ellipse
                    curp = ctx.Transform(onEl);
                    gcur = onEl.ToPointF();
                }
                var t = ctx.Transform(new PointF(ctx.startx, ctx.starty));

                //snap starto
                if (startMiddleDragNearest is DraftPoint sdp)
                {

                }

                ctx.gr.DrawLine(pen, ctx.startx, ctx.starty, curp.X, curp.Y);
                var pp = ctx.BackTransform(new PointF(ctx.startx, ctx.starty));
                Vector2 v1 = new Vector2(pp.X, pp.Y);
                Vector2 v2 = new Vector2(gcur.X, gcur.Y);
                var dist = (v2 - v1).Length;
                var hintText = dist.ToString("N2");
                if (subSnapType == SubSnapTypeEnum.PointOnLine)
                {
                    hintText = "[point on line] : " + hintText;
                }
                if (subSnapType == SubSnapTypeEnum.CenterLine)
                {
                    hintText = "[line center] : " + hintText;
                }
                if (subSnapType == SubSnapTypeEnum.Perpendicular)
                {
                    hintText = "[perpendicular] : " + hintText;
                }
                ctx.gr.DrawString(hintText, SystemFonts.DefaultFont, Brushes.Black, curp.X + 10, curp.Y);
            }
            if (ctx.isLeftDrag)//rect tool
            {
                Pen pen = new Pen(Color.Blue, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
                var gcur = ctx.GetCursor();

                var curp = ctx.Transform(gcur);

                var t = ctx.Transform(new PointF(ctx.startx, ctx.starty));
                var rxm = Math.Min(ctx.startx, curp.X);
                var rym = Math.Min(ctx.starty, curp.Y);
                var rdx = Math.Abs(ctx.startx - curp.X);
                var rdy = Math.Abs(ctx.starty - curp.Y);
                ctx.gr.DrawRectangle(pen, rxm, rym, rdx, rdy);
                var pp = ctx.BackTransform(new PointF(ctx.startx, ctx.starty));
                Vector2 v1 = new Vector2(pp.X, pp.Y);
                Vector2 v2 = new Vector2(gcur.X, gcur.Y);
                var dist = (v2 - v1).Length;
                ctx.gr.DrawString(dist.ToString("N2"), SystemFonts.DefaultFont, Brushes.Black, curp.X + 10, curp.Y);
            }
            pictureBox1.Image = ctx.Bmp;
        }

        IEditor editor;

        Draft _draft;
        public Draft Draft => _draft;
        public void SetDraft(Draft draft)
        {
            _draft = draft;

            //restore helpers
            foreach (var citem in draft.Constraints)
            {
                if (draft.Helpers.Any(z => z.Constraint == citem)) continue;
                if (citem is LinearConstraint lc)
                {
                    draft.AddHelper(new LinearConstraintHelper(lc));
                }
                if (citem is VerticalConstraint vc)
                {
                    _draft.AddHelper(new VerticalConstraintHelper(vc));
                }
                if (citem is HorizontalConstraint hc)
                {
                    _draft.AddHelper(new HorizontalConstraintHelper(hc));
                }
            }
        }
        internal void Init(IEditor editor)
        {
            this.editor = editor;
            editor.ToolChanged += Editor_ToolChanged;
        }

        private void Editor_ToolChanged(ITool obj)
        {
            lastDraftPoint = null;
        }

        internal void Finish()
        {
            _draft.EndEdit();
        }

        internal void Clear()
        {
            _draft.Elements.Clear();
            _draft.Helpers.Clear();
            _draft.Constraints.Clear();
        }

        internal void CloseLine()
        {
            _draft.Elements.Add(new DraftLine(_draft.DraftPoints.First(), _draft.DraftPoints.Last(), _draft));
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected == null || selected.Length == 0) return;
            Backup();
            foreach (var item in selected)
            {
                if (item is DraftElement de)
                    _draft.RemoveElement(de);

                if (item is IDrawable dd)
                {
                    _draft.RemoveChild(dd);
                }
            }
        }

        private void detectCOMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;
            var sx = points.Sum(z => z.X) / points.Length;
            var sy = points.Sum(z => z.Y) / points.Length;

            _draft.AddElement(new DraftPoint(_draft, sx, sy));
        }

        private void approxByCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;
            var sx = points.Sum(z => z.X) / points.Length;
            var sy = points.Sum(z => z.Y) / points.Length;
            var dp = new DraftPoint(_draft, sx, sy);
            var rad = (decimal)(points.Select(z => (z.Location - dp.Location).Length).Average());

            _draft.AddElement(new DraftEllipse(dp, rad, _draft));
            if (GUIHelpers.ShowQuestion("Delete source points?", ParentForm.Text) == DialogResult.Yes)
            {
                for (int p = 0; p < points.Length; p++)
                {
                    _draft.RemoveElement(points[p]);
                }
            }

        }

        internal void FlipHorizontal()
        {
            var maxy = _draft.DraftPoints.Max(z => z.Y);
            var miny = _draft.DraftPoints.Min(z => z.Y);
            var my = (maxy + miny) / 2;
            for (int i = 0; i < _draft.DraftPoints.Length; i++)
            {
                _draft.DraftPoints[i].SetLocation(_draft.DraftPoints[i].X, 2 * my - _draft.DraftPoints[i].Location.Y);

            }
        }
    }

    public interface IDraftEditor
    {
        object nearest { get; }
        Draft Draft { get; }
    }
}
