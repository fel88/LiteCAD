namespace LiteCAD
{
    public interface ITool
    {
        void Update();
        void MouseDown();
        void MouseUp();
        void Draw();
        void Select();
        void Deselect();
    }
}