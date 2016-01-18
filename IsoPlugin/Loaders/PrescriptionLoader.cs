using AgGateway.ADAPT.ApplicationDataModel;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System;
using System.Linq;

namespace AgGateway.ADAPT.Plugins
{
    internal class PrescriptionLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private List<RasterGridPrescription> _prescriptions;

        private PrescriptionLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _prescriptions = new List<RasterGridPrescription>();
        }

        internal static List<RasterGridPrescription> Load(TaskDataDocument taskDocument)
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

            if (gridDescriptor.TreatmentZones != null)
            {
                var treatmentZone = treatmentZones.FindById(gridDescriptor.TreatmentZones.First());
                if (treatmentZone == null)
                    return;

                LoadProducts(treatmentZone, prescription);
                prescription.Rates = LoadRatesFromTreatmentZones(gridDescriptor, treatmentZones, prescription.ProductIds);
            }
            else if (gridDescriptor.ProductRates != null)
            {
                var treatmentZoneTemplate = treatmentZones.FindById(gridDescriptor.ProductRateTemplateId);
                if (treatmentZoneTemplate == null)
                    return;

                LoadProducts(treatmentZoneTemplate, prescription);
                prescription.Rates = LoadRatesFromProducts(gridDescriptor, prescription.ProductIds);
            }
        }

        private static void LoadDefinition(GridDescriptor gridDescriptor, RasterGridPrescription prescription)
        {
            prescription.BoundingBox = new BoundingBox();
            prescription.BoundingBox.MinLatitude = gridDescriptor.Origin.Y;
            prescription.BoundingBox.MinLongitude = gridDescriptor.Origin.X;
            prescription.BoundingBox.MaxLatitude = prescription.BoundingBox.MinLatitude + gridDescriptor.CellHeight.Value.Value * gridDescriptor.RowCount;
            prescription.BoundingBox.MaxLongitude = prescription.BoundingBox.MinLongitude + gridDescriptor.CellWidth.Value.Value * gridDescriptor.ColumnCount;

            prescription.Origin = gridDescriptor.Origin;
            prescription.CellHeight = gridDescriptor.CellHeight;
            prescription.CellWidth = gridDescriptor.CellWidth;

            prescription.ColumnCount = gridDescriptor.ColumnCount;
            prescription.RowCount = gridDescriptor.RowCount;
        }

        private void LoadProducts(TreatmentZone treatmentZone, RasterGridPrescription prescription)
        {
            var productLookup = new List<int>();
            foreach (var dataVariable in treatmentZone.Variables)
            {
                var product = _taskDocument.Products.FindById(dataVariable.ProductId) ?? _taskDocument.ProductMixes.FindById(dataVariable.ProductId);
                productLookup.Add(product == null ? 0 : product.Id.ReferenceId);
            }
            prescription.ProductIds = productLookup;
        }

        private static RxRates[,] LoadRatesFromProducts(GridDescriptor gridDescriptor, List<int> productIds)
        {
            var rates = new RxRates[gridDescriptor.RowCount, gridDescriptor.ColumnCount];
            for (int cellIndex = 0; cellIndex < gridDescriptor.ProductRates.Count; cellIndex++)
            {
                var productRates = gridDescriptor.ProductRates[cellIndex];

                var rate = new RxRates { RxRate = new List<RxRate>() };

                for (int productIndex = 0; productIndex < productRates.Count; productIndex++)
                {
                    var productRate = new RxRate
                    {
                        Rate = productRates[productIndex],
                        ProductId = productIds[productIndex]
                    };
                    if (productRate.ProductId == 0)
                        productRate.ProductId = null;

                    rate.RxRate.Add(productRate);
                }

                var rowIndex = cellIndex / gridDescriptor.ColumnCount;
                var columnIndex = cellIndex % gridDescriptor.ColumnCount;

                rates[rowIndex, columnIndex] = rate;
            }

            return rates;
        }

        private static RxRates[,] LoadRatesFromTreatmentZones(GridDescriptor gridDescriptor, Dictionary<int, TreatmentZone> treatmentZones, List<int> productIds)
        {
            var rates = new RxRates[gridDescriptor.RowCount, gridDescriptor.ColumnCount];
            for (int cellIndex = 0; cellIndex < gridDescriptor.TreatmentZones.Count; cellIndex++)
            {
                var treatmentZoneId = gridDescriptor.TreatmentZones[cellIndex];

                var treatmentZone = treatmentZones.FindById(treatmentZoneId);
                if (treatmentZone == null)
                    return null;

                var rate = new RxRates { RxRate = new List<RxRate>() };

                for (int i = 0; i < treatmentZone.Variables.Count; i++)
                {
                    var dataVariable = treatmentZone.Variables[i];
                    var productRate = new RxRate
                    {
                        Rate = dataVariable.Value.Value.Value,
                        ProductId = productIds[i]
                    };

                    if (productRate.ProductId == 0)
                        productRate.ProductId = null;

                    rate.RxRate.Add(productRate);
                }

                var rowIndex = cellIndex / gridDescriptor.ColumnCount;
                var columnIndex = cellIndex % gridDescriptor.ColumnCount;

                rates[rowIndex, columnIndex] = rate;
            }

            return rates;
        }
    }
}