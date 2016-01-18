using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.Plugins
{
    internal class GridDescriptor
    {
        // Treatment zones for grid type 1
        public List<int> TreatmentZones { get; set; }

        // Product rates for grid type 2
        public List<List<int>> ProductRates { get; set; }
        // Treatment zone template id for grid type 2
        public int ProductRateTemplateId { get; set; }

        public Point Origin { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public NumericRepresentationValue CellHeight { get; set; }
        public NumericRepresentationValue CellWidth { get; set; }
    }
}
