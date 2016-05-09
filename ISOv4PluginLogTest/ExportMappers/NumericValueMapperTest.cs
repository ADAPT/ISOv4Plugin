using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class NumericValueMapperTest
    {
        private SpatialRecord _spatialRecord;
        private NumericMeter _meter;
        private Mock<IRepresentationMapper> _representationMapperMock; 
        private NumericValueMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _meter = new NumericMeter();
            _spatialRecord = new SpatialRecord();
            _representationMapperMock = new Mock<IRepresentationMapper>();

            _mapper = new NumericValueMapper(_representationMapperMock.Object);
        }

        [Test]
        public void GivenSpatialRecordWithNullMeterValueWhenMapThenIsZero()
        {
            var result = _mapper.Map(_meter, _spatialRecord);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GivenSpatialRecordWithResolutionOfOneWhenMapThenValueIsReturned()
        {
            var numericRepresentationValue = new NumericRepresentationValue(RepresentationInstanceList.vrLongitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), 85));
            _spatialRecord.SetMeterValue(_meter, numericRepresentationValue);

            _representationMapperMock.Setup(x => x.Map(numericRepresentationValue.Representation)).Returns(116);

            var result = _mapper.Map(_meter, _spatialRecord);
            Assert.AreEqual(numericRepresentationValue.Value.Value, result);
        }

        [Test]
        public void GivenSpatialRecordWhenMapThenValueIsReturned()
        {
            var numericRepresentationValue = new NumericRepresentationValue(RepresentationInstanceList.vrLongitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), 85));
            _spatialRecord.SetMeterValue(_meter, numericRepresentationValue);

            _representationMapperMock.Setup(x => x.Map(numericRepresentationValue.Representation)).Returns(1);

            var result = _mapper.Map(_meter, _spatialRecord);
            var expectedValue = (uint)numericRepresentationValue.Value.Value/.01;
            Assert.AreEqual(expectedValue, result);
        }

    }
}
