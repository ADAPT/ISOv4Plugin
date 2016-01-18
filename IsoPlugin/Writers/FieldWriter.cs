using AgGateway.ADAPT.ApplicationDataModel;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class FieldWriter : BaseWriter
    {
        private FieldWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "PFD")
        {
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
    }
}