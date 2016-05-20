using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class LinkListLoader
    {
        private string _baseFolder;
        private Dictionary<string, List<UniqueId>> _linkedIds;
        private XmlNode _rootNode;
        private TaskDataDocument _taskDocument;

        private LinkListLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _linkedIds = new Dictionary<string, List<UniqueId>>(StringComparer.OrdinalIgnoreCase);
        }

        public static Dictionary<string, List<UniqueId>> Load(TaskDataDocument taskDocument)
        {
            var loader = new LinkListLoader(taskDocument);

            return loader.Load();
        }

        private Dictionary<string, List<UniqueId>> Load()
        {
            if (!FindLinkedListNode())
                return _linkedIds;

            var rootNode = LoadXmlFile();
            if (rootNode == null)
                return _linkedIds;
            if (!VerifyRootNode(rootNode))
                return _linkedIds;

            var linkGroupNodes = rootNode.SelectNodes("LGP");
            return LinkGroupLoader.Load(linkGroupNodes);
        }

        private bool FindLinkedListNode()
        {
            var inputNodes = _rootNode.SelectNodes("AFE");
            foreach (XmlNode inputNode in inputNodes)
            {
                var fileType = inputNode.GetXmlNodeValue("@D");
                if (!string.Equals(fileType, "1", StringComparison.OrdinalIgnoreCase))
                    continue;

                var fileName = inputNode.GetXmlNodeValue("@A");
                if (string.Equals(fileName, "LINKLIST.XML", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool VerifyRootNode(XmlNode rootNode)
        {
            var majorVersion = rootNode.GetXmlNodeValue("@VersionMajor");
            IsoVersionEnum isoVersion;
            if (majorVersion == null || Enum.TryParse(majorVersion, true, out isoVersion) == false ||
                isoVersion != IsoVersionEnum.Standard_V2_Final_Draft)
                return false;

            return true;
        }

        private XmlNode LoadXmlFile()
        {
            var linkListFilename = Path.Combine(_baseFolder, "LINKLIST.XML");
            try
            {
                var linkListDocument = new XmlDocument();
                linkListDocument.Load(linkListFilename);

                return linkListDocument.SelectSingleNode("ISO11783_LinkList");
            }
            catch (XmlException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}