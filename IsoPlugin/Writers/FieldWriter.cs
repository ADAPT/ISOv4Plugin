using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.FieldBoundaries;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;

namespace AgGateway.ADAPT.IsoPlugin.Writers
{
    internal class FieldWriter : BaseWriter
    {
        private GuidanceGroupWriter _guidanceWriter;

        private FieldWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "PFD")
        {
            _guidanceWriter = new GuidanceGroupWriter(taskWriter);
        }

        internal static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Fields == null ||
                taskWriter.DataModel.Catalog.Fields.Count == 0)
                return;

            var writer = new FieldWriter(taskWriter);
            writer.WriteFields();
        }

        private void WriteFields()
        {
            WriteToExternalFile(WriteFields);
        }

        private void WriteFields(XmlWriter writer)
        {
            foreach (var field in TaskWriter.DataModel.Catalog.Fields)
            {
                var fieldId = WriteField(writer, field);
                TaskWriter.Fields[field.Id.ReferenceId] = fieldId;
            }
        }

        private string WriteField(XmlWriter writer, Field field)
        {
            var fieldId = GenerateId();
            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", fieldId);
            writer.WriteAttributeString("C", field.Description);

            WriteArea(writer, field.Area);
            WriteCropZone(writer, field.Id);

            WriteFarmReference(writer, field.FarmId);
            WriteBoundary(writer, field.ActiveBoundaryId);
            WriteGuidance(writer, field.GuidanceGroupIds);

            writer.WriteEndElement();

            return fieldId;
        }

        private static void WriteArea(XmlWriter writer, NumericRepresentationValue area)
        {
            if (area == null || area.Value == null || area.Value.Value < 0)
                writer.WriteAttributeString("D", "0");
            else
            {
                var fieldArea = area.Value.ConvertToUnit(new CompositeUnitOfMeasure("m2"));
                writer.WriteAttributeString("D", fieldArea.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void WriteCropZone(XmlWriter writer, CompoundIdentifier fieldId)
        {
            if (TaskWriter.DataModel.Catalog.CropZones == null ||
                TaskWriter.DataModel.Catalog.CropZones.Count == 0)
                return;

            foreach (var cropZone in TaskWriter.DataModel.Catalog.CropZones)
            {
                if (cropZone.FieldId == fieldId.ReferenceId)
                {
                    var cropId = TaskWriter.Crops.FindById(cropZone.CropId.Value);
                    writer.WriteXmlAttribute("G", cropId);
                    break;
                }
            }
        }

        private void WriteFarmReference(XmlWriter writer, int? farmId)
        {
            if (!farmId.HasValue)
                return;

            writer.WriteXmlAttribute("F", TaskWriter.Farms.FindById(farmId.Value));
        }

        private void WriteBoundary(XmlWriter writer, int? boundaryId)
        {
            if (!boundaryId.HasValue)
                return;

            FieldBoundary fieldBoundary = null;
            foreach (var boundary in TaskWriter.DataModel.Catalog.FieldBoundaries)
            {
                if (boundary.Id.ReferenceId == boundaryId)
                {
                    fieldBoundary = boundary;
                    break;
                }
            }
            if (fieldBoundary == null || fieldBoundary.SpatialData == null)
                return;

            ShapeWriter.WritePolygon(writer, fieldBoundary.SpatialData);
        }

        private void WriteGuidance(XmlWriter writer, List<int> guidanceGroupIds)
        {
            if (guidanceGroupIds == null || guidanceGroupIds.Count == 0 ||
                TaskWriter.DataModel.Catalog.GuidanceGroups == null ||
                TaskWriter.DataModel.Catalog.GuidanceGroups.Count == 0)
                return;

            foreach (var guidanceGroupId in guidanceGroupIds)
            {
                foreach (var guidanceGroup in TaskWriter.DataModel.Catalog.GuidanceGroups)
                {
                    if (guidanceGroup.Id.ReferenceId == guidanceGroupId)
                    {
                        _guidanceWriter.Write(writer, guidanceGroup);
                        break;
                    }
                }
            }
        }
    }
}