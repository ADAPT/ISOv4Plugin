using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public class SkyConditionsMeterCreator : IEnumeratedMeterCreator
    {
        public int DDI { get; set; }

        public int StartingSection { get; set; }

        public SkyConditionsMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows, ISODataLogValue dlv)
        {
            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtSkyCondition.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };
            return new List<ISOEnumeratedMeter> { meter };
        }

        
        public EnumeratedValue GetValueForMeter(SpatialValue value, ISOEnumeratedMeter meter)
        {
            if (value.DataLogValue.ProcessDataDDI.AsInt32DDI() != DDI)
                return null;

            const int clear = 0x20524C43;        
            const int mostlySunny = 0x2043534E;
            const int partlySunny = 0x20574546;
            const int partlyCloudy = 0x20544353;
            const int mostlyCloudy = 0x204E4B42;
            const int overcast = 0x2043564F;

            ApplicationDataModel.Representations.EnumerationMember enumMember;

            if (value.Value == clear)
                enumMember = DefinedTypeEnumerationInstanceList.dtiClear.ToModelEnumMember();
            else if (value.Value == mostlySunny)
                enumMember = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember();
            else if (value.Value == partlySunny)
                enumMember = DefinedTypeEnumerationInstanceList.dtiPartlyCloudy.ToModelEnumMember();
            else if (value.Value == partlyCloudy)
                enumMember = DefinedTypeEnumerationInstanceList.dtiPartlyCloudy.ToModelEnumMember();
            else if (value.Value == mostlyCloudy)
                enumMember = DefinedTypeEnumerationInstanceList.dtiCloudy.ToModelEnumMember();
            else if (value.Value == overcast)
                enumMember = DefinedTypeEnumerationInstanceList.dtiCloudy.ToModelEnumMember();
            else
                return null;

            return new EnumeratedValue
            {
                Representation = meter.Representation as ApplicationDataModel.Representations.EnumeratedRepresentation,
                Value = enumMember,
                Code = enumMember.Code
            };
        }

        public UInt32 GetMetersValue(List<WorkingData> meters, SpatialRecord spatialRecord)
        {
            // Martin Sperlich: I've got a GS3_2630 data set here for which meters.FirstOrDefault()
            // does return an AgGateway.ADAPT.ApplicationDataModel.LoggedData.EnumeratedWorkingData object.
            // The direct cast to AgGateway.ADAPT.ApplicationDataModel.LoggedData.EnumeratedWorkingData
            // does throw an invalid typecast exception.

            // var meter = (ISOEnumeratedMeter) meters.FirstOrDefault();
            ISOEnumeratedMeter meter = meters.FirstOrDefault() as ISOEnumeratedMeter;
            if (meter == null) return 0;
            // var value = (EnumeratedValue) spatialRecord.GetMeterValue(meter);
            EnumeratedValue value = spatialRecord.GetMeterValue(meter) as EnumeratedValue;
            if (value == null) return 0;

            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiClear.ToModelEnumMember().Code)
                return 0x20524C43;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember().Code)
                return 0x2043534E;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiPartlyCloudy.ToModelEnumMember().Code)
                return 0x20544353;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiCloudy.ToModelEnumMember().Code)
                return 0x2043564F;
            return 0;
        }
    }
}
