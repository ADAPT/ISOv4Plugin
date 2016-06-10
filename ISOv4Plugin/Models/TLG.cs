using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TLG : IWriter
    {
        public string A { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TLG");
            if (!string.IsNullOrEmpty(A)) 
                xmlBuilder.WriteAttributeString("A", A);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
