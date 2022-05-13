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

namespace LiteCAD.DraftEditor
{
    public partial class DraftEditorControl : UserControl
    {
        public DraftEditorControl()
        {
            InitializeComponent();
            ctx.Init(pictureBox1);
            ctx.InitGraphics();

            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseDown += PictureBox1_MouseDown; ;
        }

        List<DraftPoint> queue = new List<DraftPoint>();
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
            if (editor.CurrentTool is LinearConstraintTool && e.Button == MouseButtons.Left)
            {
                queue.Add(nearest as DraftPoint);
                if (queue.Count > 1)
                {
                    var cc = new LinearConstraint(queue[0], queue[1], (decimal)(queue[0].Location - queue[1].Location).Length);
                    _draft.Constraints.Add(cc);
                    _draft.Helpers.Add(new LinearConstraintHelper(cc));
                    queue.Clear();
                    editor.ResetTool();
                }
            }
            if (editor.CurrentTool is DraftLineTool && e.Button == MouseButtons.Left)
            {
                var p = (ctx.GetCursor());
                var last = _draft.Elements.OfType<DraftPoint>().LastOrDefault();
                _draft.Elements.Add(new DraftPoint(_draft, p.X, p.Y));
                if (last != null)
                {
                    _draft.Elements.Add(new DraftLine(last, _draft.Elements.Last() as DraftPoint, _draft));

                }
            }
        }

        bool isPressed = false;
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isPressed = false;
        }

        DrawingContext ctx = new DrawingContext();
        object nearest;
        void updateNearest()
        {
            var pos = ctx.GetCursor();
            var _pos = new Vector2d(pos.X, pos.Y);
            double minl = double.MaxValue;
            object minp = null;
            foreach (var item in _draft.DraftPoints)
            {
                var d = (item.Location - _pos).Length;
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
            nearest = minp;
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
                    Vector2d item0 = _draft.DraftLines[i].V0.Location;
                    Vector2d item = _draft.DraftLines[i].V1.Location;
                    var tr = ctx.Transform(item0.X, item0.Y);
                    var tr11 = ctx.Transform(item.X, item.Y);
                    ctx.gr.DrawLine(Pens.Black, tr, tr11);
                }

                foreach (var item in _draft.Helpers)
                {
                    item.Draw(ctx);
                }
            }
            if (ctx.MiddleDrag)//measure tool
            {
                Pen pen = new Pen(Color.Blue, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };

                var curp = ctx.Transform(ctx.GetCursor());
                var t = ctx.Transform(new PointF(ctx.startx, ctx.starty));
                ctx.gr.DrawLine(pen, ctx.startx, ctx.starty, curp.X, curp.Y);
                var pp = ctx.BackTransform(new PointF(ctx.startx, ctx.starty));
                Vector2 v1 = new Vector2(pp.X, pp.Y);
                Vector2 v2 = new Vector2(ctx.GetCursor().X, ctx.GetCursor().Y);
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

        }

        internal void Finish()
        {
            _draft.EndEdit();
        }

        internal void Clear()
        {
            _draft.Elements.Clear();
        }

        internal void CloseLine()
        {
            _draft.Elements.Add(new DraftLine(_draft.DraftPoints.First(), _draft.DraftPoints.Last(), _draft));
        }
    }


}
