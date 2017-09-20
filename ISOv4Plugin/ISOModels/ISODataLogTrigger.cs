/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODataLogTrigger : ISOElement
    {
        //Attributes
        public string DataLogDDI { get; set; }
        public byte? DataLogMethod { get; set; }
        public long? DataLogDistanceInterval { get; set; }
        public long? DataLogTimeInterval { get; set; }
        public long? DataLogThresholdMinimum { get; set; }
        public long? DataLogThresholdMaximum { get; set; }
        public long? DataLogThresholdChange { get; set; }
        public string DeviceElementIdRef { get; set; }
        public string ValuePresentationIdRef { get; set; }
        public long? DataLogPGN { get; set; }
        public byte? DataLogPGNStartBit { get; set; }
        public byte? DataLogPGNStopBit { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DLT");
            xmlBuilder.WriteXmlAttribute("A", DataLogDDI);
            xmlBuilder.WriteXmlAttribute("B", DataLogMethod);
            xmlBuilder.WriteXmlAttribute("C", DataLogDistanceInterval);
            xmlBuilder.WriteXmlAttribute("D", DataLogTimeInterval);
            xmlBuilder.WriteXmlAttribute("E", DataLogThresholdMinimum);
            xmlBuilder.WriteXmlAttribute("F", DataLogThresholdMaximum);
            xmlBuilder.WriteXmlAttribute("G", DataLogThresholdChange);
            xmlBuilder.WriteXmlAttribute("H", DeviceElementIdRef);
            xmlBuilder.WriteXmlAttribute("I", ValuePresentationIdRef);
            xmlBuilder.WriteXmlAttribute("J", DataLogPGN);
            xmlBuilder.WriteXmlAttribute("K", DataLogPGNStartBit);
            xmlBuilder.WriteXmlAttribute("L", DataLogPGNStopBit);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODataLogTrigger ReadXML(XmlNode node)
        {
            ISODataLogTrigger trigger = new ISODataLogTrigger();
            trigger.DataLogDDI = node.GetXmlNodeValue("@A");
            trigger.DataLogMethod = node.GetXmlNodeValueAsNullableByte("@B");
            trigger.DataLogDistanceInterval = node.GetXmlNodeValueAsNullableLong("@C");
            trigger.DataLogTimeInterval = node.GetXmlNodeValueAsNullableLong("@D");
            trigger.DataLogThresholdMinimum = node.GetXmlNodeValueAsNullableLong("@E");
            trigger.DataLogThresholdMaximum = node.GetXmlNodeValueAsNullableLong("@F");
            trigger.DataLogThresholdChange = node.GetXmlNodeValueAsNullableLong("@G");
            trigger.DeviceElementIdRef = node.GetXmlNodeValue("@H");
            trigger.ValuePresentationIdRef = node.GetXmlNodeValue("@I");
            trigger.DataLogPGN = node.GetXmlNodeValueAsNullableLong("@J");
            trigger.DataLogPGNStartBit = node.GetXmlNodeValueAsNullableByte("@K");
            trigger.DataLogPGNStopBit = node.GetXmlNodeValueAsNullableByte("@L");
            return trigger;
        }

        public static IEnumerable<ISODataLogTrigger> ReadXML(XmlNodeList nodes)
        {
            List<ISODataLogTrigger> items = new List<ISODataLogTrigger>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODataLogTrigger.ReadXML(node));
            }
            return items;
        }
    }
}