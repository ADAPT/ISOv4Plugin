using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PNT : IWriter
    {
        public PNTA? A { get; set; }
        public decimal C { get; set; }
        public decimal D { get; set; }
        public long E { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PNT");
            if(A != null)
                xmlBuilder.WriteAttributeString("A", ((int)A).ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteAttributeString("C", C.ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteAttributeString("D", D.ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteAttributeString("E", E.ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
