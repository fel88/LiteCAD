namespace LiteCAD.Common
{
    public abstract class AbstractHelper : IDrawable
    {
        public virtual void Draw()
        {

        }
        public bool Visible { get; set; } = true;
    }
    public interface IDrawable
    {
        void Draw();
    }

}