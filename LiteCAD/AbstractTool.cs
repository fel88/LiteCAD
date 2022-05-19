using LiteCAD.DraftEditor;
using System.Windows.Forms;

namespace LiteCAD
{
    public abstract class AbstractTool : ITool
    {
        public abstract void Deselect();
   

        public abstract void Draw();

        public IDraftEditor Editor;
        public abstract void MouseDown(MouseEventArgs e);

        public abstract void MouseUp(MouseEventArgs e);

        public abstract void Select();        

        public abstract void Update();
        
    }
}