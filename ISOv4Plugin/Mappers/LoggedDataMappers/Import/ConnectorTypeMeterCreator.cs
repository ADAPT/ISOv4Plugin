using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public class ConnectorTypeMeterCreator : IEnumeratedMeterCreator
    {
        public int DDI { get; set; }

        public int StartingSection { get; set; }

        public ConnectorTypeMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows, ISODataLogValue dlv)
        {
            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtHitchType.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            return new List<ISOEnumeratedMeter> { meter };
        }

        public EnumeratedValue GetValueForMeter(SpatialValue value, ISOEnumeratedMeter meter)
        {
            if (Convert.ToInt32(value.DataLogValue.ProcessDataDDI, 16) != DDI)
                return null;

            ApplicationDataModel.Representations.EnumerationMember enumMember = null;

            if (value.Value == 1)
                enumMember = DefinedTypeEnumerationInstanceList.dtiDrawbar.ToModelEnumMember();
            else if (value.Value == 2)
                enumMember = DefinedTypeEnumerationInstanceList.dtiRearTwoPoint.ToModelEnumMember();
            else if (value.Value == 3)
                enumMember = DefinedTypeEnumerationInstanceList.dtiThreePoint.ToModelEnumMember();
            else if (value.Value == 7)
                enumMember = DefinedTypeEnumerationInstanceList.dtiRearPivotWagonHitch.ToModelEnumMember();

            if (enumMember == null)
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
            var meter = (ISOEnumeratedMeter) meters.FirstOrDefault();
            var value = (EnumeratedValue) spatialRecord.GetMeterValue(meter);

            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiDrawbar.ToModelEnumMember().Code)
                return 1;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiRearTwoPoint.ToModelEnumMember().Code)
                return 2;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiThreePoint.ToModelEnumMember().Code)
                return 3;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiRearPivotWagonHitch.ToModelEnumMember().Code)
                return 7;

            return 0;
        }
    }
}
