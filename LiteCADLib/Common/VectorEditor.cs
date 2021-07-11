using OpenTK;
using System.Reflection;

namespace LiteCAD.Common
{
    public class VectorEditor : IName
    {
        public VectorEditor(PropertyInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public PropertyInfo Field;
        public double X
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).X;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.X = value;
                Field.SetValue(Object, v);
            }
        }

        public double Y
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).Y;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.Y = value;
                Field.SetValue(Object, v);
            }
        }

        public double Z
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).Z;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.Z = value;
                Field.SetValue(Object, v);
            }
        }
    }
}