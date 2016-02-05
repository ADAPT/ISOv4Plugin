using AgGateway.ADAPT.ApplicationDataModel.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class LinkListLoader
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

        internal static Dictionary<string, List<UniqueId>> Load(TaskDataDocument taskDocument)
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

            LoadLinkedIds(rootNode.SelectNodes("LGP"));

            return _linkedIds;
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

        private void LoadLinkedIds(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                LoadLinkedGroup(inputNode);
            }
        }

        private void LoadLinkedGroup(XmlNode inputNode)
        {
            var groupId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(groupId))
                return;

            var groupTypeValue = inputNode.GetXmlNodeValue("@B");
            if (string.IsNullOrEmpty(groupTypeValue))
                return;

            var manufacturerGln = inputNode.GetXmlNodeValue("@C");
            var groupNamespace = inputNode.GetXmlNodeValue("@D");
            if (groupTypeValue.Equals("2", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(manufacturerGln))
                return;

            LoadLinks(inputNode.SelectNodes("LNK"), groupTypeValue, manufacturerGln, groupNamespace);
        }

        private void LoadLinks(XmlNodeList inputNodes, string groupTypeValue, string manufacturerGln, string groupNamespace)
        {
            foreach (XmlNode linkNode in inputNodes)
            {
                string linkId;
                var uniqueId = LoadLink(linkNode, out linkId);
                if (uniqueId != null)
                {
                    UpdateId(uniqueId, groupTypeValue, manufacturerGln, groupNamespace);

                    List<UniqueId> ids;
                    if (!_linkedIds.TryGetValue(linkId, out ids))
                    {
                        ids = new List<UniqueId>();
                        _linkedIds[linkId] = ids;
                    }
                    ids.Add(uniqueId);
                }
            }
        }

        private static UniqueId LoadLink(XmlNode inputNode, out string linkId)
        {
            linkId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(linkId))
                return null;

            var linkValue = inputNode.GetXmlNodeValue("@B");
            if (string.IsNullOrEmpty(linkValue))
                return null;

            var uniqueId = new UniqueId
            {
                Id = linkValue
            };

            return uniqueId;
        }

        private static void UpdateId(UniqueId uniqueId, string groupTypeValue, string manufacturerGln, string groupNamespace)
        {
            switch (groupTypeValue)
            {
                case "1":
                    uniqueId.CiTypeEnum = CompoundIdentifierTypeEnum.UUID;
                    break;

                case "2":
                    uniqueId.CiTypeEnum = CompoundIdentifierTypeEnum.String;
                    uniqueId.SourceType = IdSourceTypeEnum.GLN;
                    uniqueId.Source = manufacturerGln;
                    break;

                case "3":
                    uniqueId.CiTypeEnum = CompoundIdentifierTypeEnum.String;
                    uniqueId.SourceType = IdSourceTypeEnum.URI;
                    uniqueId.Source = groupNamespace;
                    uniqueId.Id = string.Concat(groupNamespace, uniqueId.Id);
                    break;

                case "4":
                    uniqueId.CiTypeEnum = CompoundIdentifierTypeEnum.String;
                    uniqueId.SourceType = IdSourceTypeEnum.URI;
                    uniqueId.Id = string.Concat(groupNamespace, uniqueId.Id);
                    break;
            }
        }
    }
}