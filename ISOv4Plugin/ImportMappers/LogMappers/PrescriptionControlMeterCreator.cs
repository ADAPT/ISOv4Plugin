using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using EnumeratedRepresentation = AgGateway.ADAPT.ApplicationDataModel.Representations.EnumeratedRepresentation;
using EnumerationMember = AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public class PrescriptionControlMeterCreator : IEnumeratedMeterCreator
    {
        public PrescriptionControlMeterCreator(int ddi)
        {
            DDI = ddi;
        }

        public int DDI { get; set; }
        public int StartingSection { get; private set; }
        
        public List<ISOEnumeratedMeter> CreateMeters(IEnumerable<ISOSpatialRow> spatialRows)
        {
            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtPrescriptionControlMasterState.ToModelRepresentation(),
                GetEnumeratedValue = GetValueForMeter
            };

            return new List<ISOEnumeratedMeter> { meter };
        }

        public EnumeratedValue GetValueForMeter(SpatialValue value, EnumeratedWorkingData meter)
        {
            if (value == null)
                return null;

            EnumerationMember enumMember;

            if ((int)value.Value == 0)
                enumMember = DefinedTypeEnumerationInstanceList.dtiPrscMasterManualOff.ToModelEnumMember();
            else if ((int)value.Value == 1)
                enumMember = DefinedTypeEnumerationInstanceList.dtiPrscMasterAutoOn.ToModelEnumMember();
            else if ((int)value.Value == 2)
                enumMember = DefinedTypeEnumerationInstanceList.dtiPrscMasterError.ToModelEnumMember();
            else if ((int)value.Value == 3)
                enumMember = DefinedTypeEnumerationInstanceList.dtiPrscMasterUndefined.ToModelEnumMember();
            else
                enumMember = DefinedTypeEnumerationInstanceList.dtiPrscMasterError.ToModelEnumMember();

            return new EnumeratedValue
            {
                Representation = meter.Representation as EnumeratedRepresentation,
                Value = enumMember,
                Code = enumMember.Code
            };
        }

        public UInt32 GetMetersValue(List<WorkingData> meters, SpatialRecord spatialRecord)
        {
            var meter = (ISOEnumeratedMeter) meters.FirstOrDefault();
            var value = (EnumeratedValue)spatialRecord.GetMeterValue(meter);

            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrscMasterManualOff.ToModelEnumMember().Code)
                return 0;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrscMasterAutoOn.ToModelEnumMember().Code)
                return 1;
            if (value.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrscMasterError.ToModelEnumMember().Code)
                return 2;

            return 3;
        }
    }
}
