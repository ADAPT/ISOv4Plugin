using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class NetWeightStateMeterCreatorTest
    {
        private NetWeightStateMeterCreator _creator;

        [SetUp]
        public void Setup()
        {
            _creator = new NetWeightStateMeterCreator(230);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenReturnsSingleMeter()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenRepMeasuredWeightStatus()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(RepresentationInstanceList.dtMeasuredWeightStatus.ToModelRepresentation().Code, result[0].Representation.Code);
        }

        [Test]
        public void GivenValueZeroWhenGetValueThenReturnsUnstable()
        {
            var value = MakeSpatialValue(0);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiWeightUnStable.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueOneWhenGetValueThenReturnsStable()
        {
            var value = MakeSpatialValue(1);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiWeightStable.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueTwoWhenGetValueThenReturnsError()
        {
            var value = MakeSpatialValue(2);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiWeightError.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenUnknownValueWhenGetValueThenReturnsNull()
        {
            var value = MakeSpatialValue(3);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.IsNull(result);
        }

        [Test]
        public void GivenMetersWithUnStableWhenGetMetersValueThenIsZero()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiWeightUnStable.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x00, result);
        }

        [Test]
        public void GivenMetersWithUnStableWhenGetMetersValueThenIsOne()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiWeightStable.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x01, result);
        }

        [Test]
        public void GivenMetersWithUnStableWhenGetMetersValueThenIsTwo()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiWeightError.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x02, result);
        }

        private static SpatialValue MakeSpatialValue(int value)
        {
            return new SpatialValue
            {
                Value = value,
                Dlv = new DLV
                {
                    A = "E6"
                },
            };
        }

        private EnumeratedMeter CreateMeter()
        {
            return _creator.CreateMeters(null).Single();
        }

    }
}
