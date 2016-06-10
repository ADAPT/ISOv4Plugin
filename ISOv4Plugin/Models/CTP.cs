using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class CTP : IWriter
    {
        public string B { get; set; }
        public string A { get; set; }
        public CVT[] Items { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CTP");
            if (!string.IsNullOrEmpty(A)) 
                xmlBuilder.WriteAttributeString("A", A);
            if (!string.IsNullOrEmpty(B))
                xmlBuilder.WriteAttributeString("B", B);

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    xmlBuilder = item.WriteXML(xmlBuilder);
                }
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}