using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class SpatialValue
    {
        public int Id { get; set; }

        public DLV Dlv { get; set; }

        public double Value { get; set; }
    }
}