using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class CondensedSectionOverrideStateMeterCreatorTest
    {
        private List<ISOSpatialRow> _isoSpatialRows;

        [SetUp]
        public void Setup()
        {
            _isoSpatialRows = new List<ISOSpatialRow>();
        }

        [Test]
        public void GivenCondensedWorkStateDdiWhenCreateMetersThenMeters()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(367).CreateMeters(_isoSpatialRows);

            Assert.AreEqual(16, result.Count);
        }

        [Test]
        public void GivenCondensedWorkStateDdiWhenCreateMetersThenMetersSectionsAreSet()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(367).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 1);
        }

        [Test]
        public void GivenCondensedWorkStateDdiWhenCreateMetersThenRepresentationIsSectionStatus()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(367).CreateMeters(_isoSpatialRows);

            result.ForEach(x => Assert.AreEqual(x.Representation.Code, RepresentationInstanceList.dtPrescriptionState.DomainId));
        }

        [Test]
        public void GivenCondensedWorkStateWithSomeStatesNotInstalledThenOnlyCreatesInstalledSections()
        {
            var spatialRow = new ISOSpatialRow();
            const long notInstalled = 3;
            long value = 0;
            for (int i = 15; i > 11; --i)
            {
                value |= (notInstalled << i * 2);
            }
            var spatialValue = new SpatialValue
            {
                Dlv = new DLV
                {
                    A = "16F"
                },
                Value = value
            };
            spatialRow.SpatialValues = new List<SpatialValue> { spatialValue };
            _isoSpatialRows = new List<ISOSpatialRow> { spatialRow };
            var result = new CondensedSectionOverrideStateMeterCreator(367).CreateMeters(_isoSpatialRows);

            Assert.AreEqual(12, result.Count);
        }

        [Test]
        public void GivenCondensedWorkStateWithNoInstalledWhenCreateMetersThenNoMetersCreated()
        {
            var spatialRow = new ISOSpatialRow();
            const long value = 0xFFFFFFFF;
            var spatialValue = new SpatialValue
            {
                Dlv = new DLV
                {
                    A = "16F"
                },
                Value = value
            };
            spatialRow.SpatialValues = new List<SpatialValue> { spatialValue };
            _isoSpatialRows = new List<ISOSpatialRow> { spatialRow };
            var result = new CondensedSectionOverrideStateMeterCreator(367).CreateMeters(_isoSpatialRows);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GivenDdiAndSpatialValueWhenGetValueForMeterThenPrescriptionOverriddenIsExtracted()
        {

            var spatialValue = new SpatialValue
            {
                Dlv = new DLV
                {
                    A = "16F"
                },
                Value = 1
            };

            var meter = new EnumeratedWorkingData
            {
                DeviceElementUseId = 1
            };

            var result = new CondensedSectionOverrideStateMeterCreator(367).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrescriptionOverridden.ToModelEnumMember().Value, result.Value.Value);
        }

        [Test]
        public void GivenDdiAndSpatialValueWhenGetValueForMeterThenPrescriptionUsedIsExtracted()
        {

            var spatialValue = new SpatialValue
            {
                Dlv = new DLV
                {
                    A = "16F"
                },
                Value = 0
            };

            var meter = new EnumeratedWorkingData
            {
                DeviceElementUseId = 1
            };

            var result = new CondensedSectionOverrideStateMeterCreator(367).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrescriptionUsed.ToModelEnumMember().Value, result.Value.Value);
        }

        [Test]
        public void GivenDdiAndSpatialValueWithErrorStateWhenGetValueForMeterThennPrescriptionUsedIsReturned()
        {
            var spatialValue = new SpatialValue
            {
                Dlv = new DLV
                {
                    A = "16F"
                },
                Value = 2
            };

            var meter = new EnumeratedWorkingData
            {
                DeviceElementUseId = 1
            };

            var result = new CondensedSectionOverrideStateMeterCreator(367).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrescriptionUsed.ToModelEnumMember().Value, result.Value.Value);
        }

        [Test]
        public void GivenDdiAndSpatialValueWithNotInstalledStateWhenGetValueForMeterThenPrescriptionNotUsedIsReturned()
        {
            var spatialValue = new SpatialValue
            {
                Dlv = new DLV
                {
                    A = "16F"
                },
                Value = 3
            };

            var meter = new EnumeratedWorkingData
            {
                DeviceElementUseId = 1
            };

            var result = new CondensedSectionOverrideStateMeterCreator(367).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrescriptionNotUsed.ToModelEnumMember().Value, result.Value.Value);
        }

        [Test]
        public void GivenDdi162WhenCreateMetersThenSectionsStartWith17()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(368).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 17);
        }

        [Test]
        public void GivenDdi163WhenCreateMetersThenSectionsStartWith33()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(369).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 33);
        }

        [Test]
        public void GivenDdi164WhenCreateMetersThenSectionsStartWith49()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(370).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 49);
        }

        [Test]
        public void GivenDdi165WhenCreateMetersThenSectionsStartWith65()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(371).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 65);
        }

        [Test]
        public void GivenDdi166WhenCreateMetersThenSectionsStartWith81()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(372).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 81);
        }

        [Test]
        public void GivenDdi167WhenCreateMetersThenSectionsStartWith97()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(373).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 97);
        }

        [Test]
        public void GivenDdi168WhenCreateMetersThenSectionsStartWith113()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(374).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 113);
        }

        [Test]
        public void GivenDdi169WhenCreateMetersThenSectionsStartWith129()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(375).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 129);
        }

        [Test]
        public void GivenDdi170WhenCreateMetersThenSectionsStartWith145()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(376).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 145);
        }

        [Test]
        public void GivenDdi171WhenCreateMetersThenSectionsStartWith161()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(377).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 161);
        }

        [Test]
        public void GivenDdi172WhenCreateMetersThenSectionsStartWith177()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(378).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 177);
        }

        [Test]
        public void GivenDdi173WhenCreateMetersThenSectionsStartWith193()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(379).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 193);
        }

        [Test]
        public void GivenDdi174WhenCreateMetersThenSectionsStartWith209()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(380).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 209);
        }

        [Test]
        public void GivenDdi175WhenCreateMetersThenSectionsStartWith225()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(381).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 225);
        }

        [Test]
        public void GivenDdi176WhenCreateMetersThenSectionsStartWith241()
        {
            var result = new CondensedSectionOverrideStateMeterCreator(382).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 241);
        }

        [Test]
        public void GivenMetersWithStartAt367WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 1);
            var creator = new CondensedSectionOverrideStateMeterCreator(367);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt368WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 17);
            var creator = new CondensedSectionOverrideStateMeterCreator(368);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt369WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 33);
            var creator = new CondensedSectionOverrideStateMeterCreator(369);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt370WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 49);
            var creator = new CondensedSectionOverrideStateMeterCreator(370);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt371WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 65);
            var creator = new CondensedSectionOverrideStateMeterCreator(371);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt372WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 81);
            var creator = new CondensedSectionOverrideStateMeterCreator(372);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt373WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 97);
            var creator = new CondensedSectionOverrideStateMeterCreator(373);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt374WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 113);
            var creator = new CondensedSectionOverrideStateMeterCreator(374);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt375WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 129);
            var creator = new CondensedSectionOverrideStateMeterCreator(375);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt376WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 145);
            var creator = new CondensedSectionOverrideStateMeterCreator(376);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt377WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 161);
            var creator = new CondensedSectionOverrideStateMeterCreator(377);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt378WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 177);
            var creator = new CondensedSectionOverrideStateMeterCreator(378);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt379WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 193);
            var creator = new CondensedSectionOverrideStateMeterCreator(379);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt380WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 209);
            var creator = new CondensedSectionOverrideStateMeterCreator(380);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt381WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 225);
            var creator = new CondensedSectionOverrideStateMeterCreator(381);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt382WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 241);
            var creator = new CondensedSectionOverrideStateMeterCreator(382);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt367AndUndefinedWhenGetMetersValueThenValuesCorrect()
        {
            var meters = new List<WorkingData>();
            for (int i = 1; i < 17; i++)
            {
                meters.Add(new ISOEnumeratedMeter { DeviceElementUseId = i });
            }

            var spatialRecord = new SpatialRecord();
            for (var i = 0; i < 16; i++)
            {
                if (i % 2 == 0)
                    spatialRecord.SetMeterValue(meters[i],
                        new EnumeratedValue
                        {
                            Value = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember()
                        });
                else
                    spatialRecord.SetMeterValue(meters[i],
                        new EnumeratedValue
                        {
                            Value = DefinedTypeEnumerationInstanceList.dtiPrescriptionOverridden.ToModelEnumMember()
                        });
            }

            var creator = new CondensedSectionOverrideStateMeterCreator(367);
            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x77777777, result);
        }

        private static List<WorkingData> CreateMeters(out SpatialRecord spatialRecord, int startSection)
        {
            var meters = new List<WorkingData>();
            for (int i = startSection; i < startSection + 16; i++)
            {
                meters.Add(new ISOEnumeratedMeter { DeviceElementUseId = i });
            }

            spatialRecord = new SpatialRecord();
            for (var i = 0; i < 16; i++)
            {
                if (i % 2 == 0)
                    spatialRecord.SetMeterValue(meters[i],
                        new EnumeratedValue
                        {
                            Value = DefinedTypeEnumerationInstanceList.dtiPrescriptionUsed.ToModelEnumMember()
                        });
                else
                    spatialRecord.SetMeterValue(meters[i],
                        new EnumeratedValue
                        {
                            Value = DefinedTypeEnumerationInstanceList.dtiPrescriptionOverridden.ToModelEnumMember()
                        });
            }
            return meters;
        }

        private static void CheckConsolidatedWorkStateSectionIds(List<ISOEnumeratedMeter> result, int startingSectionNumber)
        {
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(i + startingSectionNumber, result[i].DeviceElementUseId);
            }
        }
    }
}
