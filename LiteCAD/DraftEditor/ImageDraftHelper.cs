using System.Drawing;
using LiteCAD.Common;
using LiteCAD.BRep.Editor;
using OpenTK;
using System.Xml.Linq;
using System;
using System.IO;
using System.Drawing.Imaging;

namespace LiteCAD.DraftEditor
{
    [XmlName(XmlName = "imageDraftHelper")]
    public class ImageDraftHelper : AbstractDrawable, IDraftHelper
    {
        public ImageDraftHelper(Draft parent, Bitmap bmp)
        {
            DraftParent = parent;
            Image = bmp;
            Width = Image.Width;
            Z = -1;
        }

        public ImageDraftHelper(XElement el, Draft draft)
        {
            DraftParent = draft;
            Width = Helpers.ParseDouble(el.Attribute("width").Value);
            var bts = Convert.FromBase64String(el.Value);
            var ms = new MemoryStream(bts);
            Image = Bitmap.FromStream(ms) as Bitmap;
            Z = -1;
            Rotation = Helpers.ParseDouble(el.Attribute("rotation").Value);
            X = Helpers.ParseDouble(el.Attribute("x").Value);
            Y = Helpers.ParseDouble(el.Attribute("y").Value);
            //ms.Dispose();
        }
        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<imageDraftHelper x=\"{X}\" y=\"{Y}\" rotation=\"{Rotation}\" width=\"{Width}\">");
            MemoryStream ms = new MemoryStream();
            Image.Save(ms, ImageFormat.Jpeg);
            var bstr = Convert.ToBase64String(ms.ToArray());
            writer.WriteLine(bstr);
            ms.Dispose();
            writer.WriteLine($"</imageDraftHelper>");
        }

        Bitmap Image;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Rotation { get; set; }
        public double Scale { get; set; }
        public bool Enabled { get; set; }
        public bool DrawBound { get; set; }

        public Draft DraftParent { get; private set; }

        public override void Draw()
        {

        }

        public void Draw(IDrawingContext ctx)
        {
            var editor = ctx.Tag as IDraftEditor;
            var tr0 = ctx.Transform(X, Y);
            var tr1 = ctx.Transform(X + Image.Width, Y - Image.Height);
            ctx.PushMatrix();
            ctx.ResetMatrix();
            ctx.Scale(ctx.zoom, ctx.zoom);
            ctx.Translate(ctx.sx, -ctx.sy);
            ctx.Translate(X, Y);
            float scale = (float)(Width / Image.Width);


            ctx.RotateDegress((float)Rotation);

            //ctx.DrawImage(Image, tr0.X, tr0.Y, tr1.X, tr1.Y);
            //ctx.DrawRectangle(tr0.X, tr0.Y, tr1.X - tr0.X, tr1.Y - tr0.Y);
            ctx.DrawImage(Image, 0, 0, scale * Image.Width, scale * (Image.Height));
            if (DrawBound)
                ctx.DrawRectangle(0, 0, scale * Image.Width, Image.Height * scale);


            ctx.PopMatrix();

            //ctx.DrawCircle()
        }
    }
}
