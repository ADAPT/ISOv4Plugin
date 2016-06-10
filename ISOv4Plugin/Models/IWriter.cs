using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public interface IWriter
    {
        XmlWriter WriteXML(XmlWriter xmlBuilder);
    }
}
