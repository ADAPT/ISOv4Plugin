using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IEnumeratedMeterCreator
    {
        int DDI { get; set; }
        int StartingSection { get; }
        List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows);  //CondensedWorkstateMeter is necessarily forcing CreateMeters process to read spatial data to create meters.
        EnumeratedValue GetValueForMeter(SpatialValue value, ISOEnumeratedMeter workingData);
        UInt32 GetMetersValue(List<WorkingData> meters, SpatialRecord spatialRecord);
    }

    public class ISOEnumeratedMeter : EnumeratedWorkingData
    {
        public Func<SpatialValue, ISOEnumeratedMeter, EnumeratedValue> GetEnumeratedValue { get; set;}
        public int SectionIndex { get; set; }
    }
}
