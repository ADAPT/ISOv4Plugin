using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class GuidanceAllocationLoader
    {
        private TaskDataDocument _taskDocument;
        private List<GuidanceAllocation> _allocations;

        private GuidanceAllocationLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _allocations = new List<GuidanceAllocation>();
        }

        public static List<GuidanceAllocation> Load(XmlNode inputNode, TaskDataDocument taskDocument)
        {
            var loader = new GuidanceAllocationLoader(taskDocument);
            return loader.Load(inputNode.SelectNodes("GAN"));
        }

        private List<GuidanceAllocation> Load(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                var guidanceAllocation = LoadGuidanceAllocations(inputNode);
                if(guidanceAllocation != null)
                    _allocations.Add(guidanceAllocation);
            }

            return _allocations;
        }

        private GuidanceAllocation LoadGuidanceAllocations(XmlNode inputNode)
        {
            var groupId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(groupId))
                return null;

            var group = FindGuidanceGroup(groupId);
            var gsts = inputNode.SelectNodes("GST");

            var allocation = new GuidanceAllocation
            {
                GuidanceGroupId = group.Group.Id.ReferenceId,
                GuidanceShift = GuidanceShiftLoader.Load(gsts, _taskDocument),
                TimeScopes = new List<TimeScope> { AllocationTimestampLoader.Load(inputNode) ?? AllocationTimestampLoader.Load(inputNode) }
            };
            
            return allocation;
        }

        private GuidanceGroupDescriptor FindGuidanceGroup(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId))
                return _taskDocument.GuidanceGroups.FindById(groupId);
            return null;
        }

        private static int FindGuidancePattern(GuidanceGroupDescriptor group, string patternId)
        {
            if (!string.IsNullOrEmpty(patternId))
            {
                var pattern = group.Patterns.FindById(patternId);
                if (pattern != null)
                    return pattern.Id.ReferenceId;
            }
            return 0;
        }
    }
}
