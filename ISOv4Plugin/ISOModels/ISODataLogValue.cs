/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODataLogValue : ISOElement
    {
        //Attributes
        public string ProcessDataDDI { get; set; }
        public int? ProcessDataValue { get; set; }
        public string DeviceElementIdRef { get; set; }
        public uint? DataLogPGN { get; set; }
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
            base.WriteXML(xmlBuilder);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISODataLogValue ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISODataLogValue dlv = new ISODataLogValue();
            dlv.ProcessDataDDI = node.GetXmlNodeValue("@A");
            dlv.ProcessDataValue = node.GetXmlNodeValueAsNullableInt("@B");
            dlv.DeviceElementIdRef = node.GetXmlNodeValue("@C");
            dlv.DataLogPGN = node.GetXmlNodeValueAsNullableUInt("@D");
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

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.ProcessDataDDI, 4, errors, "A"); //DDI validation could be improved upon
            if (ProcessDataValue.HasValue) ValidateRange(this, x => x.ProcessDataValue.Value, Int32.MinValue, Int32.MaxValue-1, errors, "B");//Value may be empty in the timelog header
            RequireString(this, x => x.DeviceElementIdRef, 14, errors, "C");
            if (DataLogPGN.HasValue) ValidateRange<ISODataLogValue, uint>(this, x => x.DataLogPGN.Value, 0, (uint)(Math.Pow(2,18) - 1), errors, "D");
            if (DataLogPGNStartBit.HasValue) ValidateRange<ISODataLogValue, byte>(this, x => x.DataLogPGNStartBit.Value, 0, 63, errors, "E");
            if (DataLogPGNStopBit.HasValue) ValidateRange<ISODataLogValue, byte>(this, x => x.DataLogPGNStopBit.Value, 0, 63, errors, "F");
            return errors;
        }
    }
}
