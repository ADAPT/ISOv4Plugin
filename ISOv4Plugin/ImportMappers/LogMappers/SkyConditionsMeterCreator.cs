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
    public class SkyConditionsMeterCreator : IEnumeratedMeterCreator
    {
        public int DDI { get; set; }

        public int StartingSection { get; set; }

        public SkyConditionsMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows)
        {
            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtSkyCondition.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            return new List<ISOEnumeratedMeter> { meter };
        }

        
        public EnumeratedValue GetValueForMeter(SpatialValue value, EnumeratedMeter meter)
        {
            if (Convert.ToInt32(value.Dlv.A, 16) != DDI)
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

        public UInt32 GetMetersValue(List<Meter> meters, SpatialRecord spatialRecord)
        {
            var meter = (ISOEnumeratedMeter) meters.FirstOrDefault();
            var value = (EnumeratedValue) spatialRecord.GetMeterValue(meter);

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
