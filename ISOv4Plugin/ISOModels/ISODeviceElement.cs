/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODeviceElement : ISOElement
    {
        public ISODeviceElement(ISODevice device)
        {
            DeviceObjectReferences = new List<ISODeviceObjectReference>();
            Device = device;
        }

        //Attributes
        public string DeviceElementId  { get; set; }
        public uint DeviceElementObjectId  { get; set; }
        private int DeviceElementTypeInt { get; set; }
        public ISODeviceElementType DeviceElementType  { get { return (ISODeviceElementType)DeviceElementTypeInt; }  set { DeviceElementTypeInt = (int)value; } }
        public string DeviceElementDesignator { get; set; }
        public uint DeviceElementNumber { get; set; }
        public uint ParentObjectId { get; set; }

        //Child Elements
        public List<ISODeviceObjectReference> DeviceObjectReferences {get; set;}

        public ISODevice Device { get; private set; }
        public ISOElement Parent { get { if (ParentObjectId == 0) return Device; else return Device.DeviceElements.Single(det => det.DeviceElementObjectId == ParentObjectId);  } }
        public IEnumerable<ISODeviceProcessData> DeviceProcessDatas { get { return Device.DeviceProcessDatas.Where(dpd => DeviceObjectReferences.Select(dor => dor.DeviceObjectId).Contains(dpd.ObjectID)); } }
        public IEnumerable<ISODeviceProperty> DeviceProperties { get { return Device.DeviceProperties.Where(dpt => DeviceObjectReferences.Select(dor => dor.DeviceObjectId).Contains(dpt.ObjectID)); } }
        public IEnumerable<ISODeviceElement> ChildDeviceElements { get { return Device.DeviceElements.Where(det => det.ParentObjectId == DeviceElementObjectId); } }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DET");
            xmlBuilder.WriteXmlAttribute("A", DeviceElementId );
            xmlBuilder.WriteXmlAttribute<uint>("B", DeviceElementObjectId );
            xmlBuilder.WriteXmlAttribute("C", ((int)DeviceElementType).ToString());
            xmlBuilder.WriteXmlAttribute("D", DeviceElementDesignator);
            xmlBuilder.WriteXmlAttribute<uint>("E", DeviceElementNumber);
            xmlBuilder.WriteXmlAttribute<uint>("F", ParentObjectId);
            base.WriteXML(xmlBuilder);
            foreach (ISODeviceObjectReference item in DeviceObjectReferences) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceElement ReadXML(XmlNode node, ISODevice device)
        {
            ISODeviceElement item = new ISODeviceElement(device);
            item.DeviceElementId  = node.GetXmlNodeValue("@A");
            item.DeviceElementObjectId  = node.GetXmlNodeValueAsUInt("@B");
            item.DeviceElementTypeInt  = node.GetXmlNodeValueAsInt("@C");
            item.DeviceElementDesignator = node.GetXmlNodeValue("@D");
            item.DeviceElementNumber = node.GetXmlNodeValueAsUInt("@E");
            item.ParentObjectId = node.GetXmlNodeValueAsUInt("@F");
            item.ProprietarySchemaExtensions = ReadProperietarySchemaExtensions(node);
            XmlNodeList dorNodes = node.SelectNodes("DOR");
            if (dorNodes != null)
            {
                item.DeviceObjectReferences.AddRange(ISODeviceObjectReference.ReadXML(dorNodes));
            }

            return item;
        }

        public static IEnumerable<ISODeviceElement> ReadXML(XmlNodeList nodes, ISODevice device)
        {
            List<ISODeviceElement> items = new List<ISODeviceElement>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceElement.ReadXML(node, device));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.DeviceElementId, 14, errors, "A");
            RequireRange<ISODeviceElement, uint>(this, x => x.DeviceElementObjectId, 1, 65534, errors, "B");
            ValidateEnumerationValue(typeof(ISODeviceElementType), DeviceElementTypeInt, errors);
            ValidateString(this, x => x.DeviceElementDesignator, 32, errors, "D");
            RequireRange<ISODeviceElement, uint>(this, x => x.DeviceElementNumber, 0, 4095, errors, "E");
            RequireRange<ISODeviceElement, uint>(this, x => x.ParentObjectId, 0, 65534, errors, "F");
            DeviceObjectReferences.ForEach(i => i.Validate(errors));

            return errors;
        }
    }
}
