/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class SpatialValue
    {
        public int Id { get; set; }

        public ISODataLogValue DataLogValue { get; set; }

        public double Value { get; set; }

        public ISODeviceProcessData DeviceProcessData { get; set; }
    }
}
