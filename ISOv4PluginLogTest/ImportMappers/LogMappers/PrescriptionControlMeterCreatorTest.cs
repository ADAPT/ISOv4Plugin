using System.Collections.Generic;
using System.Linq;
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
    public class PrescriptionControlMeterCreatorTest
    {

        private PrescriptionControlMeterCreator _creator;

        [SetUp]
        public void Setup()
        {
            _creator = new PrescriptionControlMeterCreator(158);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenReturnsSingleMeter()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GivenSpatialRowsWhenCreateThenRepIsCorrect()
        {
            var result = _creator.CreateMeters(null);

            Assert.AreEqual(RepresentationInstanceList.dtPrescriptionControlMasterState.ToModelRepresentation().Code, result[0].Representation.Code);
        }

        [Test]
        public void GivenValueZeroWhenGetValueThenReturnsOff()
        {
            var value = MakeSpatialValue(0);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrscMasterManualOff.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueOneWhenGetValueThenReturnsOn()
        {
            var value = MakeSpatialValue(1);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrscMasterAutoOn.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueTwoWhenGetValueThenReturnsError()
        {
            var value = MakeSpatialValue(2);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrscMasterError.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenValueThreeWhenGetValueThenReturnsUndefinede()
        {
            var value = MakeSpatialValue(3);
            var result = _creator.GetValueForMeter(value, CreateMeter());

            Assert.AreEqual(DefinedTypeEnumerationInstanceList.dtiPrscMasterUndefined.ToModelEnumMember().Code, result.Code);
        }

        [Test]
        public void GivenMetersWithManualOffWhenGetMetersValueThenZero()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiPrscMasterManualOff.ToModelEnumMember() });
            
            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x00, result);
        }

        [Test]
        public void GivenMetersWithAutoOnWhenGetMetersValueThenOne()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiPrscMasterAutoOn.ToModelEnumMember() });
            
            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x01, result);
        }

        [Test]
        public void GivenMetersWithErrorWhenGetMetersValueThenTwo()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiPrscMasterError.ToModelEnumMember() });
            
            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x02, result);
        }

        [Test]
        public void GivenMetersWithUndefinedWhenGetMetersValueThenThree()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiPrscMasterUndefined.ToModelEnumMember() });
            
            var result = _creator.GetMetersValue(meters, spatialRecord);
            Assert.AreEqual(0x03, result);
        }

        [Test]
        public void GivenMetersWithUnknownWhenGetMetersValueThenThree()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter();
            var meters = new List<Meter> { enumeratedMeter1 };

            var spatialRecord = new SpatialRecord();
            spatialRecord.SetMeterValue(enumeratedMeter1, new EnumeratedValue { Value = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember() });
            
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
                    ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 158 }
                }
            };
        }

        private EnumeratedMeter CreateMeter()
        {
            return _creator.CreateMeters(null).Single();
        }





    }

  
}
