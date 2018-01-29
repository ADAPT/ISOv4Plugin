using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using EnumeratedRepresentation = AgGateway.ADAPT.ApplicationDataModel.Representations.EnumeratedRepresentation;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public class WorkStateMeterCreator : IEnumeratedMeterCreator
    {
        public WorkStateMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public int DDI { get; set; }
        public int StartingSection { get; private set; }

        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows)
        {
            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtRecordingStatus.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            return new List<ISOEnumeratedMeter> {meter};
        }

        public EnumeratedValue GetValueForMeter(SpatialValue value, EnumeratedWorkingData meter)
        {
            var enumMember = value != null && value.Value == 1
                ? DefinedTypeEnumerationInstanceList.dtiRecordingStatusOn.ToModelEnumMember()
                : DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember();
            return new EnumeratedValue
            {
                Representation = meter.Representation as EnumeratedRepresentation,
                Value = enumMember,
                Code = enumMember.Code
            };
        }

        public UInt32 GetMetersValue(List<WorkingData> meters, SpatialRecord spatialRecord)
        {
            var meter = meters.SingleOrDefault();
            var value = (EnumeratedValue)spatialRecord.GetMeterValue(meter);

            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiRecordingStatusOn.ToModelEnumMember().Code)
                return 1;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember().Code)
                return 0;

            return 3;
        }
    }
}
