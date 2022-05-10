using OpenTK;

namespace LiteCAD
{
    public abstract class CameraViewManager
    {
        public GLControl Control;
        public EventWrapperGlControl EventWrapper;

        public virtual void Detach()
        {
            EventWrapper.MouseUpAction = null;
            EventWrapper.MouseDownAction = null;
            EventWrapper.MouseWheelAction = null;
            EventWrapper.KeyDownAction = null;
            EventWrapper.KeyUpUpAction = null;

        }

        public abstract void Update();

        public virtual void Attach(EventWrapperGlControl control, Camera camera)
        {
            Control = control.Control;
            EventWrapper = control;
        }
    }
}

