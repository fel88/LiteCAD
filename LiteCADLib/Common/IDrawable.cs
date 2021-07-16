namespace LiteCAD.Common
{
    public interface IDrawable
    {
        bool Visible { get; set; }
        void Draw();
        bool Selected { get; set; }
    }

}