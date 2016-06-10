using System.Globalization;
using System.Linq;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PLN : IWriter
    {
        public PLNA? A { get; set; }
        public string B { get; set; }
        public IWriter[] Items { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PLN");
            if(A != null)
                xmlBuilder.WriteAttributeString("A", ((int)A).ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(B))
                xmlBuilder.WriteAttributeString("B", B);
            if(Items != null)
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
