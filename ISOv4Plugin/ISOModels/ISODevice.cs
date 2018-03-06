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
    public class ISODevice : ISOElement
    {
        public ISODevice()
        {
            DeviceElements = new List<ISODeviceElement>();
            DeviceProcessDatas = new List<ISODeviceProcessData>();
            DeviceProperties = new List<ISODeviceProperty>();
            DeviceValuePresentations = new List<ISODeviceValuePresentation>();
        }

        //Attributes
        public string DeviceId { get; set; }
        public string DeviceDesignator { get; set; }
        public string DeviceSoftwareVersion { get; set; }
        public string ClientNAME { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceStructureLabel { get; set; }
        public string DeviceLocalizationLabel { get; set; }

        //Child Elements
        public List<ISODeviceElement> DeviceElements { get; set; }
        public List<ISODeviceProcessData> DeviceProcessDatas { get; set; }
        public List<ISODeviceProperty> DeviceProperties { get; set; }
        public List<ISODeviceValuePresentation> DeviceValuePresentations { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DVC");
            xmlBuilder.WriteXmlAttribute("A", DeviceId);
            xmlBuilder.WriteXmlAttribute("B", DeviceDesignator);
            xmlBuilder.WriteXmlAttribute("C", DeviceSoftwareVersion);
            xmlBuilder.WriteXmlAttribute("D", ClientNAME);
            xmlBuilder.WriteXmlAttribute("E", DeviceSerialNumber);
            xmlBuilder.WriteXmlAttribute("F", DeviceStructureLabel);
            xmlBuilder.WriteXmlAttribute("G", DeviceLocalizationLabel);
            foreach (ISODeviceElement item in DeviceElements) { item.WriteXML(xmlBuilder); }
            foreach (ISODeviceProcessData item in DeviceProcessDatas) { item.WriteXML(xmlBuilder); }
            foreach (ISODeviceProperty item in DeviceProperties) { item.WriteXML(xmlBuilder); }
            foreach (ISODeviceValuePresentation item in DeviceValuePresentations) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISODevice ReadXML(XmlNode node)
        {
            ISODevice device = new ISODevice();
            device.DeviceId = node.GetXmlNodeValue("@A");
            device.DeviceDesignator = node.GetXmlNodeValue("@B");
            device.DeviceSoftwareVersion = node.GetXmlNodeValue("@C");
            device.ClientNAME = node.GetXmlNodeValue("@D");
            device.DeviceSerialNumber = node.GetXmlNodeValue("@E");
            device.DeviceStructureLabel = node.GetXmlNodeValue("@F");
            device.DeviceLocalizationLabel = node.GetXmlNodeValue("@G");

            XmlNodeList detNodes = node.SelectNodes("DET");
            if (detNodes != null)
            {
                device.DeviceElements.AddRange(ISODeviceElement.ReadXML(detNodes, device));
            }

            XmlNodeList dpdNodes = node.SelectNodes("DPD");
            if (dpdNodes != null)
            {
                device.DeviceProcessDatas.AddRange(ISODeviceProcessData.ReadXML(dpdNodes));
            }

            XmlNodeList dptNodes = node.SelectNodes("DPT");
            if (dptNodes != null)
            {
                device.DeviceProperties.AddRange(ISODeviceProperty.ReadXML(dptNodes));
            }

            XmlNodeList dvpNodes = node.SelectNodes("DVP");
            if (dvpNodes != null)
            {
                device.DeviceValuePresentations.AddRange(ISODeviceValuePresentation.ReadXML(dvpNodes));
            }

            return device;
        }

        public static IEnumerable<ISODevice> ReadXML(XmlNodeList dvcNodes)
        {
            List<ISODevice> items = new List<ISODevice>();
            foreach (XmlNode dvcNode in dvcNodes)
            {
                items.Add(ISODevice.ReadXML(dvcNode));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.DeviceId, 14, errors, "A");
            ValidateString(this, x => x.DeviceDesignator, 32, errors, "B");
            ValidateString(this, x => x.DeviceSoftwareVersion, 32, errors, "C");
            RequireString(this, x => x.ClientNAME, 16, errors, "D");//Hex validation could be improved upon
            ValidateString(this, x => x.DeviceSerialNumber, 32, errors, "E");
            RequireString(this, x => x.DeviceStructureLabel, 32, errors, "F");//Hex validation could be improved upon
            RequireString(this, x => x.DeviceLocalizationLabel, 32, errors, "G");//Hex validation could be improved upon
            DeviceElements.ForEach(i => i.Validate(errors));
            DeviceProcessDatas.ForEach(i => i.Validate(errors));
            DeviceProperties.ForEach(i => i.Validate(errors));
            DeviceValuePresentations.ForEach(i => i.Validate(errors));

            return errors;
        }
    }
}
