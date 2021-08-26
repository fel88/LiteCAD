using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LiteCAD.Tools
{
    public class ToolHeader : Control
    {
        public ToolHeader()
        {
            DoubleBuffered = true;
			Height = 40;
        }
		private ContentAlignment textAlign = ContentAlignment.MiddleCenter;

		private Color topColor = Color.White;

		public ContentAlignment TextAlign
		{
			get
			{
				return textAlign;
			}
			set
			{
				if (textAlign == value)
				{
					return;
				}
				textAlign = value;
				Invalidate();
			}
		}

		public Color TopColor
		{
			get
			{
				return topColor;
			}
			set
			{
				if (topColor == value)
				{
					return;
				}
				topColor = value;
				Invalidate();
			}
		}
		protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            Brush brushBack = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), TopColor, BackColor);
            Brush brushText = new SolidBrush(ForeColor);
            StringFormat sf = new StringFormat();
            ContentAlignment textAlign = TextAlign;

			g.FillRectangle(brushBack, new Rectangle(0, 0, Width, Height));
			g.DrawString(Text, Font, brushText, new RectangleF(0f, 0f, Width, Height), sf);
			
		}
    }


}
