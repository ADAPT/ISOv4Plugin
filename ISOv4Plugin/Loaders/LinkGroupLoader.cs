using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class LinkGroupLoader
    {
        private static Dictionary<string, List<UniqueId>> _linkIds;

        public static Dictionary<string, List<UniqueId>> Load(XmlNodeList linkGroupNodes)
        {
            _linkIds = new Dictionary<string, List<UniqueId>>();
            foreach (XmlNode linkGroupNode in linkGroupNodes)
            {
                LoadLinkedGroup(linkGroupNode);
            }
            return _linkIds;
        }

        private static void LoadLinkedGroup(XmlNode inputNode)
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

            var linkNodes = inputNode.SelectNodes("LNK");
            LoadLinks(linkNodes, groupTypeValue, manufacturerGln, groupNamespace);
        }

        private static void LoadLinks(XmlNodeList linkNodes, string groupTypeValue, string manufacturerGln, string groupNamespace)
        {
            foreach (XmlNode linkNode in linkNodes)
            {
                string linkId;
                var uniqueId = LoadLink(linkNode, out linkId);
                if (uniqueId != null)
                {
                    UpdateId(uniqueId, groupTypeValue, manufacturerGln, groupNamespace);

                    List<UniqueId> ids;
                    if (!_linkIds.TryGetValue(linkId, out ids))
                    {
                        ids = new List<UniqueId>();
                        _linkIds[linkId] = ids;
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
                Id = linkValue,
                Source = inputNode.GetXmlNodeValue("@C")
            };

            return uniqueId;
        }

        private static void UpdateId(UniqueId uniqueId, string groupTypeValue, string manufacturerGln, string groupNamespace)
        {
            switch (groupTypeValue)
            {
                case "1":
                    uniqueId.IdType = IdTypeEnum.UUID;
                    break;

                case "2":
                    int value;
                    var isInt = int.TryParse(uniqueId.Id, out value);

                    uniqueId.IdType = isInt ? IdTypeEnum.LongInt : IdTypeEnum.String;
                    uniqueId.SourceType = IdSourceTypeEnum.GLN;
                    uniqueId.Source = manufacturerGln;
                    break;

                case "3":
                    uniqueId.IdType = IdTypeEnum.String;
                    uniqueId.SourceType = IdSourceTypeEnum.URI;
                    uniqueId.Source = groupNamespace;
                    uniqueId.Id = string.Concat(groupNamespace, uniqueId.Id);
                    break;

                case "4":
                    uniqueId.IdType = IdTypeEnum.String;
                    uniqueId.SourceType = IdSourceTypeEnum.URI;
                    uniqueId.Id = string.Concat(groupNamespace, uniqueId.Id);
                    break;
            }
        }
    }
}
