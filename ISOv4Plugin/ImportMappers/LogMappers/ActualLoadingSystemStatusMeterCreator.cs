using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public class ActualLoadingSystemStatusMeterCreator : IEnumeratedMeterCreator
    {
        public int DDI { get; set; }
        
        public int StartingSection { get; set; }

        public ActualLoadingSystemStatusMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows)
        {
            var unloadingMeter = new ISOEnumeratedMeter
            {
                SectionId = 1,
                Representation = RepresentationInstanceList.dtUnloadingAugerState.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            var loadingMeter = new ISOEnumeratedMeter
            {
                SectionId = 2,
                Representation = RepresentationInstanceList.dtUnloadingAugerState.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            var meters = new List<ISOEnumeratedMeter> { unloadingMeter, loadingMeter };

            return meters;
        }

        public EnumeratedValue GetValueForMeter(SpatialValue value, EnumeratedMeter meter)
        {
            if (Convert.ToInt32(value.Dlv.A, 16) != DDI)
                return null;

            byte twoBitsValue = GetValue((int)value.Value, meter.SectionId);

            ApplicationDataModel.Representations.EnumerationMember enumMember = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateError.ToModelEnumMember();

            if(twoBitsValue == 0)
                enumMember = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateDisabled.ToModelEnumMember();
            else if (twoBitsValue == 1)
                enumMember = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateEnabled.ToModelEnumMember();
            else if (twoBitsValue == 2)
                enumMember = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateError.ToModelEnumMember();
            else if (twoBitsValue == 3)
                enumMember = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateNotAvailable.ToModelEnumMember();

            return new EnumeratedValue
            {
                Representation = meter.Representation as ApplicationDataModel.Representations.EnumeratedRepresentation,
                Value = enumMember,
                Code = enumMember.Code
            };
        }

        public UInt32 GetMetersValue(List<Meter> meters, SpatialRecord spatialRecord)
        {
            var section1Meter = (ISOEnumeratedMeter)meters.SingleOrDefault(x => x.SectionId == 1);
            var section1Value = (EnumeratedValue)spatialRecord.GetMeterValue(section1Meter);
            var sectionOneIsoValue = ConvertToIsoValue(section1Value);
            var section2Meter = (ISOEnumeratedMeter)meters.SingleOrDefault(x => x.SectionId == 2);
            var section2Value = (EnumeratedValue)spatialRecord.GetMeterValue(section2Meter);
            var sectionTwoIsoValue = ConvertToIsoValue(section2Value);

            return Convert.ToUInt32(sectionOneIsoValue | (sectionTwoIsoValue << 8));
        }

        private int ConvertToIsoValue(EnumeratedValue value)
        {
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateDisabled.ToModelEnumMember().Code)
                return 0;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateEnabled.ToModelEnumMember().Code)
                return 1;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateError.ToModelEnumMember().Code)
                return 2;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateNotAvailable.ToModelEnumMember().Code)
                return 3;

            return 3;
        }


        private byte GetValue(int value, int section)
        {
            const int twoBitMask = 0x3;

            int shiftValue = 0;
            if (section == 2)
                shiftValue = 8;

            int shiftedValue = value >> shiftValue;
            int parsedValue = shiftedValue & twoBitMask;

            return (byte)parsedValue;
        }
    }
}
