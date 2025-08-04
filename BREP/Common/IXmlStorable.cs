using System.IO;
using System.Xml.Linq;

namespace BREP
{
    public interface IXmlStorable
    {
        void StoreXml(TextWriter writer);
        void RestoreXml(XElement elem);

    }
}