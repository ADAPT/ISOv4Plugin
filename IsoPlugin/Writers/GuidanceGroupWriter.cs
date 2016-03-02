using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;

namespace AgGateway.ADAPT.IsoPlugin.Writers
{
    internal class GuidanceGroupWriter : BaseWriter
    {
        private GuidancePatternWriter _guidancePatternWriter;

        internal GuidanceGroupWriter(TaskDocumentWriter taskWriter)
            :base(taskWriter, "GGP")
        {
            _guidancePatternWriter = new GuidancePatternWriter(taskWriter);
        }

        internal void Write(XmlWriter writer, GuidanceGroup guidanceGroup)
        {
            if (guidanceGroup == null)
                return;

            WriteGuidacenGroup(writer, guidanceGroup);
        }

        private void WriteGuidacenGroup(XmlWriter writer, GuidanceGroup guidanceGroup)
        {
            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", GenerateId());
            writer.WriteXmlAttribute("B", guidanceGroup.Description);

            WriteGuidancePatterns(writer, guidanceGroup.GuidancePatternIds);
            WriteBoundary(writer, guidanceGroup.BoundingPolygon);

            writer.WriteEndElement();
        }

        private void WriteGuidancePatterns(XmlWriter writer, List<int> guidancePatternIds)
        {
            if (guidancePatternIds == null ||
                guidancePatternIds.Count == 0 ||
                TaskWriter.DataModel.Catalog.GuidancePatterns == null ||
                TaskWriter.DataModel.Catalog.GuidancePatterns.Count == 0)
                return;

            foreach (var guidancePatternId in guidancePatternIds)
            {
                foreach (var guidancePattern in TaskWriter.DataModel.Catalog.GuidancePatterns)
                {
                    if (guidancePattern.Id.ReferenceId == guidancePatternId)
                    {
                        _guidancePatternWriter.Write(writer, guidancePattern);
                        break;
                    }
                }
            }
        }

        private void WriteBoundary(XmlWriter writer, MultiPolygon boundary)
        {
            if (boundary == null || boundary.Polygons == null || boundary.Polygons.Count == 0)
                return;

            // Guidance pattern only supports a single polygon-based boundary
            var polygon = boundary.Polygons[0];
            ShapeWriter.WritePolygon(writer, polygon);
        }
    }
}