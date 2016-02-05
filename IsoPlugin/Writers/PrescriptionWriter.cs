using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class PrescriptionWriter : BaseWriter
    {
        private GridWriter _gridWriter;

        private PrescriptionWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "TSK")
        {
            _gridWriter = new GridWriter(taskWriter);
        }

        internal static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Prescriptions == null ||
                taskWriter.DataModel.Catalog.Prescriptions.Count == 0)
                return;

            var writer = new PrescriptionWriter(taskWriter);
            writer.Write();
        }

        private void Write()
        {
            WriteToExternalFile(WritePrescriptions);
        }

        private void WritePrescriptions(XmlWriter writer)
        {
            foreach (var prescription in TaskWriter.DataModel.Catalog.Prescriptions.OfType<RasterGridPrescription>())
            {
                WritePrescription(writer, prescription);
            }
        }

        private void WritePrescription(XmlWriter writer, RasterGridPrescription prescription)
        {
            if (!IsValidPrescription(prescription))
                return;

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", GenerateId());
            writer.WriteAttributeString("B", prescription.Description);

            WriteFieldMeta(writer, prescription.FieldId);

            // Task status - planned
            writer.WriteAttributeString("G", "1");

            WriteTreatmentZones(writer, prescription);

            _gridWriter.Write(writer, prescription);

            writer.WriteEndElement();
        }

        private static bool IsValidPrescription(RasterGridPrescription prescription)
        {
            return prescription.Rates != null &&
                prescription.CellHeight != null &&
                prescription.CellWidth != null &&
                prescription.Origin != null;
        }

        private void WriteFieldMeta(XmlWriter writer, int fieldId)
        {
            var field = TaskWriter.Fields.FindById(fieldId);
            writer.WriteXmlAttribute("E", field);
        }

        private void WriteTreatmentZones(XmlWriter writer, RasterGridPrescription prescription)
        {
            if (prescription.ProductIds == null)
                return;

            var lossOfSignlaTreatmentZone = new TreatmentZone { Name = "Loss of GPS", Variables = new List<DataVariable>() };
            var outOfFieldTreatmentZone = new TreatmentZone { Name = "Out of Field", Variables = new List<DataVariable>() };
            var defaultTreatmentZone = new TreatmentZone { Name = "Default", Variables = new List<DataVariable>() };

            var defaultRate = new NumericRepresentationValue(null, new NumericValue(null, 0));

            foreach (var productId in prescription.ProductIds)
            {
                var isoProductId = TaskWriter.Products.FindById(productId);

                AddDataVariable(lossOfSignlaTreatmentZone, prescription.LossOfGpsRate, isoProductId);
                AddDataVariable(outOfFieldTreatmentZone, prescription.OutOfFieldRate, isoProductId);
                AddDataVariable(defaultTreatmentZone, defaultRate, isoProductId);
            }

            var lossOfSignalZoneId = "253";
            if (lossOfSignlaTreatmentZone.Variables.Count > 0)
                writer.WriteXmlAttribute("I", lossOfSignalZoneId);

            var outOfFieldZoneId = "254";
            if (outOfFieldTreatmentZone.Variables.Count > 0)
                writer.WriteXmlAttribute("J", outOfFieldZoneId);

            TreatmentZoneWriter.Write(writer, "1", defaultTreatmentZone);
            if (lossOfSignlaTreatmentZone.Variables.Count > 0)
                TreatmentZoneWriter.Write(writer, lossOfSignalZoneId, lossOfSignlaTreatmentZone);
            if (outOfFieldTreatmentZone.Variables.Count > 0)
                TreatmentZoneWriter.Write(writer, outOfFieldZoneId, outOfFieldTreatmentZone);
        }

        private static void AddDataVariable(TreatmentZone treatmentZone, NumericRepresentationValue value, string productId)
        {
            if (value != null && value.Value != null)
            {
                var dataVariable = new DataVariable
                {
                    ProductId = productId,
                    Value = value
                };

                treatmentZone.Variables.Add(dataVariable);
            }
        }
    }
}