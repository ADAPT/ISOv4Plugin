using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.Representation.UnitSystem;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class GuidanceShiftLoader
    {
        private readonly TaskDataDocument _taskDataDocument;

        public GuidanceShiftLoader(TaskDataDocument taskDataDocument)
        {
            _taskDataDocument = taskDataDocument;
        }

        public static GuidanceShift Load(XmlNodeList inputNode, TaskDataDocument taskDataDocument)
        {
            var loader = new GuidanceShiftLoader(taskDataDocument);
         
            var node = inputNode.Item(0);
            return loader.Load(node);
        }

        private GuidanceShift Load(XmlNode node)
        {
            var groupId = node.GetXmlAttribute("A");
            var patternId = node.GetXmlAttribute("B");
            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(patternId))
                return null;

            var guidanceGroup = FindGuidanceGroup(groupId);

            return new GuidanceShift
            {
                GuidanceGroupId = guidanceGroup != null && guidanceGroup.Group != null ? guidanceGroup.Group.Id.ReferenceId : 0,
                GuidancePatterId = FindGuidancePatternId(guidanceGroup, patternId),
                EastShift = GetShiftValue(node.GetXmlAttribute("C")),
                NorthShift = GetShiftValue(node.GetXmlAttribute("D")),
                PropagationOffset = GetShiftValue(node.GetXmlAttribute("E"))
            };
        }

        private GuidanceGroupDescriptor FindGuidanceGroup(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId))
                return _taskDataDocument.GuidanceGroups.FindById(groupId);
            return null;
        }

        private static int FindGuidancePatternId(GuidanceGroupDescriptor group, string patternId)
        {
            if (!string.IsNullOrEmpty(patternId) && group != null)
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
