using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal class GridLoader
    {
        private GridDescriptor _descriptor;
        private string _gridFileName;
        private string _baseFolder;
        private Dictionary<int, TreatmentZone> _treatmentZones;

        private GridLoader(string baseFolder, Dictionary<int, TreatmentZone> treatmentZones)
        {
            _descriptor = new GridDescriptor();
            _baseFolder = baseFolder;
            _treatmentZones = treatmentZones;
        }

        internal static GridDescriptor Load(XmlNode inputNode, Dictionary<int, TreatmentZone> treatmentZones, string baseFolder)
        {
            var loader = new GridLoader(baseFolder, treatmentZones);
            return loader.Load(inputNode);
        }

        private GridDescriptor Load(XmlNode inputNode)
        {
            var gridNode = inputNode.SelectSingleNode("GRD");
            if (gridNode == null)
                return null;

            if (!LoadGridDefinition(gridNode))
                return null;
            if (!LoadRates(gridNode))
                return null;

            return _descriptor;
        }

        private bool LoadGridDefinition(XmlNode inputNode)
        {
            if (!LoadOriginPosition(inputNode))
                return false;

            if (!LoadCellDimensions(inputNode))
                return false;

            return true;
        }

        private bool LoadOriginPosition(XmlNode inputNode)
        {
            double minLatitude;
            if (!inputNode.GetXmlNodeValue("@A").ParseValue(out minLatitude) ||
                minLatitude < -90 || minLatitude > 90)
                return false;

            double minLongitude;
            if (!inputNode.GetXmlNodeValue("@B").ParseValue(out minLongitude) ||
                minLongitude < -180 || minLongitude > 180)
                return false;

            _descriptor.Origin = new Point();
            _descriptor.Origin.Y = minLatitude;
            _descriptor.Origin.X = minLongitude;

            return true;
        }

        private bool LoadCellDimensions(XmlNode inputNode)
        {
            double gridCellSizeLatitude;
            if (!inputNode.GetXmlNodeValue("@C").ParseValue(out gridCellSizeLatitude) ||
                gridCellSizeLatitude < 0 || gridCellSizeLatitude > 1)
                return false;

            double gridCellSizeLongitude;
            if (!inputNode.GetXmlNodeValue("@D").ParseValue(out gridCellSizeLongitude) ||
                gridCellSizeLongitude < 0 || gridCellSizeLongitude > 1)
                return false;

            int columnCount;
            if (!inputNode.GetXmlNodeValue("@E").ParseValue(out columnCount) ||
                columnCount < 0)
                return false;

            int rowCount;
            if (!inputNode.GetXmlNodeValue("@F").ParseValue(out rowCount) ||
                rowCount < 0)
                return false;

            _descriptor.ColumnCount = columnCount;
            _descriptor.RowCount = rowCount;
            _descriptor.CellHeight = new NumericRepresentationValue(null, new NumericValue(null, gridCellSizeLatitude));
            _descriptor.CellWidth = new NumericRepresentationValue(null, new NumericValue(null, gridCellSizeLongitude));

            return true;
        }

        private bool LoadRates(XmlNode gridNode)
        {
            _gridFileName = gridNode.GetXmlNodeValue("@G");
            if (string.IsNullOrEmpty(_gridFileName))
                return false;

            var gridType = gridNode.GetXmlNodeValue("@I");
            if (string.IsNullOrEmpty(gridType))
                return false;

            if (string.Equals(gridType, "1", StringComparison.OrdinalIgnoreCase))
                return LoadRatesForGridType1();
            else if (string.Equals(gridType, "2", StringComparison.OrdinalIgnoreCase))
                return LoadRatesForGridType2(gridNode);

            return false;
        }

        private bool LoadRatesForGridType1()
        {
            _descriptor.TreatmentZones = new List<int>();
            try
            {
                string filePath = Path.ChangeExtension(Path.Combine(_baseFolder, _gridFileName), ".bin");
                using (var fileStream = File.OpenRead(filePath))
                {
                    int treatmentZoneId;
                    while (true)
                    {
                        treatmentZoneId = fileStream.ReadByte();
                        if (treatmentZoneId == -1)
                            break;

                        _descriptor.TreatmentZones.Add(treatmentZoneId);
                    }
                }
            }
            catch (IOException)
            {
                return false;
            }

            return _descriptor.TreatmentZones.Count == _descriptor.RowCount * _descriptor.ColumnCount;
        }

        private bool LoadRatesForGridType2(XmlNode inputNode)
        {
            int numberOfRatesPerCell = CountNumberOfRatesPerCell(inputNode);
            if (numberOfRatesPerCell < 0)
                return false;

            _descriptor.ProductRates = new List<List<int>>();
            try
            {
                string filePath = Path.ChangeExtension(Path.Combine(_baseFolder, _gridFileName), ".bin");
                using (var fileStream = File.OpenRead(filePath))
                {
                    var bytes = new byte[4];
                    var rates = new List<int>();
                    var rateCount = 0;

                    while (true)
                    {
                        var result = fileStream.Read(bytes, 0, bytes.Length);
                        if (result == 0)
                            break;

                        rateCount++;
                        rates.Add(BitConverter.ToInt32(bytes, 0));

                        if (rateCount == numberOfRatesPerCell)
                        {
                            _descriptor.ProductRates.Add(rates);
                            rateCount = 0;
                            rates = new List<int>();
                        }
                    }
                }
            }
            catch (IOException)
            {
                return false;
            }

            return _descriptor.ProductRates.Count == _descriptor.RowCount * _descriptor.ColumnCount;
        }

        private int CountNumberOfRatesPerCell(XmlNode inputNode)
        {
            int treatmentZoneId;
            if (inputNode.GetXmlNodeValue("@J").ParseValue(out treatmentZoneId))
                return -1;

            _descriptor.ProductRateTemplateId = treatmentZoneId;

            var treatmentZone = _treatmentZones.FindById(treatmentZoneId);
            if (treatmentZone == null || treatmentZone.Variables == null)
                return 0;
            return treatmentZone.Variables.Count;
        }
    }
}
