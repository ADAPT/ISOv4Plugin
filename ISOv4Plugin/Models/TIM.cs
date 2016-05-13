using System;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TIM : IWriter
    {
        public DateTime A { get; set; }
        public DateTime B { get; set; }
        public bool BSpecified { get; set; }
        public TIMD D { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TIM");
            xmlBuilder.WriteAttributeString("A", A.ToString("yyyy-MM-ddThh:mm:ss"));
            xmlBuilder.WriteAttributeString("B", B.ToString("yyyy-MM-ddThh:mm:ss"));
            xmlBuilder.WriteAttributeString("D", ((int)D).ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
