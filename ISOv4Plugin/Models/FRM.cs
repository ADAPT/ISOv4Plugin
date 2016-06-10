using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class FRM : IWriter
    {
        public string A { get; set; }
        public string B { get; set; }
        public string I { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("FRM");
            if (!string.IsNullOrEmpty(A))
                xmlBuilder.WriteAttributeString("A", A);
            if (!string.IsNullOrEmpty(B)) 
                xmlBuilder.WriteAttributeString("B", B);
            if (!string.IsNullOrEmpty(I)) 
                xmlBuilder.WriteAttributeString("I", I);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}