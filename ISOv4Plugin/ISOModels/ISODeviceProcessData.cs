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
using System.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODeviceProcessData : ISOElement
    {
        //Attributes
        public uint ObjectID { get; set; }

        private string _ddi;
        public string DDI
        {
            get => _ddi;
            set
            {
                _ddi = value;
                _int32Ddi = -1;
            }
        }

        private int _int32Ddi = -1;
        public int Int32DDI
        {
            get
            {
                if (_int32Ddi == -1)
                {
                    _int32Ddi = DDI.AsInt32DDI();
                }
                return _int32Ddi;
            }
        }

        public int Property  { get; set; }
        public int TriggerMethods { get; set; }
        public string Designator { get; set; }
        public uint? DeviceValuePresentationObjectId { get; set; }

        public ISODeviceValuePresentation DeviceValuePresentation { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DPD");
            xmlBuilder.WriteXmlAttribute<uint>("A", ObjectID);
            xmlBuilder.WriteXmlAttribute("B", DDI );
            xmlBuilder.WriteXmlAttribute<int>("C", Property);
            xmlBuilder.WriteXmlAttribute<int>("D", TriggerMethods);
            xmlBuilder.WriteXmlAttribute("E", Designator);
            xmlBuilder.WriteXmlAttribute<uint>("F", DeviceValuePresentationObjectId);
            base.WriteXML(xmlBuilder);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceProcessData ReadXML(XmlNode node, List<ISODeviceValuePresentation> deviceValuePresentations)
        {
            ISODeviceProcessData item = new ISODeviceProcessData();
            item.ObjectID = node.GetXmlNodeValueAsUInt("@A");
            item.DDI  = node.GetXmlNodeValue("@B");
            item.Property  = node.GetXmlNodeValueAsInt("@C");
            item.TriggerMethods = node.GetXmlNodeValueAsInt("@D");
            item.Designator = node.GetXmlNodeValue("@E");
            item.DeviceValuePresentationObjectId = node.GetXmlNodeValueAsNullableUInt("@F");
            if (item.DeviceValuePresentationObjectId.HasValue)
            {
                item.DeviceValuePresentation = deviceValuePresentations.FirstOrDefault(d => d.ObjectID == item.DeviceValuePresentationObjectId.Value);
            }

            return item;
        }

        public static IEnumerable<ISODeviceProcessData> ReadXML(XmlNodeList nodes, List<ISODeviceValuePresentation> deviceValuePresentations)
        {
            List<ISODeviceProcessData> items = new List<ISODeviceProcessData>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceProcessData.ReadXML(node, deviceValuePresentations));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireRange<ISODeviceProcessData, uint>(this, x => x.ObjectID, 1, 65534, errors, "A");
            RequireString(this, x => x.DDI, 4, errors, "B"); //DDI validation could be improved upon
            RequireRange(this, x => x.Property, 0, 7, errors, "C");
            RequireRange(this, x => x.TriggerMethods, 0, 31, errors, "D");
            ValidateString(this, x => x.Designator, 32, errors, "E");
            if (DeviceValuePresentationObjectId.HasValue) ValidateRange<ISODeviceProcessData, uint>(this, x => x.DeviceValuePresentationObjectId.Value, 1, 65534, errors, "F");
            return errors;
        }
    }
}
