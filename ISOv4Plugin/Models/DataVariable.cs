using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class DataVariable
    {
        public double Value { get; set; }
        public string ProductId { get; set; }
        public int Ddi { get; set; }
        public IsoUnit IsoUnit { get; set; }
        public UnitOfMeasure UserUnit { get; set; }
    }
}
