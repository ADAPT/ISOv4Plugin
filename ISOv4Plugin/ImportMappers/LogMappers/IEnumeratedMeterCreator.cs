using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IEnumeratedMeterCreator
    {
        int DDI { get; set; }
        int StartingSection { get; }
        List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows);
        EnumeratedValue GetValueForMeter(SpatialValue value, EnumeratedMeter meter);
        UInt32 GetMetersValue(List<Meter> meters, SpatialRecord spatialRecord);
    }

    public class ISOEnumeratedMeter : EnumeratedMeter
    {
        public Func<SpatialValue, ISOEnumeratedMeter, EnumeratedValue> GetEnumeratedValue { get; set;}   
    }
}