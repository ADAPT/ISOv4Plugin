using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class GridLoader
    {
        private GridDescriptor _descriptor;
        private string _gridFileName;
        private string _baseFolder;

        private GridLoader(string baseFolder)
        {
            _descriptor = new GridDescriptor();
            _baseFolder = baseFolder;
        }

        internal static GridDescriptor Load(XmlNode inputNode, string baseFolder)
        {
            var loader = new GridLoader(baseFolder);
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
            if (!LoadMinPosition(inputNode))
                return false;

            if (!LoadMaxPosition(inputNode))
                return false;

            return true;
        }

        private bool LoadMinPosition(XmlNode inputNode)
        {
            double minLatitude;
            if (!inputNode.GetXmlNodeValue("@A").ParseValue(out minLatitude) ||
                minLatitude < -90 || minLatitude > 90)
                return false;

            double minLongitude;
            if (!inputNode.GetXmlNodeValue("@B").ParseValue(out minLongitude) ||
                minLongitude < -180 || minLongitude > 180)
                return false;

            _descriptor.BoundingBox = new BoundingBox();
            _descriptor.BoundingBox.MinLatitude = minLatitude;
            _descriptor.BoundingBox.MinLongitude = minLongitude;

            return true;
        }

        private bool LoadMaxPosition(XmlNode inputNode)
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
            _descriptor.RowLength = gridCellSizeLatitude;
            _descriptor.ColumnLength = gridCellSizeLongitude;
            _descriptor.BoundingBox.MaxLatitude = _descriptor.BoundingBox.MinLatitude + gridCellSizeLatitude * rowCount;
            _descriptor.BoundingBox.MaxLongitude = _descriptor.BoundingBox.MinLongitude + gridCellSizeLongitude * columnCount;

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
                string filePath = Path.Combine(_baseFolder, _gridFileName);
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

            return true;
        }

        private bool LoadRatesForGridType2(XmlNode inputNode)
        {
            int numberOfRatesPerCell = CountNumberOfRatesPerCell(inputNode);
            if (numberOfRatesPerCell < 0)
                return false;

            _descriptor.ProductRates = new List<List<int>>();
            try
            {
                string filePath = Path.Combine(_baseFolder, _gridFileName);
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

            return true;
        }

        private int CountNumberOfRatesPerCell(XmlNode inputNode)
        {
            var treatmentZoneId = inputNode.GetXmlNodeValue("@J");
            if (string.IsNullOrEmpty(treatmentZoneId))
                return -1;

            _descriptor.ProductRateTemplateId = treatmentZoneId;

            var dataVariableNodes = inputNode.SelectNodes(string.Format(CultureInfo.InvariantCulture, "TZN[@A='{0}']/PDV", treatmentZoneId));
            return dataVariableNodes.Count;
        }
    }
}
