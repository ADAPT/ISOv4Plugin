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
    public class ISODeviceProcessData : ISOElement
    {
        //Attributes
        public int ObjectID { get; set; }
        public string DDI  { get; set; }
        public int Property  { get; set; }
        public int TriggerMethods { get; set; }
        public string Designator { get; set; }
        public int? DeviceValuePresentationObjectId { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DPD");
            xmlBuilder.WriteXmlAttribute<int>("A", ObjectID);
            xmlBuilder.WriteXmlAttribute("B", DDI );
            xmlBuilder.WriteXmlAttribute<int>("C", Property);
            xmlBuilder.WriteXmlAttribute<int>("D", TriggerMethods);
            xmlBuilder.WriteXmlAttribute("E", Designator);
            xmlBuilder.WriteXmlAttribute<int>("F", DeviceValuePresentationObjectId);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceProcessData ReadXML(XmlNode node)
        {
            ISODeviceProcessData item = new ISODeviceProcessData();
            item.ObjectID = node.GetXmlNodeValueAsInt("@A");
            item.DDI  = node.GetXmlNodeValue("@B");
            item.Property  = node.GetXmlNodeValueAsInt("@C");
            item.TriggerMethods = node.GetXmlNodeValueAsInt("@D");
            item.Designator = node.GetXmlNodeValue("@E");
            item.DeviceValuePresentationObjectId = node.GetXmlNodeValueAsNullableInt("@F");

            return item;
        }

        public static IEnumerable<ISODeviceProcessData> ReadXML(XmlNodeList nodes)
        {
            List<ISODeviceProcessData> items = new List<ISODeviceProcessData>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceProcessData.ReadXML(node));
            }
            return items;
        }
    }
}