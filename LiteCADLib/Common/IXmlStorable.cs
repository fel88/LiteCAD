using System.IO;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    public interface IXmlStorable
    {
        void StoreXml(TextWriter writer);
        void RestoreXml(XElement elem);

    }
}