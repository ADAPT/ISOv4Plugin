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
    public class ConnectorTypeMeterCreatorTest
    {
        private ConnectorTypeMeterCreator _creator;

        [SetUp]
        public void Setup()
        {
            _creator = new ConnectorTypeMeterCreator(157);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenReturnsSingleMeter()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenRepHitchType()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(RepresentationInstanceList.dtHitchType.ToModelRepresentation().Code, result[0].Representation.Code);
        }

        [Test]
        public void GivenValueOneWhenCreateThenReturnsDrawbar()
        {
            var value = MakeSpatialValue(1);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiDrawbar.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueTwoWhenCreateThenReturnsThreepoint()
        {
            var value = MakeSpatialValue(2);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiRearTwoPoint.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueThreeWhenCreateThenReturnsThreepoint()
        {
            var value = MakeSpatialValue(3);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiThreePoint.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueSevenWhenCreateThenReturnsPivotWagonHitch()
        {
            var value = MakeSpatialValue(7);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiRearPivotWagonHitch.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenMetersWithDrawbarWhenGetMetersValueThenOne()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter
            {
                GetEnumeratedValue = (value, meter) => new EnumeratedValue
                {
                    Value = DefinedTypeEnumerationInstanceList.dtiDrawbar.ToModelEnumMember(),
                },
            };
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiDrawbar.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x01, result);
        }
        
        [Test]
        public void GivenMetersWithRearTwoPointWhenGetMetersValueThenTwo()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter
            {
                GetEnumeratedValue = (value, meter) => new EnumeratedValue
                {
                    Value = DefinedTypeEnumerationInstanceList.dtiRearTwoPoint.ToModelEnumMember(),
                },
            };
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiRearTwoPoint.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x02, result);
        }

        [Test]
        public void GivenMetersWithThreePointWhenGetMetersValueThenThree()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiThreePoint.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x03, result);
        }

        [Test]
        public void GivenMetersWithRearPivotWagonHitchWhenGetMetersValueThenSeven()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiRearPivotWagonHitch.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x07, result);
        }

        [Test]
        public void GivenMetersWithUnknownHitchWhenGetMetersValueThenZero()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiUnload.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x00, result);
        }

        private static SpatialValue MakeSpatialValue(int value)
        {
            return new SpatialValue
            {
                Value = value,
                DlvHeader = new DLVHeader
                {
                    ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 157 }
                }
            };
        }

        private EnumeratedMeter CreateMeter()
        {
            return _creator.CreateMeters(null).Single();
        }
    }
}
