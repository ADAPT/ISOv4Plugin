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
    public class ISO11783_TaskData : ISOElement
    {
        public ISO11783_TaskData()
        {
            ChildElements = new List<ISOElement>();
        }

        public List<ISOElement> ChildElements { get; set; }
        public ISO11783_LinkList LinkList { get; set; }
        public string DataFolder { get; set; }
        public string FilePath { get; set; }

        public int VersionMajor { get; set;}
        public int VersionMinor { get; set;}
        public string ManagementSoftwareManufacturer { get; set; }
        public string ManagementSoftwareVersion { get; set; }
        public string TaskControllerManufacturer { get; set; }
        public string TaskControllerVersion { get; set; }
        public ISOTaskDataTransferOrigin DataTransferOrigin { get { return (ISOTaskDataTransferOrigin)DataTransferOriginInt; } set { DataTransferOriginInt = (int)value; } }
        private int DataTransferOriginInt { get; set; }
        public string DataTransferLanguage { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("ISO11783_TaskData");
            xmlBuilder.WriteXmlAttribute<int>("VersionMajor", VersionMajor);
            xmlBuilder.WriteXmlAttribute<int>("VersionMinor", VersionMinor);
            xmlBuilder.WriteAttributeString("ManagementSoftwareManufacturer", ManagementSoftwareManufacturer ?? string.Empty);
            xmlBuilder.WriteAttributeString("ManagementSoftwareVersion", ManagementSoftwareVersion ?? string.Empty);
            xmlBuilder.WriteAttributeString("TaskControllerManufacturer", TaskControllerManufacturer ?? string.Empty);
            xmlBuilder.WriteAttributeString("TaskControllerVersion", TaskControllerVersion ?? string.Empty);
            xmlBuilder.WriteXmlAttribute("DataTransferOrigin", ((int)DataTransferOrigin).ToString());
            if (DataTransferLanguage != null)
            {
                xmlBuilder.WriteAttributeString("DataTransferLanguage", DataTransferLanguage);
            }
            base.WriteXML(xmlBuilder);

            if (ChildElements != null)
            {
                foreach (var item in ChildElements)
                {
                    item.WriteXML(xmlBuilder);
                }
            }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISO11783_TaskData ReadXML(XmlNode taskDataNode, string baseFolder)
        {
            ISO11783_TaskData taskData = new ISO11783_TaskData();

            //Attributes
            taskData.VersionMajor = taskDataNode.GetXmlNodeValueAsInt("@VersionMajor");
            taskData.VersionMinor = taskDataNode.GetXmlNodeValueAsInt("@VersionMinor");
            taskData.ManagementSoftwareManufacturer = taskDataNode.GetXmlNodeValue("@ManagementSoftwareManufacturer");
            taskData.ManagementSoftwareVersion = taskDataNode.GetXmlNodeValue("@ManagementSoftwareVersion");
            taskData.TaskControllerManufacturer = taskDataNode.GetXmlNodeValue("@TaskControllerManufacturer");
            taskData.TaskControllerVersion = taskDataNode.GetXmlNodeValue("@TaskControllerVersion");
            taskData.DataTransferOriginInt = taskDataNode.GetXmlNodeValueAsInt("@DataTransferOrigin");
            taskData.DataTransferLanguage = taskDataNode.GetXmlNodeValue("@DataTransferLanguage");

            //--------------
            //Child Elements
            //--------------
            //Attached Files
            XmlNodeList afeNodes = taskDataNode.SelectNodes("AFE");
            if (afeNodes != null)
            {
                taskData.ChildElements.AddRange(ISOAttachedFile.ReadXML(afeNodes));
            }
            ProcessExternalNodes(taskDataNode, "AFE", baseFolder, taskData, ISOAttachedFile.ReadXML);

            //Coded Comments
            XmlNodeList cctNodes = taskDataNode.SelectNodes("CCT");
            if (cctNodes != null)
            {
                taskData.ChildElements.AddRange(ISOCodedComment.ReadXML(cctNodes));
            }
            ProcessExternalNodes(taskDataNode, "CCT", baseFolder, taskData, ISOCodedComment.ReadXML);

            //Crop Types
            XmlNodeList ctpNodes = taskDataNode.SelectNodes("CTP");
            if (ctpNodes != null)
            {
                taskData.ChildElements.AddRange(ISOCropType.ReadXML(ctpNodes));
            }
            ProcessExternalNodes(taskDataNode, "CTP", baseFolder, taskData, ISOCropType.ReadXML);

            //Cultural Practices
            XmlNodeList cpcNodes = taskDataNode.SelectNodes("CPC");
            if (cpcNodes != null)
            {
                taskData.ChildElements.AddRange(ISOCulturalPractice.ReadXML(cpcNodes));
            }
            ProcessExternalNodes(taskDataNode, "CPC", baseFolder, taskData, ISOCulturalPractice.ReadXML);

            //Customers
            XmlNodeList ctrNodes = taskDataNode.SelectNodes("CTR");
            if (ctrNodes != null)
            {
                taskData.ChildElements.AddRange(ISOCustomer.ReadXML(ctrNodes));
            }
            ProcessExternalNodes(taskDataNode, "CTR", baseFolder, taskData, ISOCustomer.ReadXML);

            //Devices
            XmlNodeList dvcNodes = taskDataNode.SelectNodes("DVC");
            if (dvcNodes != null)
            {
                taskData.ChildElements.AddRange(ISODevice.ReadXML(dvcNodes));
            }
            ProcessExternalNodes(taskDataNode, "DVC", baseFolder, taskData, ISODevice.ReadXML);

            //Farms
            XmlNodeList frmNodes = taskDataNode.SelectNodes("FRM");
            if (frmNodes != null)
            {
                taskData.ChildElements.AddRange(ISOFarm.ReadXML(frmNodes));
            }
            ProcessExternalNodes(taskDataNode, "FRM", baseFolder, taskData, ISOFarm.ReadXML);

            //Operation Techniques
            XmlNodeList otqNodes = taskDataNode.SelectNodes("OTQ");
            if (otqNodes != null)
            {
                taskData.ChildElements.AddRange(ISOOperationTechnique.ReadXML(otqNodes));
            }
            ProcessExternalNodes(taskDataNode, "OTQ", baseFolder, taskData, ISOOperationTechnique.ReadXML);

            //Partfields
            XmlNodeList pfdNodes = taskDataNode.SelectNodes("PFD");
            if (pfdNodes != null)
            {
                taskData.ChildElements.AddRange(ISOPartfield.ReadXML(pfdNodes));
            }
            ProcessExternalNodes(taskDataNode, "PFD", baseFolder, taskData, ISOPartfield.ReadXML);

            //Products
            XmlNodeList pdtNodes = taskDataNode.SelectNodes("PDT");
            if (pdtNodes != null)
            {
                taskData.ChildElements.AddRange(ISOProduct.ReadXML(pdtNodes));
            }
            ProcessExternalNodes(taskDataNode, "PDT", baseFolder, taskData, ISOProduct.ReadXML);

            //Product Groups
            XmlNodeList pgpNodes = taskDataNode.SelectNodes("PGP");
            if (pgpNodes != null)
            {
                taskData.ChildElements.AddRange(ISOProductGroup.ReadXML(pgpNodes));
            }
            ProcessExternalNodes(taskDataNode, "PGP", baseFolder, taskData, ISOProductGroup.ReadXML);

            //Task Controller Capabilities
            XmlNodeList tccNodes = taskDataNode.SelectNodes("TCC");
            if (tccNodes != null)
            {
                taskData.ChildElements.AddRange(ISOTaskControllerCapabilities.ReadXML(tccNodes));
            }
            ProcessExternalNodes(taskDataNode, "TCC", baseFolder, taskData, ISOTaskControllerCapabilities.ReadXML);

            //Tasks
            XmlNodeList tskNodes = taskDataNode.SelectNodes("TSK");
            if (tskNodes != null)
            {
                taskData.ChildElements.AddRange(ISOTask.ReadXML(tskNodes));
            }
            ProcessExternalNodes(taskDataNode, "TSK", baseFolder, taskData, ISOTask.ReadXML);

            //Value Presentations
            XmlNodeList vpnNodes = taskDataNode.SelectNodes("VPN");
            if (vpnNodes != null)
            {
                taskData.ChildElements.AddRange(ISOValuePresentation.ReadXML(vpnNodes));
            }
            ProcessExternalNodes(taskDataNode, "VPN", baseFolder, taskData, ISOValuePresentation.ReadXML);

            //Workers
            XmlNodeList wkrNodes = taskDataNode.SelectNodes("WKR");
            if (wkrNodes != null)
            {
                taskData.ChildElements.AddRange(ISOWorker.ReadXML(wkrNodes));
            }
            ProcessExternalNodes(taskDataNode, "WKR", baseFolder, taskData, ISOWorker.ReadXML);

            //LinkList
            ISOAttachedFile linkListFile = taskData.ChildElements.OfType<ISOAttachedFile>().SingleOrDefault(afe => afe.FileType == 1);
            if (linkListFile != null)
            {
                XmlDocument linkDocument = new XmlDocument();
                string linkPath = Path.Combine(baseFolder, linkListFile.FilenamewithExtension);
                if (File.Exists(linkPath))
                {
                    linkDocument.Load(linkPath);
                    XmlNode linkRoot = linkDocument.SelectSingleNode("ISO11783LinkList");
                    taskData.LinkList = ISO11783_LinkList.ReadXML(linkRoot, baseFolder);
                }
            }

            return taskData;

        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireRange(this, x => x.VersionMajor, 0, 4, errors);
            RequireRange(this, x => x.VersionMinor, 0, 99, errors);
            ValidateString(this, x => x.ManagementSoftwareManufacturer, 32, errors);
            ValidateString(this, x => x.ManagementSoftwareVersion, 32, errors);
            ValidateString(this, x => x.TaskControllerManufacturer, 32, errors);
            ValidateString(this, x => x.TaskControllerVersion, 32, errors);
            ValidateEnumerationValue(typeof(ISOTaskDataTransferOrigin), DataTransferOriginInt, errors);
            ChildElements.ForEach(i => i.Validate(errors));
            if (LinkList != null)  LinkList.Validate(errors);

            return errors;
        }

        private static void ProcessExternalNodes(XmlNode node, string xmlPrefix, string baseFolder, ISO11783_TaskData taskData, Func<XmlNodeList, IEnumerable<ISOElement>> readDelegate)
        {
            var externalNodes = node.SelectNodes($"XFR[starts-with(@A, '{xmlPrefix}')]");
            for (int i = 0; i < externalNodes.Count; i++)
            {
                var inputNodes = externalNodes[i].LoadActualNodes("XFR", baseFolder);
                if (inputNodes != null)
                {
                    taskData.ChildElements.AddRange(readDelegate(inputNodes));
                }
            }
        }
    }
}
