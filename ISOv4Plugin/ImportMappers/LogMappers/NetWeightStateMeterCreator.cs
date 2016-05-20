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
    public class NetWeightStateMeterCreator : IEnumeratedMeterCreator
    {
        public int DDI { get; set; }

        public int StartingSection { get; set; }

        public NetWeightStateMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows)
        {
            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtMeasuredWeightStatus.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            return new List<ISOEnumeratedMeter> { meter };
        }

        public EnumeratedValue GetValueForMeter(SpatialValue value, EnumeratedMeter meter)
        {
            if (Convert.ToInt32(value.Dlv.A, 16) != DDI)
                return null;

            ApplicationDataModel.Representations.EnumerationMember enumMember;

            if (value.Value == 0)
                enumMember = DefinedTypeEnumerationInstanceList.dtiWeightUnStable.ToModelEnumMember();
            else if (value.Value == 1)
                enumMember = DefinedTypeEnumerationInstanceList.dtiWeightStable.ToModelEnumMember();
            else if (value.Value == 2)
                enumMember = DefinedTypeEnumerationInstanceList.dtiWeightError.ToModelEnumMember();
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
            var meter = (ISOEnumeratedMeter)meters.SingleOrDefault();
            var value = (EnumeratedValue)spatialRecord.GetMeterValue(meter);

            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiWeightUnStable.ToModelEnumMember().Code)
                return 0;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiWeightStable.ToModelEnumMember().Code)
                return 1;

            return 2;
        }
    }
}
