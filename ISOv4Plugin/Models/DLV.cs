using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class DLV : IWriter
    {

        public string A { get; set; }
        public long? B { get; set; }
        public string C { get; set; }
        public ulong? D { get; set; }
        public byte? E { get; set; }
        public byte? F { get; set; }


        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DLV");
            if (!string.IsNullOrEmpty(A))
                xmlBuilder.WriteAttributeString("A", A);
            if(B != null)
                xmlBuilder.WriteAttributeString("B", B.Value.ToString(CultureInfo.InvariantCulture));
            if(!string.IsNullOrEmpty(C))
                xmlBuilder.WriteAttributeString("C", C);
            if(D != null)
                xmlBuilder.WriteAttributeString("D", D.Value.ToString(CultureInfo.InvariantCulture));
            if(E != null)
                xmlBuilder.WriteAttributeString("E", E.Value.ToString(CultureInfo.InvariantCulture));
            if(F != null) 
                xmlBuilder.WriteAttributeString("F", F.Value.ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
