using System;

namespace LiteCAD.Common
{
    public interface IEditFieldsContainer
    {
        IName[] GetObjects();
    }
    public class EditFieldAttribute : Attribute
    {

    }
}