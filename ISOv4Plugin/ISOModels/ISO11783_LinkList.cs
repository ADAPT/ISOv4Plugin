/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISO11783_LinkList : ISOElement
    {
        public ISO11783_LinkList()
        {
            LinkGroups = new List<ISOLinkGroup>();
        }

        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public string ManagementSoftwareManufacturer { get; set; }
        public string ManagementSoftwareVersion { get; set; }
        public string TaskControllerManufacturer { get; set; }
        public string TaskControllerVersion { get; set; }
        public string FileVersion { get; set; }
        public ISOTaskDataTransferOrigin DataTransferOrigin { get { return (ISOTaskDataTransferOrigin)DataTransferOriginInt; } set { DataTransferOriginInt = (int)value; } }
        private int DataTransferOriginInt {get; set;}
        public List<ISOLinkGroup> LinkGroups { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("ISO11783LinkList");
            xmlBuilder.WriteXmlAttribute<int>("VersionMajor", VersionMajor);
            xmlBuilder.WriteXmlAttribute<int>("VersionMinor", VersionMinor);
            xmlBuilder.WriteAttributeString("ManagementSoftwareManufacturer", ManagementSoftwareManufacturer ?? string.Empty);
            xmlBuilder.WriteAttributeString("ManagementSoftwareVersion", ManagementSoftwareVersion ?? string.Empty);
            xmlBuilder.WriteAttributeString("TaskControllerManufacturer", TaskControllerManufacturer ?? string.Empty);
            xmlBuilder.WriteAttributeString("TaskControllerVersion", TaskControllerVersion ?? string.Empty);
            xmlBuilder.WriteAttributeString("DataTransferOrigin", ((int)DataTransferOrigin).ToString() ?? string.Empty);
            xmlBuilder.WriteAttributeString("FileVersion", FileVersion ?? string.Empty);
            base.WriteXML(xmlBuilder);
            foreach (ISOLinkGroup item in LinkGroups) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISO11783_LinkList ReadXML(XmlNode linkListNode, string baseFolder)
        {
            ISO11783_LinkList linkList = new ISO11783_LinkList();

            linkList.VersionMajor = linkListNode.GetXmlNodeValueAsInt("@VersionMajor");
            linkList.VersionMinor = linkListNode.GetXmlNodeValueAsInt("@VersionMinor");
            linkList.ManagementSoftwareManufacturer = linkListNode.GetXmlNodeValue("@ManagementSoftwareManufacturer") ?? string.Empty;
            linkList.ManagementSoftwareVersion = linkListNode.GetXmlNodeValue("@ManagementSoftwareVersion") ?? string.Empty;
            linkList.TaskControllerManufacturer = linkListNode.GetXmlNodeValue("@TaskControllerManufacturer") ?? string.Empty;
            linkList.TaskControllerVersion = linkListNode.GetXmlNodeValue("@TaskControllerVersion") ?? string.Empty;
            linkList.DataTransferOriginInt = linkListNode.GetXmlNodeValueAsInt("@DataTransferOrigin");
            linkList.FileVersion = linkListNode.GetXmlNodeValue("@FileVersion") ?? string.Empty;

            XmlNodeList lgpNodes = linkListNode.SelectNodes("LGP");
            if (lgpNodes != null)
            {
                linkList.LinkGroups.AddRange(ISOLinkGroup.ReadXML(lgpNodes));
            }
            
            return linkList;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireRange(this, x => x.VersionMajor, 4, 99, errors);
            RequireRange(this, x => x.VersionMinor, 0, 99, errors);
            RequireString(this, x => x.ManagementSoftwareManufacturer, 32, errors);
            RequireString(this, x => x.ManagementSoftwareVersion, 32, errors);
            ValidateString(this, x => x.TaskControllerManufacturer, 32, errors);
            ValidateString(this, x => x.TaskControllerVersion, 32, errors);
            ValidateEnumerationValue(typeof(ISOTaskDataTransferOrigin), DataTransferOriginInt, errors);
            RequireNonZeroCount(LinkGroups, "LGP", errors);
            LinkGroups.ForEach(g => g.Validate(errors));

            return errors;
        }
    }
}
