using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class PrescriptionWriter : BaseWriter
    {
        private GridWriter _gridWriter;
        private ADAPT.Representation.UnitSystem.UnitOfMeasureConverter _unitConverter;

        private PrescriptionWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "TSK")
        {
            _unitConverter = new ADAPT.Representation.UnitSystem.UnitOfMeasureConverter();
            _gridWriter = new GridWriter(taskWriter);
        }

        public static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Prescriptions == null ||
                !taskWriter.DataModel.Catalog.Prescriptions.Any())
                return;

            var writer = new PrescriptionWriter(taskWriter);
            writer.WritePrescriptions(taskWriter);
        }

        private void WritePrescriptions(TaskDocumentWriter writer)
        {
            foreach (var prescription in TaskWriter.DataModel.Catalog.Prescriptions.OfType<RasterGridPrescription>())
            {
                WritePrescription(writer, prescription);
            }
        }

        private void WritePrescription(TaskDocumentWriter taskWriter, RasterGridPrescription prescription)
        {
            var writer = taskWriter.RootWriter;

            if (!IsValidPrescription(prescription))
                return;

            var prescriptionId = prescription.Id.FindIsoId() ?? GenerateId();

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", prescriptionId);
            writer.WriteAttributeString("B", prescription.Description);

            WriteFieldMeta(writer, prescription.FieldId);

            // Task status - planned
            writer.WriteAttributeString("G", "1");

            var defaultTreatmentZone = WriteTreatmentZones(writer, prescription);

            _gridWriter.Write(writer, prescription, defaultTreatmentZone);
            var matchingLoggedData = null as LoggedData;

            if (taskWriter.DataModel.Documents != null && taskWriter.DataModel.Documents.LoggedData != null)
                matchingLoggedData = taskWriter.DataModel.Documents.LoggedData.SingleOrDefault(x => x.OperationData.FirstOrDefault(y => y.PrescriptionId == prescription.Id.ReferenceId) != null);

            if (matchingLoggedData != null)
            {
                var taskMapper = new TaskMapper();
                var isoInt = Convert.ToInt32(prescriptionId.Remove(0, 3))-1;


                var mappedTsk =
                    taskMapper.Map(new List<LoggedData> { matchingLoggedData }, taskWriter.DataModel.Catalog,
                        taskWriter.BaseFolder, isoInt, taskWriter).First();

                foreach (var item in mappedTsk.Items)
                {
                    item.WriteXML(taskWriter.RootWriter);
                }
            }
            else
            {
                TaskWriter.Ids.Add(prescriptionId, prescription.Id);
            }

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

            if (!string.IsNullOrEmpty(field))
                WriteFarmMeta(writer, fieldId);

        }

        private void WriteFarmMeta(XmlWriter writer, int fieldId)
        {
            foreach (var field in TaskWriter.DataModel.Catalog.Fields)
            {
                if (field.Id.ReferenceId == fieldId)
                {
                    if (field.FarmId.HasValue)
                    {
                        var farmId = TaskWriter.Farms.FindById(field.FarmId.Value);
                        writer.WriteXmlAttribute("D", farmId);

                        if (!string.IsNullOrEmpty(farmId))
                            WriteCustomerMeta(writer, field.FarmId.Value);
                    }
                    break;
                }
            }
        }

        private void WriteCustomerMeta(XmlWriter writer, int farmId)
        {
            foreach (var farm in TaskWriter.DataModel.Catalog.Farms)
            {
                if (farm.Id.ReferenceId == farmId)
                {
                    if (farm.GrowerId.HasValue)
                    {
                        var customerId = TaskWriter.Customers.FindById(farm.GrowerId.Value);
                        writer.WriteXmlAttribute("C", customerId);
                    }
                    break;
                }
            }
        }

        private TreatmentZone WriteTreatmentZones(XmlWriter writer, RasterGridPrescription prescription)
        {
            if (prescription.ProductIds == null)
                return null;

            var lossOfSignalTreatmentZone = new TreatmentZone { Name = "Loss of GPS", Variables = new List<DataVariable>() };
            var outOfFieldTreatmentZone = new TreatmentZone { Name = "Out of Field", Variables = new List<DataVariable>() };
            var defaultTreatmentZone = new TreatmentZone { Name = "Default", Variables = new List<DataVariable>() };

            var defaultRate = new NumericRepresentationValue(null, new NumericValue(prescription.RxProductLookups.First().UnitOfMeasure, 0));
            var isoUnit = DetermineIsoUnit(prescription.RxProductLookups.First().UnitOfMeasure);

            foreach (var productId in prescription.ProductIds)
            {
                var isoProductId = TaskWriter.Products.FindById(productId) ?? TaskWriter.CropVarieties.FindById(productId);

                AddDataVariable(lossOfSignalTreatmentZone, prescription.LossOfGpsRate, isoProductId, isoUnit);
                AddDataVariable(outOfFieldTreatmentZone, prescription.OutOfFieldRate, isoProductId, isoUnit);
                AddDataVariable(defaultTreatmentZone, defaultRate, isoProductId, isoUnit);
            }

            var lossOfSignalZoneId = "253";
            if (lossOfSignalTreatmentZone.Variables.Count > 0)
                writer.WriteXmlAttribute("I", lossOfSignalZoneId);

            var outOfFieldZoneId = "254";
            if (outOfFieldTreatmentZone.Variables.Count > 0)
                writer.WriteXmlAttribute("J", outOfFieldZoneId);

            TreatmentZoneWriter.Write(writer, "1", defaultTreatmentZone);
            if (lossOfSignalTreatmentZone.Variables.Count > 0)
                TreatmentZoneWriter.Write(writer, lossOfSignalZoneId, lossOfSignalTreatmentZone);
            if (outOfFieldTreatmentZone.Variables.Count > 0)
                TreatmentZoneWriter.Write(writer, outOfFieldZoneId, outOfFieldTreatmentZone);

            return defaultTreatmentZone;
        }

        private static IsoUnit DetermineIsoUnit(UnitOfMeasure rateUnit)
        {
            if (rateUnit == null)
                return null;

            return UnitFactory.Instance.GetUnitByDimension(rateUnit.Dimension);
        }

        private void AddDataVariable(TreatmentZone treatmentZone, NumericRepresentationValue value, string productId, IsoUnit unit)
        {
            if (value != null && value.Value != null)
            {
                var targetValue = value.Value.Value;

                // Convert input value to Iso unit
                var adaptUnit = unit.ToAdaptUnit();
                UnitOfMeasure userUnit = null;
                if (adaptUnit != null && value.Value.UnitOfMeasure != null &&
                    adaptUnit.Dimension == value.Value.UnitOfMeasure.Dimension)
                {
                    userUnit = value.Value.UnitOfMeasure;
                    targetValue = _unitConverter.Convert(userUnit.ToInternalUom(), adaptUnit.ToInternalUom(), targetValue);
                }

                var dataVariable = new DataVariable
                {
                    ProductId = productId,
                    Value = targetValue,
                    IsoUnit = unit,
                    UserUnit = userUnit
                };

                treatmentZone.Variables.Add(dataVariable);
            }
        }
    }
}