using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class CVT : IWriter 
    {
        public string B { get; set; }
        public string A { get; set; }
        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CVT");
            if (!string.IsNullOrEmpty(A)) 
                xmlBuilder.WriteAttributeString("A", A);
            if (!string.IsNullOrEmpty(B)) 
                xmlBuilder.WriteAttributeString("B", B);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }
    }
}
