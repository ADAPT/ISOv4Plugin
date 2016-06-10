using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PDT : IWriter
    {
        public string B { get; set; }
        public string A { get; set; }
        public PDTF? F { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PDT");
            if(!string.IsNullOrEmpty(A))
                xmlBuilder.WriteAttributeString("A", A);
            if(!string.IsNullOrEmpty(B))
                xmlBuilder.WriteAttributeString("B", B);
            if(F != null)
                xmlBuilder.WriteAttributeString("F", ((int)F).ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}