using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Representations;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal class DataVariable
    {
        public double Value { get; set; }
        public string ProductId { get; set; }
        public IsoUnit IsoUnit { get; set; }
        public UnitOfMeasure UserUnit { get; set; }
    }
}
