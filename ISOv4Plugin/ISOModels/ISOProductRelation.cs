/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOProductRelation : ISOElement
    {
        //Attributes
        public string ProductIdRef { get; set; }
        public long QuantityValue { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PRN");
            xmlBuilder.WriteXmlAttribute("A", ProductIdRef);
            xmlBuilder.WriteXmlAttribute<long>("B", QuantityValue);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOProductRelation ReadXML(XmlNode node)
        {
            ISOProductRelation item = new ISOProductRelation();
            item.ProductIdRef = node.GetXmlNodeValue("@A");
            item.QuantityValue = node.GetXmlNodeValueAsNullableLong("@B").Value;
            return item;
        }

        public static IEnumerable<ISOProductRelation> ReadXML(XmlNodeList nodes)
        {
            List<ISOProductRelation> items = new List<ISOProductRelation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOProductRelation.ReadXML(node));
            }
            return items;
        }
    }
}