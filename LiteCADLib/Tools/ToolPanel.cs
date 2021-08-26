using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;

namespace LiteCAD.Tools
{
    public partial class ToolPanel : UserControl
    {
        public ToolPanel()
        {
            InitializeComponent();
            DoubleBuffered = true;

            BackColor = Color.FromArgb(63, 82, 111);
            toolHeader1.BackColor = Color.FromArgb(154, 166, 187);
            toolHeader1.Dock = DockStyle.Top;
            toolHeader1.Enabled = false;
            toolHeader1.Font = new Font("Tahoma", 8.25f, FontStyle.Bold);
            toolHeader1.ForeColor = Color.White;
            toolHeader1.Location = new Point(3, 3);
            toolHeader1.Name = "header1";
            toolHeader1.Size = new Size(0, 23);
            toolHeader1.TabIndex = 4;
            toolHeader1.Text = "header1";
            toolHeader1.TextAlign = ContentAlignment.MiddleCenter;
            toolHeader1.TopColor = Color.FromArgb(63, 82, 111);
            MouseLeave += ToolPanel_MouseLeave;
            Padding = new Padding(3);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;

            panel2.AutoSize = true;
            panel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel2.BackColor = Color.FromArgb(250, 250, 250);

            panel2.Location = new Point(3, 26);
            panel2.Name = "panel1";

            panel2.TabIndex = 5;

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void ToolPanel_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }


        private bool moving;
        private Point pivot = Point.Empty;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && Cursor == Cursors.SizeAll)
            {
                pivot = e.Location;
                moving = true;
                Anchor = AnchorStyles.None;
            }
        }
        public string Title
        {
            get
            {
                return toolHeader1.Text;
            }
            set
            {
                if (toolHeader1.Text == value)
                {
                    return;
                }
                toolHeader1.Text = value;
                toolHeader1.Invalidate();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            moving = false;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Location.Y < toolHeader1.Height)
            {
                Cursor = Cursors.SizeAll;
            }
            else
            {
                Cursor = Cursors.Default;
            }
            if (moving)
            {
                Anchor = AnchorStyles.None;
                Left += e.Location.X - pivot.X;
                Top += e.Location.Y - pivot.Y;
            }
        }



        internal void SetControl(UserControl pointEditorToolPanel)
        {
            panel2.Controls.Add(pointEditorToolPanel);
        }
    }
}
