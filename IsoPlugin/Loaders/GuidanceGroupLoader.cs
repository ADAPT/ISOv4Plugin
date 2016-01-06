using AgGateway.ADAPT.ApplicationDataModel;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal static class GuidanceGroupLoader
    {
        internal static Dictionary<string, GuidanceGroupDescriptor> LoadGuidanceGroups(XmlNodeList inputNodes)
        {
            var groups = new Dictionary<string, GuidanceGroupDescriptor>();
            foreach (XmlNode inputNode in inputNodes)
            {
                string groupId;
                var group = LoadGuidanceGroup(inputNode, out groupId);
                if (group == null)
                    return null;

                groups.Add(groupId, group);
            }

            return groups;
        }

        private static GuidanceGroupDescriptor LoadGuidanceGroup(XmlNode inputNode, out string groupId)
        {
            groupId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(groupId))
                return null;

            var group = new GuidanceGroup();
            group.Description = inputNode.GetXmlNodeValue(@"B");

            group.BoundingPolygon = ShapeLoader.LoadPolygon(inputNode.SelectNodes("PLN[0]"));

            var patterns = GuidancePatternLoader.LoadGuidancePatterns(inputNode.SelectNodes("GPN"));
            group.GuidancePatternIds = patterns.Values.Select(x => x.Id.ReferenceId).ToList();

            return new GuidanceGroupDescriptor(group, patterns);
        }
    }
}
