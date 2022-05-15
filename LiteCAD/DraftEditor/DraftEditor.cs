﻿using System;
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

namespace LiteCAD.DraftEditor
{
    public partial class DraftEditorControl : UserControl
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

            if (selected is IDrawable dd)
            {
                dd.Selected = false;
            }

            selected = nearest;
            if (selected is IDrawable dd2)
            {
                dd2.Selected = true;
            }

            if (e == MouseButtons.Middle)
            {
                ctx.isMiddleDrag = true;
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
                if (nearest is LinearConstraintHelper lh)
                {
                    editor.ObjectSelect(lh.constraint);
                }
                else
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
                        Helpers.Warning("such constraint already exist", ParentForm.Text);
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
                        Helpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    editor.ResetTool();
                }
            }
            if (editor.CurrentTool is LinearConstraintTool && e.Button == MouseButtons.Left)
            {
                if (nearest is DraftPoint)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftPoint);
                }
                if (queue.Count > 1)
                {
                    var cc = new LinearConstraint(queue[0], queue[1], (decimal)((queue[0] as DraftPoint).Location - (queue[1] as DraftPoint).Location).Length);
                    if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new LinearConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        Helpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    editor.ResetTool();
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

                    _draft.AddElement(new DraftLine(p0, p1, _draft));
                    _draft.AddElement(new DraftLine(p1, p2, _draft));
                    _draft.AddElement(new DraftLine(p2, p3, _draft));
                    _draft.AddElement(new DraftLine(p3, p0, _draft));
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
        }

        public void FitAll()
        {
            if (_draft == null || _draft.DraftPoints.Count() == 0) return;

            var t = _draft.DraftPoints.Select(z => z.Location).ToArray();
            ctx.FitToPoints(t.Select(z => z.ToPointF()).ToArray(), 5);
        }

        DrawingContext ctx = new DrawingContext() { DragButton = MouseButtons.Left };
        object nearest;
        object selected;
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
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Visible) return;
            ctx.gr.Clear(Color.White);
            ctx.UpdateDrag();
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

                    if (nearest == item)
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
                    Pen p = new Pen(el == selected ? Color.Blue : Color.Black);

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

                    ctx.gr.DrawEllipse(el == selected ? Pens.Blue : Pens.Black, tr.X - rad, tr.Y - rad, rad * 2, rad * 2);

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
                if (nearest is DraftPoint dp)
                {
                    curp = ctx.Transform(dp.Location);
                    gcur = dp.Location.ToPointF();
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
                ctx.gr.DrawLine(pen, ctx.startx, ctx.starty, curp.X, curp.Y);
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
        public void SetDraft(Draft draft)
        {
            _draft = draft;
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
            if (selected == null) return;
            if (selected is DraftElement de)
                _draft.RemoveElement(de);

            if (selected is IDrawable dd)
            {
                _draft.RemoveChild(dd);
            }
        }
    }
}
