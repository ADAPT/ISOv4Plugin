using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Export
{
    public class SpatialValueAssert
    {
        public static void AreEqual(ISOSpatialRow isoSpatialRow, SpatialRecord adaptSpatialRecord, List<Meter> meters)
        {
            foreach (var meter in meters)
            {
                var adaptValue = adaptSpatialRecord.GetMeterValue(meter);
                if(adaptValue == null)
                    continue;

                var isoValue = isoSpatialRow.SpatialValues.Single(v => v.Id == meter.Id.FindIntIsoId());

                var adaptEnumeratedValue = adaptValue as EnumeratedValue;
                if (adaptEnumeratedValue != null)
                {
                    var representation = RepresentationManager.Instance.Representations.First(x => x.DomainId == adaptEnumeratedValue.Representation.Code);
                    Assert.AreEqual(representation.Ddi.GetValueOrDefault(), isoValue.DlvHeader.ProcessDataDDI.Value);

                    if(representation.Ddi == 141)
                        CompareWorkState(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi == 157)
                        CompareConnectorType(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi == 158)
                        ComparePrescriptionControl(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi == 160)
                        CompareSectionControlState(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi >= 161 && representation.Ddi <= 176)
                        CompareCondensedWorkState(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi == 210)
                        CompareSkyConditions(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi == 230)
                        CompareNetWeightState(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi == 240)
                        CompareActualLoadingSystemStatus(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi >= 290 && representation.Ddi < 305)
                        CompareCondensedWorkState(adaptEnumeratedValue, isoValue);
                    if (representation.Ddi >= 367 && representation.Ddi <= 382)
                        CompareCondensedSectionOverrideState(adaptEnumeratedValue, isoValue);
                }

                var adaptNumericValue = adaptValue as NumericRepresentationValue;
                if (adaptNumericValue != null)
                {
                    var representation = RepresentationManager.Instance.Representations.FirstOrDefault(x => x.DomainId == adaptNumericValue.Representation.Code);
                    if (representation != null)
                        Assert.AreEqual(representation.Ddi, isoValue.DlvHeader.ProcessDataDDI.Value);

                    Assert.AreEqual(adaptNumericValue.Value.Value, isoValue.Value);
                }
            }
        }

        private static void CompareCondensedSectionOverrideState(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrescriptionUsed.ToModelEnumMember().Code)
                Assert.AreEqual(0, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrescriptionOverridden.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
        }

        private static void CompareActualLoadingSystemStatus(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateDisabled.ToModelEnumMember().Code)
                Assert.AreEqual(0, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateEnabled.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateError.ToModelEnumMember().Code)
                Assert.AreEqual(2, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateNotAvailable.ToModelEnumMember().Code)
                Assert.AreEqual(3, isoValue.Value);
        }

        private static void CompareNetWeightState(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiWeightUnStable.ToModelEnumMember().Code)
                Assert.AreEqual(0, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiWeightStable.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
        }

        private static void CompareSkyConditions(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiClear.ToModelEnumMember().Code)
                Assert.AreEqual(0x20524C43, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember().Code)
                Assert.AreEqual(0x2043534E, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiPartlyCloudy.ToModelEnumMember().Code)
                Assert.AreEqual(0x20544353, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiCloudy.ToModelEnumMember().Code)
                Assert.AreEqual(0x2043564F, isoValue.Value);
        }

        private static void CompareCondensedWorkState(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember().Code)
                Assert.AreEqual(0, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiRecordingStatusOn.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
        }

        private static void CompareSectionControlState(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiSCMasterManualOff.ToModelEnumMember().Code)
                Assert.AreEqual(0, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiSCMasterAutoOn.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiSCMasterError.ToModelEnumMember().Code)
                Assert.AreEqual(2, isoValue.Value);
        }

        private static void ComparePrescriptionControl(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrscMasterManualOff.ToModelEnumMember().Code)
                Assert.AreEqual(0, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrscMasterAutoOn.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiPrscMasterError.ToModelEnumMember().Code)
                Assert.AreEqual(2, isoValue.Value);
        }

        private static void CompareConnectorType(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiDrawbar.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiRearTwoPoint.ToModelEnumMember().Code)
                Assert.AreEqual(2, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiThreePoint.ToModelEnumMember().Code)
                Assert.AreEqual(3, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiRearPivotWagonHitch.ToModelEnumMember().Code)
                Assert.AreEqual(7, isoValue.Value);
        }

        private static void CompareWorkState(EnumeratedValue adaptEnumeratedValue, SpatialValue isoValue)
        {
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember().Code)
                Assert.AreEqual(0, isoValue.Value);
            if (adaptEnumeratedValue.Value.Code == DefinedTypeEnumerationInstanceList.dtiRecordingStatusOn.ToModelEnumMember().Code)
                Assert.AreEqual(1, isoValue.Value);
        }
    }
}