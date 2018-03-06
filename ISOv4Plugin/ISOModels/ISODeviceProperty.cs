/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODeviceProperty : ISOElement
    {
        //Attributes
        public uint ObjectID { get; set; }
        public string DDI  { get; set; }
        public int Value  { get; set; }
        public string Designator { get; set; }
        public uint? DeviceValuePresentationObjectId { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DPT");
            xmlBuilder.WriteXmlAttribute<uint>("A", ObjectID);
            xmlBuilder.WriteXmlAttribute("B", DDI );
            xmlBuilder.WriteXmlAttribute<int>("C", Value);
            xmlBuilder.WriteXmlAttribute("D", Designator);
            xmlBuilder.WriteXmlAttribute<uint>("E", DeviceValuePresentationObjectId);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceProperty ReadXML(XmlNode node)
        {
            ISODeviceProperty item = new ISODeviceProperty();
            item.ObjectID = node.GetXmlNodeValueAsUInt("@A");
            item.DDI  = node.GetXmlNodeValue("@B");
            item.Value = node.GetXmlNodeValueAsInt("@C");
            item.Designator = node.GetXmlNodeValue("@D");
            item.DeviceValuePresentationObjectId = node.GetXmlNodeValueAsNullableUInt("@E");

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

        public override List<IError> Validate(List<IError> errors)
        {
            RequireRange<ISODeviceProperty, uint>(this, x => x.ObjectID, 1, 65534, errors, "A");
            RequireString(this, x => x.DDI, 4, errors, "B"); //DDI validation could be improved upon
            RequireRange(this, x => x.Value, Int32.MinValue, Int32.MaxValue -1, errors, "C");
            ValidateString(this, x => x.Designator, 32, errors, "D");
            if (DeviceValuePresentationObjectId.HasValue) ValidateRange<ISODeviceProperty, uint>(this, x => x.DeviceValuePresentationObjectId.Value, 1, 65534, errors, "E");
            return errors;
        }
    }
}
