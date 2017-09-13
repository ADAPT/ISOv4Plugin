/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class GridDescriptor
    {
        // Treatment zones for grid type 1
        public List<int> TreatmentZoneCodes { get; set; }

        // Product rates for grid type 2
        public List<List<double>> ProductRates { get; set; }
        // Treatment zone template id for grid type 2
        public int ProductRateTemplateId { get; set; }

        public Point Origin { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public NumericRepresentationValue CellHeight { get; set; }
        public NumericRepresentationValue CellWidth { get; set; }

        internal bool LoadGridDefinition(ISOGrid grid)
        {
            if (!LoadOriginPosition(grid))
            {
                return false;
            }

            if (!LoadCellDimensions(grid))
            {
                return false;
            }

            return true;
        }

        private bool LoadOriginPosition(ISOGrid grid)
        {
            decimal minLatitude = grid.GridMinimumNorthPosition;
            if (minLatitude < -90 || minLatitude > 90)
            {
                return false;
            }

            decimal minLongitude = grid.GridMinimumEastPosition;
            if (minLongitude < -180 || minLongitude > 180)
            {
                return false;
            }

            Origin = new Point();
            Origin.Y = (double)minLatitude;
            Origin.X = (double)minLongitude;

            return true;
        }

        private bool LoadCellDimensions(ISOGrid grid)
        {
            double gridCellSizeLatitude = grid.GridCellNorthSize;
            if (gridCellSizeLatitude < 0 || gridCellSizeLatitude > 1)
            {
                return false;
            }

            double gridCellSizeLongitude = grid.GridCellEastSize;
            if (gridCellSizeLongitude < 0 || gridCellSizeLongitude > 1)
            {
                return false;
            }

            long columnCount = grid.GridMaximumColumn;
            if (columnCount < 0)
            {
                return false;
            }

            long rowCount = grid.GridMaximumRow;
            if (rowCount < 0)
            {
                return false;
            }

            ColumnCount = (int)columnCount;
            RowCount = (int)rowCount;
            CellHeight = new NumericRepresentationValue(null, new NumericValue(null, gridCellSizeLatitude));
            CellWidth = new NumericRepresentationValue(null, new NumericValue(null, gridCellSizeLongitude));

            return true;
        }

        internal bool LoadRates(string dataPath, ISOGrid grid, ISOTreatmentZone treatmentZone = null)
        {
            if (string.IsNullOrEmpty(grid.Filename))
            {
                return false;
            }

            if (grid.GridType == 1)
            {
                TreatmentZoneCodes = grid.GetRatesForGridType1(dataPath);
                return TreatmentZoneCodes.Count == RowCount * ColumnCount;
            }
            else if (grid.GridType == 2)
            {
                ProductRates = grid.GetRatesForGridType2(dataPath, treatmentZone);
                return ProductRates.Count == RowCount * ColumnCount;
            }
            else
            {
                return false;
            }
        }
    }
}
