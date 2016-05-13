using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class CTR : IWriter
    {
        public string A { get; set; }
        public string B { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CTR");
            if (!string.IsNullOrEmpty(A))
                xmlBuilder.WriteAttributeString("A", A);
            if (!string.IsNullOrEmpty(B)) 
                xmlBuilder.WriteAttributeString("B", B);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}