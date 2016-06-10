using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TSK : IWriter
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public TSKG G { get; set; }
        public byte H { get; set; }
        public byte I { get; set; }
        public byte J { get; set; }
        public IWriter[] Items { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TSK");
            if (!string.IsNullOrEmpty(A)) 
                xmlBuilder.WriteAttributeString("A", A);
            if (!string.IsNullOrEmpty(B))
                xmlBuilder.WriteAttributeString("B", B);
            if (!string.IsNullOrEmpty(C))
                xmlBuilder.WriteAttributeString("C", C);
            if (!string.IsNullOrEmpty(D))
                xmlBuilder.WriteAttributeString("D", D);
            if (!string.IsNullOrEmpty(E))
                xmlBuilder.WriteAttributeString("E", E);
            if (!string.IsNullOrEmpty(F)) 
                xmlBuilder.WriteAttributeString("F", F);
            xmlBuilder.WriteAttributeString("G", ((int) G).ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteAttributeString("H", ((int)H).ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteAttributeString("I", ((int)I).ToString(CultureInfo.InvariantCulture));
            xmlBuilder.WriteAttributeString("J", ((int)J).ToString(CultureInfo.InvariantCulture));
            if(Items != null)
            {
                foreach (var item in Items)
                {
                    item.WriteXML(xmlBuilder);
                }
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}