using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using NUnit.Framework;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System.Collections.Generic;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class AcutalLoadingSystemStatusMeterCreatorTest
    {
        private ActualLoadingSystemStatusMeterCreator _creator;

        [SetUp]
        public void Setup()
        {
            _creator = new ActualLoadingSystemStatusMeterCreator(240);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenReturnsTwoMeters()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenRepIsUnloadingAugerState()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(RepresentationInstanceList.dtUnloadingAugerState.ToModelRepresentation().Code, result[0].Representation.Code);
            Assert.AreEqual(RepresentationInstanceList.dtUnloadingAugerState.ToModelRepresentation().Code, result[1].Representation.Code);
        }

        [Test]
        public void GivenUnloadingValueZeroThenReturnsDisabled()
        {
            var value = MakeSpatialValue(0xFF00);
            var result = _creator.GetValueForMeter(value, CreateMeters()[0]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateDisabled.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenLoadingValueZeroThenReturnsDisabled()
        {
            var value = MakeSpatialValue(0x00FF);
            var result = _creator.GetValueForMeter(value, CreateMeters()[1]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateDisabled.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenUnloadingValueOneThenReturnsEnabled()
        {
            var value = MakeSpatialValue(0xFF01);
            var result = _creator.GetValueForMeter(value, CreateMeters()[0]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateEnabled.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenLoadingValueOneThenReturnsEnabled()
        {
            var value = MakeSpatialValue(0x01FF);
            var result = _creator.GetValueForMeter(value, CreateMeters()[1]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateEnabled.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenUnloadingValueTwoThenReturnsError()
        {
            var value = MakeSpatialValue(0xFF02);
            var result = _creator.GetValueForMeter(value, CreateMeters()[0]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateError.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenLoadingValueTwoThenReturnsError()
        {
            var value = MakeSpatialValue(0x02FF);
            var result = _creator.GetValueForMeter(value, CreateMeters()[1]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateError.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenUnloadingValueThreeThenReturnsError()
        {
            var value = MakeSpatialValue(0x0003);
            var result = _creator.GetValueForMeter(value, CreateMeters()[0]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateNotAvailable.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenLoadingValueThreeThenReturnsError()
        {
            var value = MakeSpatialValue(0x0300);
            var result = _creator.GetValueForMeter(value, CreateMeters()[1]);

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateNotAvailable.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenMetersWhenGetMetersValueThenValuesReturned()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter
            {
                SectionId = 1,
            };
            var enumeratedMeter2 = new ISOEnumeratedMeter
            {
                SectionId = 2,
            };
            var meters = new List<Meter> { enumeratedMeter1, enumeratedMeter2 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateDisabled.ToModelEnumMember() });
            spatialRecord.SetMeterValue(enumeratedMeter2, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateEnabled.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x0100, result);
        }

        [Test]
        public void GivenMetersErrorAndNotAvailableWhenGetMetersValueThenValuesReturned()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter
            {
                SectionId = 1,
            };
            var enumeratedMeter2 = new ISOEnumeratedMeter
            {
                SectionId = 2,
            };
            var meters = new List<Meter> { enumeratedMeter1, enumeratedMeter2 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateError.ToModelEnumMember() });
            spatialRecord.SetMeterValue(enumeratedMeter2, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiUnloadingAugerStateNotAvailable.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x0302, result);
        }

        [Test]
        public void GivenMetersUnknownWhenGetMetersValueThenValuesReturned()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter
            {
                SectionId = 1,
            };
            var enumeratedMeter2 = new ISOEnumeratedMeter
            {
                SectionId = 2,
            };
            var meters = new List<Meter> { enumeratedMeter1, enumeratedMeter2 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember() });
            spatialRecord.SetMeterValue(enumeratedMeter2, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x0303, result);
        }



        private static SpatialValue MakeSpatialValue(int value)
        {
            return new SpatialValue
            {
                Value = value,
                DlvHeader = new DLVHeader
                {
                    ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 240 }
                }
            };
        }

        private List<ISOEnumeratedMeter> CreateMeters()
        {
            return _creator.CreateMeters(null);
        }
    }
}
