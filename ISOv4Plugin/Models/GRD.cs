using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class GRD : IWriter
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public ulong E { get; set; }
        public ulong F { get; set; }
        public string G { get; set; }
        public ulong? H { get; set; }
        public byte I { get; set; }
        public byte? J { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("GRD");
            
                xmlBuilder.WriteAttributeString("A", A.ToString());
                xmlBuilder.WriteAttributeString("B", B.ToString());
                xmlBuilder.WriteAttributeString("C", C.ToString());
                xmlBuilder.WriteAttributeString("D", D.ToString());
                xmlBuilder.WriteAttributeString("E", E.ToString());
                xmlBuilder.WriteAttributeString("F", F.ToString());
                if (!string.IsNullOrEmpty(G))
                    xmlBuilder.WriteAttributeString("G", G);
                if (H != null)
                    xmlBuilder.WriteAttributeString("H", H.ToString());
                xmlBuilder.WriteAttributeString("I", I.ToString());
                if(J != null)
                    xmlBuilder.WriteAttributeString("J", J.Value.ToString());
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
