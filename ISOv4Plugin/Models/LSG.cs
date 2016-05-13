using System.Globalization;
using System.Linq;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class LSG : IWriter
    {
        public LSGA? A { get; set; }
        public object[] Items { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("LSG");
            if(A != null)
                xmlBuilder.WriteAttributeString("A", ((int)A).ToString(CultureInfo.InvariantCulture));
            if(Items != null)
            {
                foreach (var item in Items.Cast<IWriter>())
                {
                    xmlBuilder = item.WriteXML(xmlBuilder);
                }
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
