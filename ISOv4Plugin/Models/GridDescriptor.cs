using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class GridDescriptor
    {
        // Treatment zones for grid type 1
        public List<int> TreatmentZones { get; set; }

        // Product rates for grid type 2
        public List<List<double>> ProductRates { get; set; }
        // Treatment zone template id for grid type 2
        public int ProductRateTemplateId { get; set; }

        public Point Origin { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public NumericRepresentationValue CellHeight { get; set; }
        public NumericRepresentationValue CellWidth { get; set; }
    }
}
