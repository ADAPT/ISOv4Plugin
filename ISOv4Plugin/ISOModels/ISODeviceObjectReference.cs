/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODeviceObjectReference : ISOElement
    {
        //Attributes
        public int DeviceObjectId  { get; set; }
        
        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DOR");
            xmlBuilder.WriteXmlAttribute<int>("A", DeviceObjectId );
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceObjectReference ReadXML(XmlNode node)
        {
            ISODeviceObjectReference item = new ISODeviceObjectReference();
            item.DeviceObjectId = node.GetXmlNodeValueAsInt("@A");
            return item;
        }

        public static IEnumerable<ISODeviceObjectReference> ReadXML(XmlNodeList nodes)
        {
            List<ISODeviceObjectReference> items = new List<ISODeviceObjectReference>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceObjectReference.ReadXML(node));
            }
            return items;
        }
    }
}