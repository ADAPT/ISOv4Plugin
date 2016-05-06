using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using NUnit.Framework;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class SectionControlStateMeterCreatorTest
    {
        private SectionControlStateMeterCreator _creator;

        [SetUp]
        public void Setup()
        {
            _creator = new SectionControlStateMeterCreator(160);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenReturnsSingleMeter()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenRepSectionControlMasterState()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(RepresentationInstanceList.dtSectionControlMasterState.ToModelRepresentation().Code, result[0].Representation.Code);
        }

        [Test]
        public void GivenValueZeroWhenGetValueThenReturnsOff()
        {
            var value = MakeSpatialValue(0);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiSCMasterManualOff.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueOneWhenGetValueThenReturnsOn()
        {
            var value = MakeSpatialValue(1);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiSCMasterAutoOn.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueTwoWhenGetValueThenReturnsError()
        {
            var value = MakeSpatialValue(2);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiSCMasterError.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueThreeWhenGetValueThenReturnsUndefined()
        {
            var value = MakeSpatialValue(3);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiSCMasterUndefined.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenMetersWithManualOffWhenGetMetersValueThenZero()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> {enumeratedMeter};

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSCMasterManualOff.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x00, result);
        }

        [Test]
        public void GivenMetersWithAutoOnWhenGetMetersValueThenOne()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> {enumeratedMeter};

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSCMasterAutoOn.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x01, result);
        }

        [Test]
        public void GivenMetersWithErrorWhenGetMetersValueThenTwo()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> {enumeratedMeter};

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSCMasterError.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x02, result);
        }

        [Test]
        public void GivenMetersWithUndefinedWhenGetMetersValueThenThree()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> {enumeratedMeter};

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSCMasterUndefined.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x03, result);
        }

        private static SpatialValue MakeSpatialValue(int value)
        {
            return new SpatialValue
            {
                Value = value,
                DlvHeader = new DLVHeader
                {
                    ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 160 }
                }
            };
        }

        private EnumeratedMeter CreateMeter()
        {
            return _creator.CreateMeters(null).Single();
        }
    }
}
