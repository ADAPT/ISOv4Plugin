using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public class SectionControlStateMeterCreator : IEnumeratedMeterCreator
    {
        public int DDI { get; set; }

        public int StartingSection { get; set; }

        public SectionControlStateMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows)
        {
            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtSectionControlMasterState.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            return new List<ISOEnumeratedMeter> { meter };
        }

        public EnumeratedValue GetValueForMeter(SpatialValue value, EnumeratedWorkingData meter)
        {
            if (Convert.ToInt32(value.DataLogValue.ProcessDataDDI, 16) != DDI)
                return null;

            ApplicationDataModel.Representations.EnumerationMember enumMember = DefinedTypeEnumerationInstanceList.dtiSCMasterUndefined.ToModelEnumMember();

            var reservedBitsMask = 0x00000003;
            var valueLowerTwoBits = (int)value.Value & reservedBitsMask;

            if(valueLowerTwoBits == 0)
                enumMember = DefinedTypeEnumerationInstanceList.dtiSCMasterManualOff.ToModelEnumMember();
            else if (valueLowerTwoBits == 1)
                enumMember = DefinedTypeEnumerationInstanceList.dtiSCMasterAutoOn.ToModelEnumMember();
            else if (valueLowerTwoBits == 2)
                enumMember = DefinedTypeEnumerationInstanceList.dtiSCMasterError.ToModelEnumMember();
            else if (valueLowerTwoBits == 3)
                enumMember = DefinedTypeEnumerationInstanceList.dtiSCMasterUndefined.ToModelEnumMember();

            return new EnumeratedValue
            {
                Representation = meter.Representation as ApplicationDataModel.Representations.EnumeratedRepresentation,
                Value = enumMember,
                Code = enumMember.Code
            };
        }

        public UInt32 GetMetersValue(List<WorkingData> meters, SpatialRecord spatialRecord)
        {
            var meter = meters.FirstOrDefault();
            var value = (EnumeratedValue)spatialRecord.GetMeterValue(meter);

            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiSCMasterManualOff.ToModelEnumMember().Code)
                return 0;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiSCMasterAutoOn.ToModelEnumMember().Code)
                return 1;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiSCMasterError.ToModelEnumMember().Code)
                return 2;
            
            return 3;
        }
    }
}
