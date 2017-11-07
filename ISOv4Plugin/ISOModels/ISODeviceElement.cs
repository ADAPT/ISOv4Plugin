/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Linq;

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
        public int DeviceElementObjectId  { get; set; }
        public ISODeviceElementType DeviceElementType  { get; set; }
        public string DeviceElementDesignator { get; set; }
        public int DeviceElementNumber { get; set; }
        public int ParentObjectId { get; set; }

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
            xmlBuilder.WriteXmlAttribute<int>("B", DeviceElementObjectId );
            xmlBuilder.WriteXmlAttribute("C", ((int)DeviceElementType).ToString());
            xmlBuilder.WriteXmlAttribute("D", DeviceElementDesignator);
            xmlBuilder.WriteXmlAttribute<int>("E", DeviceElementNumber);
            xmlBuilder.WriteXmlAttribute<int>("F", ParentObjectId);
            foreach (ISODeviceObjectReference item in DeviceObjectReferences) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceElement ReadXML(XmlNode node, ISODevice device)
        {
            ISODeviceElement item = new ISODeviceElement(device);
            item.DeviceElementId  = node.GetXmlNodeValue("@A");
            item.DeviceElementObjectId  = node.GetXmlNodeValueAsInt("@B");
            item.DeviceElementType  = (ISODeviceElementType)(node.GetXmlNodeValueAsInt("@C"));
            item.DeviceElementDesignator = node.GetXmlNodeValue("@D");
            item.DeviceElementNumber = node.GetXmlNodeValueAsInt("@E");
            item.ParentObjectId = node.GetXmlNodeValueAsInt("@F");
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
    }
}