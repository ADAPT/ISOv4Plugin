/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODataLogValue : ISOElement
    {
        //Attributes
        public string ProcessDataDDI { get; set; }
        public long? ProcessDataValue { get; set; }
        public string DeviceElementIdRef { get; set; }
        public long? DataLogPGN { get; set; }
        public byte? DataLogPGNStartBit { get; set; }
        public byte? DataLogPGNStopBit { get; set; }

        public int Order { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DLV");
            xmlBuilder.WriteXmlAttribute("A", ProcessDataDDI);

            //Custom behavior for ProcessDataValue to support TimeLog use with an empty B
            string value = ProcessDataValue.HasValue ? ProcessDataValue.ToString() : string.Empty;
            xmlBuilder.WriteAttributeString("B", value);

            xmlBuilder.WriteXmlAttribute("C", DeviceElementIdRef);
            xmlBuilder.WriteXmlAttribute("D", DataLogPGN);
            xmlBuilder.WriteXmlAttribute("E", DataLogPGNStartBit);
            xmlBuilder.WriteXmlAttribute("F", DataLogPGNStopBit);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISODataLogValue ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISODataLogValue dlv = new ISODataLogValue();
            dlv.ProcessDataDDI = node.GetXmlNodeValue("@A");
            dlv.ProcessDataValue = node.GetXmlNodeValueAsNullableLong("@B");
            dlv.DeviceElementIdRef = node.GetXmlNodeValue("@C");
            dlv.DataLogPGN = node.GetXmlNodeValueAsNullableLong("@D");
            dlv.DataLogPGNStartBit = node.GetXmlNodeValueAsNullableByte("@E");
            dlv.DataLogPGNStopBit = node.GetXmlNodeValueAsNullableByte("@F");

            return dlv;
        }

        public static List<ISODataLogValue> ReadXML(XmlNodeList nodes)
        {
            List<ISODataLogValue> items = new List<ISODataLogValue>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODataLogValue.ReadXML(node));
            }
            return items;
        }
    }
}