namespace LiteCAD
{
    public abstract class AbstractTool : ITool
    {
        public abstract void Deselect();
   

        public abstract void Draw();
        

        public abstract void MouseDown();

        public abstract void MouseUp();

        public abstract void Select();        

        public abstract void Update();
        
    }
}