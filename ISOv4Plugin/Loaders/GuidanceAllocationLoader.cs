using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.Representation.UnitSystem;

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
            return loader.Load(inputNode.SelectNodes("GAS"));
        }

        private List<GuidanceAllocation> Load(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                _allocations.AddRange(LoadGuidanceAllocations(inputNode));
            }

            return _allocations;
        }

        private List<GuidanceAllocation> LoadGuidanceAllocations(XmlNode inputNode)
        {
            List<GuidanceAllocation> allocations = new List<GuidanceAllocation>();

            var groupId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(groupId))
                return allocations;

            var group = _taskDocument.GuidanceGroups.FindById(groupId);
            if (group == null)
                return allocations;

            var baseTimeScope = AllocationTimestampLoader.Load(inputNode);

            foreach (XmlNode shiftNode in inputNode.SelectNodes("GST"))
            {
                var allocation = LoadGuidanceShift(shiftNode, group, baseTimeScope);
                if (allocation == null)
                    continue;

                allocations.Add(allocation);
            }

            return allocations;
        }

        private GuidanceAllocation LoadGuidanceShift(XmlNode inputNode, GuidanceGroupDescriptor group, TimeScope baseTimeScope)
        {
            var groupId = inputNode.GetXmlNodeValue("@A");
            var patternId = inputNode.GetXmlNodeValue("@B");
            if (string.IsNullOrEmpty(groupId) && string.IsNullOrEmpty(patternId))
                return null;

            group = FindGuidanceGroup(groupId) ?? group;
            var allocation = new GuidanceAllocation
            {
                GuidanceGroupId = group.Group.Id.ReferenceId,
                GuidancePatternId = FindGuidancePattern(group, patternId),

                EastShift = GetShiftValue(inputNode.GetXmlNodeValue("@C")),
                NorthShift = GetShiftValue(inputNode.GetXmlNodeValue("@D")),
                PropagationOffset = GetShiftValue(inputNode.GetXmlNodeValue("@E")),

                TimeScope = AllocationTimestampLoader.Load(inputNode) ?? baseTimeScope
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

        private static NumericRepresentationValue GetShiftValue(string inputValue)
        {
            long shift;
            if (!inputValue.ParseValue(out shift))
                return null;

            var shiftUnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("mm");
            var numericValue = new NumericValue(shiftUnitOfMeasure, shift);
            return new NumericRepresentationValue(null, numericValue.UnitOfMeasure, numericValue);
        }
    }
}
