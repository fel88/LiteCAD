namespace LiteCAD.Common
{
    public abstract class AbstractDrawable : IDrawable
    {
        public abstract void Draw();
        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }
    }

}