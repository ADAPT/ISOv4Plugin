/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODeviceProperty : ISOElement
    {
        //Attributes
        public int ObjectID { get; set; }
        public string DDI  { get; set; }
        public long Value  { get; set; }
        public string Designator { get; set; }
        public int? DeviceValuePresentationObjectId { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DPT");
            xmlBuilder.WriteXmlAttribute<int>("A", ObjectID);
            xmlBuilder.WriteXmlAttribute("B", DDI );
            xmlBuilder.WriteXmlAttribute<long>("C", Value);
            xmlBuilder.WriteXmlAttribute("D", Designator);
            xmlBuilder.WriteXmlAttribute<int>("E", DeviceValuePresentationObjectId);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceProperty ReadXML(XmlNode node)
        {
            ISODeviceProperty item = new ISODeviceProperty();
            item.ObjectID = node.GetXmlNodeValueAsInt("@A");
            item.DDI  = node.GetXmlNodeValue("@B");
            item.Value = node.GetXmlNodeValueAsLong("@C");
            item.Designator = node.GetXmlNodeValue("@D");
            item.DeviceValuePresentationObjectId = node.GetXmlNodeValueAsNullableInt("@E");

            return item;
        }

        public static IEnumerable<ISODeviceProperty> ReadXML(XmlNodeList nodes)
        {
            List<ISODeviceProperty> items = new List<ISODeviceProperty>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceProperty.ReadXML(node));
            }
            return items;
        }
    }
}