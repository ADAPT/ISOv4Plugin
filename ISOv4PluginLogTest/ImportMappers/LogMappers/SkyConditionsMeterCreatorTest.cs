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
    public class SkyConditionsMeterCreatorTest
    {
        private SkyConditionsMeterCreator _creator;

        [SetUp]
        public void Setup()
        {
            _creator = new SkyConditionsMeterCreator(210);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenReturnsSingleMeter()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenRepSkyConditions()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(RepresentationInstanceList.dtSkyCondition.ToModelRepresentation().Code, result[0].Representation.Code);
        }

        [Test]
        public void GivenValueClrWhenGetValueThenReturnsClear()
        {
            var bytes = System.Text.Encoding.Default.GetBytes("CLR ").ToArray();
            var clear = System.BitConverter.ToInt32(bytes, 0);
            
            var value = MakeSpatialValue(clear);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiClear.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueNscWhenGetValueThenReturnsMostlySunny()
        {
            var bytes = System.Text.Encoding.Default.GetBytes("NSC ").ToArray();
            var mostlySunny = System.BitConverter.ToInt32(bytes, 0);

            var value = MakeSpatialValue(mostlySunny);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueFewWhenGetValueThenReturnsPartySunny()
        {
            var bytes = System.Text.Encoding.Default.GetBytes("FEW ").ToArray();
            var partlySunny = System.BitConverter.ToInt32(bytes, 0);

            var value = MakeSpatialValue(partlySunny);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPartlyCloudy.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueSctWhenGetValueThenReturnsPartlyCloudy()
        {
            var bytes = System.Text.Encoding.Default.GetBytes("SCT ").ToArray();
            var partlyCloudy = System.BitConverter.ToInt32(bytes, 0);

            var value = MakeSpatialValue(partlyCloudy);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPartlyCloudy.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueBknWhenGetValueThenReturnsMostlyCloudy()
        {
            var bytes = System.Text.Encoding.Default.GetBytes("BKN ").ToArray();
            var partlyCloudy = System.BitConverter.ToInt32(bytes, 0);

            var value = MakeSpatialValue(partlyCloudy);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiCloudy.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueOvcWhenGetValueThenReturnsCloudy()
        {
            var bytes = System.Text.Encoding.Default.GetBytes("OVC ").ToArray();
            var partlyCloudy = System.BitConverter.ToInt32(bytes, 0);

            var value = MakeSpatialValue(partlyCloudy);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiCloudy.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenUnknownValueWhenGetValueThenReturnsNull()
        {
            var value = MakeSpatialValue(0x12341234);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.IsNull(result);
        }

        [Test]
        public void GivenMetersWithClearWhenGetMetersValueThenIsCorrect()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiClear.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x20524C43, result);
        }

        [Test]
        public void GivenMetersWithSunnyWhenGetMetersValueThenIsCorrect()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x2043534E, result);
        }

        [Test]
        public void GivenMetersWithPartlyCloudyWhenGetMetersValueThenIsCorrect()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiPartlyCloudy.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x20544353, result);
        }

        [Test]
        public void GivenMetersWithCloudyWhenGetMetersValueThenIsCorrect()
        {
            var enumeratedMeter = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiCloudy.ToModelEnumMember() });

            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x2043564F, result);
        }

        private static SpatialValue MakeSpatialValue(int value)
        {
            return new SpatialValue
            {
                Value = value,
                Dlv = new DLV
                {
                    A = "D2"
                },
            };
        }

        private EnumeratedMeter CreateMeter()
        {
            return _creator.CreateMeters(null).Single();
        }
    }
}
