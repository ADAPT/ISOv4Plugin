using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class CondensedWorkStateMeterCreatorTest
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
            var result = new CondensedWorkStateMeterCreator(161, 161).CreateMeters(_isoSpatialRows);

            Assert.AreEqual(16, result.Count);
        }

        [Test]
        public void GivenCondensedWorkStateDdiWhenCreateMetersThenMetersSectionsAreSet()
        {
            var result = new CondensedWorkStateMeterCreator(161, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 1);
        }

        [Test]
        public void GivenCondensedWorkStateDdiWhenCreateMetersThenRepresentationIsSectionStatus()
        {
            var result = new CondensedWorkStateMeterCreator(161, 161).CreateMeters(_isoSpatialRows);

            result.ForEach(x => Assert.AreEqual(x.Representation.Code, RepresentationInstanceList.dtRecordingStatus.DomainId));
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
                DlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 161 }  },
                Value = value
            };
            spatialRow.SpatialValues = new List<SpatialValue> { spatialValue };
            _isoSpatialRows = new List<ISOSpatialRow> { spatialRow };
            var result = new CondensedWorkStateMeterCreator(161, 161).CreateMeters(_isoSpatialRows);

            Assert.AreEqual(12, result.Count);
        }

        [Test]
        public void GivenCondensedWorkStateWithNoInstalledWhenCreateMetersThenNoMetersCreated()
        {
            var spatialRow = new ISOSpatialRow();
            const long value = 0xFFFFFFFF;
            var spatialValue = new SpatialValue
            {
                DlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 161 }  },
                Value = value
            };
            spatialRow.SpatialValues = new List<SpatialValue> { spatialValue };
            _isoSpatialRows = new List<ISOSpatialRow> { spatialRow };
            var result = new CondensedWorkStateMeterCreator(161, 161).CreateMeters(_isoSpatialRows);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GivenDdiAndSpatialValueWhenGetValueForMeterThenSectionValueOnIsExtracted()
        {

            var spatialValue = new SpatialValue
            {
                DlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 161 }  },
                Value = 1
            };

            var meter = new EnumeratedMeter
            {
                SectionId = 1
            };

            var result = new CondensedWorkStateMeterCreator(161, 161).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiRecordingStatusOn.ToModelEnumMember().Value, result.Value.Value);
        }

        [Test]
        public void GivenDdiAndSpatialValueWhenGetValueForMeterThenSectionValueOffIsExtracted()
        {

            var spatialValue = new SpatialValue
            {
                DlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 161 }  },
                Value = 0
            };

            var meter = new EnumeratedMeter
            {
                SectionId = 1
            };

            var result = new CondensedWorkStateMeterCreator(161, 161).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember().Value, result.Value.Value);
        }

        [Test]
        public void GivenDdiAndSpatialValueWithErrorStateWhenGetValueForMeterThenSectionValueOffIsReturned()
        {
            var spatialValue = new SpatialValue
            {
                DlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 161 }  },
                Value = 2
            };

            var meter = new EnumeratedMeter
            {
                SectionId = 1
            };

            var result = new CondensedWorkStateMeterCreator(161, 161).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember().Value, result.Value.Value);
        }

        [Test]
        public void GivenDdiAndSpatialValueWithNotInstalledStateWhenGetValueForMeterThenSectionValueOffIsReturned()
        {
            var spatialValue = new SpatialValue
            {
                DlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 161 }  },
                Value = 3
            };

            var meter = new EnumeratedMeter
            {
                SectionId = 1
            };

            var result = new CondensedWorkStateMeterCreator(161, 161).GetValueForMeter(spatialValue, meter);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember().Value, result.Value.Value);
        }


        [Test]
        public void GivenDdi162WhenCreateMetersThenSectionsStartWith17()
        {
            var result = new CondensedWorkStateMeterCreator(162, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 17);
        }

        [Test]
        public void GivenDdi163WhenCreateMetersThenSectionsStartWith33()
        {
            var result = new CondensedWorkStateMeterCreator(163, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 33);
        }

        [Test]
        public void GivenDdi164WhenCreateMetersThenSectionsStartWith49()
        {
            var result = new CondensedWorkStateMeterCreator(164, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 49);
        }

        [Test]
        public void GivenDdi165WhenCreateMetersThenSectionsStartWith65()
        {
            var result = new CondensedWorkStateMeterCreator(165, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 65);
        }

        [Test]
        public void GivenDdi166WhenCreateMetersThenSectionsStartWith81()
        {
            var result = new CondensedWorkStateMeterCreator(166, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 81);
        }

        [Test]
        public void GivenDdi167WhenCreateMetersThenSectionsStartWith97()
        {
            var result = new CondensedWorkStateMeterCreator(167, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 97);
        }

        [Test]
        public void GivenDdi168WhenCreateMetersThenSectionsStartWith113()
        {
            var result = new CondensedWorkStateMeterCreator(168, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 113);
        }

        [Test]
        public void GivenDdi169WhenCreateMetersThenSectionsStartWith129()
        {
            var result = new CondensedWorkStateMeterCreator(169, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 129);
        }

        [Test]
        public void GivenDdi170WhenCreateMetersThenSectionsStartWith145()
        {
            var result = new CondensedWorkStateMeterCreator(170, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 145);
        }

        [Test]
        public void GivenDdi171WhenCreateMetersThenSectionsStartWith161()
        {
            var result = new CondensedWorkStateMeterCreator(171, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 161);
        }

        [Test]
        public void GivenDdi172WhenCreateMetersThenSectionsStartWith177()
        {
            var result = new CondensedWorkStateMeterCreator(172, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 177);
        }

        [Test]
        public void GivenDdi173WhenCreateMetersThenSectionsStartWith193()
        {
            var result = new CondensedWorkStateMeterCreator(173, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 193);
        }

        [Test]
        public void GivenDdi174WhenCreateMetersThenSectionsStartWith209()
        {
            var result = new CondensedWorkStateMeterCreator(174, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 209);
        }

        [Test]
        public void GivenDdi175WhenCreateMetersThenSectionsStartWith225()
        {
            var result = new CondensedWorkStateMeterCreator(175, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 225);
        }

        [Test]
        public void GivenDdi176WhenCreateMetersThenSectionsStartWith241()
        {
            var result = new CondensedWorkStateMeterCreator(176, 161).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 241);
        }

        [Test]
        public void GivenDdi290WhenCreateMetersThenSectionsStartWith17()
        {
            var result = new CondensedWorkStateMeterCreator(290, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 1);
        }

        [Test]
        public void GivenDdi291WhenCreateMetersThenSectionsStartWith17()
        {
            var result = new CondensedWorkStateMeterCreator(291, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 17);
        }

        [Test]
        public void GivenDdi292WhenCreateMetersThenSectionsStartWith33()
        {
            var result = new CondensedWorkStateMeterCreator(292, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 33);
        }

        [Test]
        public void GivenDdi293WhenCreateMetersThenSectionsStartWith49()
        {
            var result = new CondensedWorkStateMeterCreator(293, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 49);
        }

        [Test]
        public void GivenDdi294WhenCreateMetersThenSectionsStartWith65()
        {
            var result = new CondensedWorkStateMeterCreator(294, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 65);
        }

        [Test]
        public void GivenDdi295WhenCreateMetersThenSectionsStartWith81()
        {
            var result = new CondensedWorkStateMeterCreator(295, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 81);
        }

        [Test]
        public void GivenDdi296WhenCreateMetersThenSectionsStartWith97()
        {
            var result = new CondensedWorkStateMeterCreator(296, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 97);
        }

        [Test]
        public void GivenDdi297WhenCreateMetersThenSectionsStartWith113()
        {
            var result = new CondensedWorkStateMeterCreator(297, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 113);
        }

        [Test]
        public void GivenDdi298WhenCreateMetersThenSectionsStartWith129()
        {
            var result = new CondensedWorkStateMeterCreator(298, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 129);
        }

        [Test]
        public void GivenDdi299WhenCreateMetersThenSectionsStartWith145()
        {
            var result = new CondensedWorkStateMeterCreator(299, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 145);
        }

        [Test]
        public void GivenDdi300WhenCreateMetersThenSectionsStartWith161()
        {
            var result = new CondensedWorkStateMeterCreator(300, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 161);
        }

        [Test]
        public void GivenDdi301WhenCreateMetersThenSectionsStartWith177()
        {
            var result = new CondensedWorkStateMeterCreator(301, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 177);
        }

        [Test]
        public void GivenDdi302WhenCreateMetersThenSectionsStartWith193()
        {
            var result = new CondensedWorkStateMeterCreator(302, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 193);
        }

        [Test]
        public void GivenDdi303WhenCreateMetersThenSectionsStartWith209()
        {
            var result = new CondensedWorkStateMeterCreator(303, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 209);
        }

        [Test]
        public void GivenDdi304WhenCreateMetersThenSectionsStartWith225()
        {
            var result = new CondensedWorkStateMeterCreator(304, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 225);
        }

        [Test]
        public void GivenDdi305WhenCreateMetersThenSectionsStartWith241()
        {
            var result = new CondensedWorkStateMeterCreator(305, 290).CreateMeters(_isoSpatialRows);

            CheckConsolidatedWorkStateSectionIds(result, 241);
        }

        [Test]
        public void GivenMetersWithStartAt161WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 1);
            var creator = new CondensedWorkStateMeterCreator(161, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt162WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 17);
            var creator = new CondensedWorkStateMeterCreator(162, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt163WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 33);
            var creator = new CondensedWorkStateMeterCreator(163, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt164WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 49);
            var creator = new CondensedWorkStateMeterCreator(164, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt165WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 65);
            var creator = new CondensedWorkStateMeterCreator(165, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt166WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 81);
            var creator = new CondensedWorkStateMeterCreator(166, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt167WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 97);
            var creator = new CondensedWorkStateMeterCreator(167, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt168WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 113);
            var creator = new CondensedWorkStateMeterCreator(168, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt169WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 129);
            var creator = new CondensedWorkStateMeterCreator(169, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt170WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 145);
            var creator = new CondensedWorkStateMeterCreator(170, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt171WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 161);
            var creator = new CondensedWorkStateMeterCreator(171, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt172WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 177);
            var creator = new CondensedWorkStateMeterCreator(172, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt173WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 193);
            var creator = new CondensedWorkStateMeterCreator(173, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt174WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 209);
            var creator = new CondensedWorkStateMeterCreator(174, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt175WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 225);
            var creator = new CondensedWorkStateMeterCreator(175, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt176WhenGetMetersValueThenValuesCorrect()
        {
            SpatialRecord spatialRecord;
            var meters = CreateMeters(out spatialRecord, 241);
            var creator = new CondensedWorkStateMeterCreator(176, 161);

            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x44444444, result);
        }

        [Test]
        public void GivenMetersWithStartAt161AndUndefinedWhenGetMetersValueThenValuesCorrect()
        {
            var meters = new List<Meter>();
            for (int i = 1; i < 17; i++)
            {
                meters.Add(new ISOEnumeratedMeter { SectionId = i });
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
                            Value = DefinedTypeEnumerationInstanceList.dtiRecordingStatusOn.ToModelEnumMember()
                        });
            }

            var creator = new CondensedWorkStateMeterCreator(161, 161);
            var result = creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x77777777, result);
        }

        private static List<Meter> CreateMeters(out SpatialRecord spatialRecord, int startSection)
        {
            var meters = new List<Meter>();
            for (int i = startSection; i < startSection + 16; i++)
            {
                meters.Add(new ISOEnumeratedMeter {SectionId = i});
            }

            spatialRecord = new SpatialRecord();
            for (var i = 0; i < 16; i++)
            {
                if (i%2 == 0)
                    spatialRecord.SetMeterValue(meters[i],
                        new EnumeratedValue
                        {
                            Value = DefinedTypeEnumerationInstanceList.dtiRecordingStatusOff.ToModelEnumMember()
                        });
                else
                    spatialRecord.SetMeterValue(meters[i],
                        new EnumeratedValue
                        {
                            Value = DefinedTypeEnumerationInstanceList.dtiRecordingStatusOn.ToModelEnumMember()
                        });
            }
            return meters;
        }

        private static void CheckConsolidatedWorkStateSectionIds(List<ISOEnumeratedMeter> result, int startingSectionNumber)
        {
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(i + startingSectionNumber, result[i].SectionId);
            }
        }
    }
}
