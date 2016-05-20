using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using NumericRepresentation = AgGateway.ADAPT.ApplicationDataModel.Representations.NumericRepresentation;
using UnitOfMeasure = AgGateway.ADAPT.ApplicationDataModel.Common.UnitOfMeasure;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class PrescriptionLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private List<RasterGridPrescription> _prescriptions;
        private static RepresentationMapper _representationMapper;

        private PrescriptionLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _prescriptions = new List<RasterGridPrescription>();
            _representationMapper = new RepresentationMapper();
        }

        public static List<RasterGridPrescription> Load(TaskDataDocument taskDocument)
        {
            var loader = new PrescriptionLoader(taskDocument);

            return loader.Load();
        }

        private List<RasterGridPrescription> Load()
        {
            LoadPrescriptions(_rootNode.SelectNodes("TSK"));
            ProcessExternalNodes();

            return _prescriptions;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'TSK')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadPrescriptions(inputNodes);
            }
        }

        private void LoadPrescriptions(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                var prescription = LoadPrescription(inputNode);
                if (prescription != null)
                    _prescriptions.Add(prescription);
            }
        }

        private RasterGridPrescription LoadPrescription(XmlNode inputNode)
        {
            if (!HasPrescription(inputNode))
                return null;

            var prescription = new RasterGridPrescription();

            // Required fields. Do not proceed if they are missing
            var prescriptionId = inputNode.GetXmlNodeValue("@A");
            if (prescriptionId == null)
                return null;

            // Optional fields
            prescription.Description = inputNode.GetXmlNodeValue("@B");

            LoadFieldAndCropZone(inputNode.GetXmlNodeValue("@E"), prescription);

            LoadGrid(inputNode, prescription);

            _taskDocument.LoadLinkedIds(prescriptionId, prescription.Id);
            return prescription;
        }

        private static bool HasPrescription(XmlNode inputNode)
        {
            var gridNodes = inputNode.SelectNodes("GRD");
            return gridNodes.Count == 1;
        }

        private void LoadFieldAndCropZone(string fieldId, RasterGridPrescription prescription)
        {
            if (string.IsNullOrEmpty(fieldId))
                return;

            var cropZone = _taskDocument.CropZones.FindById(fieldId);
            if (cropZone != null)
            {
                prescription.CropZoneId = cropZone.Id.ReferenceId;
                prescription.FieldId = cropZone.FieldId;
            }
            else
            {
                var field = _taskDocument.Fields.FindById(fieldId);
                if (field != null)
                    prescription.FieldId = field.Id.ReferenceId;
            }
        }

        private void LoadGrid(XmlNode inputNode, RasterGridPrescription prescription)
        {
            var treatmentZones = TreatmentZoneLoader.Load(inputNode, _taskDocument);
            var gridDescriptor = GridLoader.Load(inputNode, treatmentZones, _taskDocument.BaseFolder);

            if (gridDescriptor == null)
                return;

            LoadDefinition(gridDescriptor, prescription);
            LoadRates(inputNode, gridDescriptor, treatmentZones, prescription);
        }

        private void LoadRates(XmlNode inputNode, GridDescriptor gridDescriptor, Dictionary<int, TreatmentZone> treatmentZones, RasterGridPrescription prescription)
        {
            prescription.LossOfGpsRate = LoadRateFromTreatmentZones(inputNode.GetXmlNodeValue("@I"), treatmentZones).FirstOrDefault();
            prescription.OutOfFieldRate = LoadRateFromTreatmentZones(inputNode.GetXmlNodeValue("@J"), treatmentZones).FirstOrDefault();

            if (gridDescriptor.TreatmentZones != null)
            {
                var treatmentZone = treatmentZones.FindById(gridDescriptor.TreatmentZones.First());
                if (treatmentZone == null)
                    return;

                LoadProducts(treatmentZone, prescription);
                LoadRateUnits(treatmentZone, prescription);
                prescription.Rates = LoadRatesFromTreatmentZones(gridDescriptor, treatmentZones, prescription.ProductIds);
            }
            else if (gridDescriptor.ProductRates != null)
            {
                var treatmentZoneTemplate = treatmentZones.FindById(gridDescriptor.ProductRateTemplateId);
                if (treatmentZoneTemplate == null)
                    return;

                LoadProducts(treatmentZoneTemplate, prescription);
                LoadRateUnits(treatmentZoneTemplate, prescription);
                prescription.Rates = LoadRatesFromProducts(gridDescriptor, prescription.ProductIds);
            }
        }

        private static void LoadDefinition(GridDescriptor gridDescriptor, RasterGridPrescription prescription)
        {
            prescription.BoundingBox = new BoundingBox();
            prescription.BoundingBox.MinY = new NumericRepresentationValue(RepresentationInstanceList.vrLatitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), gridDescriptor.Origin.Y));
            prescription.BoundingBox.MinX = new NumericRepresentationValue(RepresentationInstanceList.vrLongitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), gridDescriptor.Origin.X));
            var maxYValue = prescription.BoundingBox.MinY.Value.Value + gridDescriptor.CellHeight.Value.Value * gridDescriptor.RowCount;
            var maxXValue = prescription.BoundingBox.MinX.Value.Value + gridDescriptor.CellWidth.Value.Value * gridDescriptor.ColumnCount;
            prescription.BoundingBox.MaxY = new NumericRepresentationValue(RepresentationInstanceList.vrLatitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), maxYValue));
            prescription.BoundingBox.MaxX = new NumericRepresentationValue(RepresentationInstanceList.vrLongitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), maxXValue));

            prescription.Origin = gridDescriptor.Origin;
            prescription.CellHeight = gridDescriptor.CellHeight;
            prescription.CellWidth = gridDescriptor.CellWidth;

            prescription.ColumnCount = gridDescriptor.ColumnCount;
            prescription.RowCount = gridDescriptor.RowCount;
        }

        private static List<NumericRepresentationValue> LoadRateFromTreatmentZones(string zoneId, Dictionary<int, TreatmentZone> treatmentZones)
        {
            var rates = new List<NumericRepresentationValue>();
            int treatmentZoneId;
            if (!zoneId.ParseValue(out treatmentZoneId))
                return rates;

            if (!treatmentZones.ContainsKey(treatmentZoneId))
                return rates;

            var treatmentZone = treatmentZones[treatmentZoneId];
            if (treatmentZone.Variables == null || treatmentZone.Variables.Count == 0)
                return rates;

            foreach (var dataVariable in treatmentZone.Variables)
            {
                rates.Add(new NumericRepresentationValue
                {
                    Representation = _representationMapper.Map(dataVariable.Ddi) as NumericRepresentation,
                    Value = new NumericValue(dataVariable.IsoUnit.ToAdaptUnit(), dataVariable.Value),
                    UserProvidedUnitOfMeasure = dataVariable.UserUnit
                });
            }
            return rates;
        }

        private void LoadProducts(TreatmentZone treatmentZone, RasterGridPrescription prescription)
        {
            var productIds = new List<int>();
            foreach (var dataVariable in treatmentZone.Variables)
            {
                var product = _taskDocument.Products.FindById(dataVariable.ProductId) ?? _taskDocument.ProductMixes.FindById(dataVariable.ProductId); 
                product = product ?? _taskDocument.CropVarieties.FindById(dataVariable.ProductId);
                productIds.Add(product == null ? 0 : product.Id.ReferenceId);
            }
            prescription.ProductIds = productIds;
        }

        private void LoadRateUnits(TreatmentZone treatmentZone, RasterGridPrescription prescription)
        {
            if(prescription.RxProductLookups == null)
                prescription.RxProductLookups = new List<RxProductLookup>();

            var rxRates = new List<RxRate>();
            foreach (var dataVariable in treatmentZone.Variables)
            {
                var product = _taskDocument.Products.FindById(dataVariable.ProductId) ?? _taskDocument.ProductMixes.FindById(dataVariable.ProductId);
                var rxProductLookup = new RxProductLookup
                {
                    ProductId = product == null ? 0 : product.Id.FindIntIsoId(),
                    UnitOfMeasure = dataVariable.IsoUnit.ToAdaptUnit(),
                };
                prescription.RxProductLookups.Add(rxProductLookup);
                var rxRate = new RxRate
                {
                    Rate = dataVariable.Value,
                    RxProductLookupId = rxProductLookup.Id.ReferenceId,
                };
                rxRates.Add(rxRate);
            }
            prescription.Rates = new List<RxRates>{ new RxRates{ RxRate = rxRates }}; 
        }

        private static List<RxRates> LoadRatesFromProducts(GridDescriptor gridDescriptor, List<int> productIds)
        {
            var rates = new List<RxRates>();
            foreach (var productRates in gridDescriptor.ProductRates)
            {
                var rate = new RxRates { RxRate = new List<RxRate>() };

                for (int productIndex = 0; productIndex < productRates.Count; productIndex++)
                {
                    AddRate(productIds[productIndex], productRates[productIndex], rate);
                }

                rates.Add(rate);
            }

            return rates;
        }

        private static List<RxRates> LoadRatesFromTreatmentZones(GridDescriptor gridDescriptor, Dictionary<int, TreatmentZone> treatmentZones, List<int> productIds)
        {
            var rates = new List<RxRates>();
            foreach (var treatmentZoneId in gridDescriptor.TreatmentZones)
            {
                var treatmentZone = treatmentZones.FindById(treatmentZoneId);
                if (treatmentZone == null)
                    return null;

                var rate = new RxRates { RxRate = new List<RxRate>() };

                for (int i = 0; i < treatmentZone.Variables.Count; i++)
                {
                    var dataVariable = treatmentZone.Variables[i];
                    AddRate(productIds[i], dataVariable.Value, rate);
                }

                rates.Add(rate);
            }

            return rates;
        }

        private static void AddRate(int productId, double productRate, RxRates rates)
        {
            var rxRate = new RxRate
            {
                Rate = productRate,
                RxProductLookupId = productId,
            };

            rates.RxRate.Add(rxRate);
        }
    }
}