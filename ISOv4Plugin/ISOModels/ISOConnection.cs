/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOConnection : ISOElement
    {
        //Attributes
        public string DeviceIdRef_0 { get; set; }
        public string DeviceElementIdRef_0 { get; set; }
        public string DeviceIdRef_1 { get; set; }
        public string DeviceElementIdRef_1 { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CNN");
            xmlBuilder.WriteXmlAttribute("A", DeviceIdRef_0);
            xmlBuilder.WriteXmlAttribute("B", DeviceElementIdRef_0);
            xmlBuilder.WriteXmlAttribute("C", DeviceIdRef_1);
            xmlBuilder.WriteXmlAttribute("D", DeviceElementIdRef_1);

            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOConnection ReadXML(XmlNode node)
        {
            ISOConnection item = new ISOConnection();
            item.DeviceIdRef_0 = node.GetXmlNodeValue("@A");
            item.DeviceElementIdRef_0 = node.GetXmlNodeValue("@B");
            item.DeviceIdRef_1 = node.GetXmlNodeValue("@C");
            item.DeviceElementIdRef_1 = node.GetXmlNodeValue("@D");

            return item;
        }

        public static IEnumerable<ISOConnection> ReadXML(XmlNodeList nodes)
        {
            List<ISOConnection> items = new List<ISOConnection>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOConnection.ReadXML(node));
            }
            return items;
        }
    }
}