using System.Reflection;

namespace LiteCAD.Common
{
    public class StringFieldEditor : IName
    {
        public StringFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public string Value
        {
            get
            {
                return ((string)Field.GetValue(Object));
            }
            set
            {
                Field.SetValue(Object, value);
            }
        }

    }
}