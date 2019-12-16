/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODataLogTrigger : ISOElement
    {
        //Attributes
        public string DataLogDDI { get; set; }
        public byte DataLogMethod { get; set; }
        public int? DataLogDistanceInterval { get; set; }
        public int? DataLogTimeInterval { get; set; }
        public int? DataLogThresholdMinimum { get; set; }
        public int? DataLogThresholdMaximum { get; set; }
        public int? DataLogThresholdChange { get; set; }
        public string DeviceElementIdRef { get; set; }
        public string ValuePresentationIdRef { get; set; }
        public uint? DataLogPGN { get; set; }
        public byte? DataLogPGNStartBit { get; set; }
        public byte? DataLogPGNStopBit { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DLT");
            xmlBuilder.WriteXmlAttribute("A", DataLogDDI);
            xmlBuilder.WriteXmlAttribute<byte>("B", DataLogMethod);
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
            base.WriteXML(xmlBuilder);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODataLogTrigger ReadXML(XmlNode node)
        {
            ISODataLogTrigger trigger = new ISODataLogTrigger();
            trigger.DataLogDDI = node.GetXmlNodeValue("@A");
            trigger.DataLogMethod = node.GetXmlNodeValueAsByte("@B");
            trigger.DataLogDistanceInterval = node.GetXmlNodeValueAsNullableInt("@C");
            trigger.DataLogTimeInterval = node.GetXmlNodeValueAsNullableInt("@D");
            trigger.DataLogThresholdMinimum = node.GetXmlNodeValueAsNullableInt("@E");
            trigger.DataLogThresholdMaximum = node.GetXmlNodeValueAsNullableInt("@F");
            trigger.DataLogThresholdChange = node.GetXmlNodeValueAsNullableInt("@G");
            trigger.DeviceElementIdRef = node.GetXmlNodeValue("@H");
            trigger.ValuePresentationIdRef = node.GetXmlNodeValue("@I");
            trigger.DataLogPGN = node.GetXmlNodeValueAsNullableUInt("@J");
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

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.DataLogDDI, 4, errors, "A"); //DDI validation could be improved upon
            RequireRange<ISODataLogTrigger, byte>(this, x => x.DataLogMethod, 1, 31, errors, "B");
            if (DataLogDistanceInterval.HasValue) ValidateRange(this, x => x.DataLogDistanceInterval.Value, 0, 1000000, errors, "C");
            if (DataLogTimeInterval.HasValue) ValidateRange(this, x => x.DataLogTimeInterval.Value, 0, 60000, errors, "D");
            if (DataLogThresholdMinimum.HasValue) ValidateRange(this, x => x.DataLogThresholdMinimum.Value, Int32.MinValue, Int32.MaxValue -1, errors, "E");
            if (DataLogThresholdMaximum.HasValue) ValidateRange(this, x => x.DataLogThresholdMaximum.Value, Int32.MinValue, Int32.MaxValue - 1, errors, "F");
            if (DataLogThresholdChange.HasValue) ValidateRange(this, x => x.DataLogThresholdChange.Value, Int32.MinValue, Int32.MaxValue - 1, errors, "G");
            if (DeviceElementIdRef != null) ValidateString(this, x => x.DeviceElementIdRef, 14, errors, "H");
            if (ValuePresentationIdRef != null) ValidateString(this, x => x.ValuePresentationIdRef, 14, errors, "I");
            if (DataLogPGN.HasValue) ValidateRange<ISODataLogTrigger, uint>(this, x => x.DataLogPGN.Value, 0, (uint)(Math.Pow(2,18) - 1), errors, "J");
            if (DataLogPGNStartBit.HasValue) ValidateRange<ISODataLogTrigger, byte>(this, x => x.DataLogPGNStartBit.Value, 0, 63, errors, "K");
            if (DataLogPGNStopBit.HasValue) ValidateRange<ISODataLogTrigger, byte>(this, x => x.DataLogPGNStopBit.Value, 0, 63, errors, "L");
            return errors;
        }
    }
}
