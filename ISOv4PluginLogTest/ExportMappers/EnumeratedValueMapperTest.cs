using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class EnumeratedValueMapperTest
    {
        private Mock<IEnumeratedMeterFactory> _enumeratedMeterFactoryMock;
        private Mock<IRepresentationMapper> _representationMapperMock;
        private EnumeratedValueMapper _enumeratedValueMapper;

        [SetUp]
        public void Setup()
        {
            _enumeratedMeterFactoryMock = new Mock<IEnumeratedMeterFactory>();
            _representationMapperMock = new Mock<IRepresentationMapper>();

            _enumeratedValueMapper = new EnumeratedValueMapper(_enumeratedMeterFactoryMock.Object, _representationMapperMock.Object);
        }

        [Test]
        public void GivenCurrentMeterWithMatchingMetersWhenMapThenValueReturned()
        {
            var enumeratedMeter1 = new ISOEnumeratedMeter{ Representation = RepresentationInstanceList.dtRecordingStatus.ToModelRepresentation() };
            var enumeratedMeter2 = new ISOEnumeratedMeter();

            enumeratedMeter1.Id.UniqueIds.Add(new UniqueId{ Id = "DLV3", Source = UniqueIdMapper.IsoSource, CiTypeEnum = CompoundIdentifierTypeEnum.String });
            enumeratedMeter2.Id.UniqueIds.Add(new UniqueId{ Id = "DLV3", Source = UniqueIdMapper.IsoSource, CiTypeEnum = CompoundIdentifierTypeEnum.String });

            var meters = new List<Meter> { enumeratedMeter1, enumeratedMeter2 };

            var spatialRecord = new SpatialRecord();

            const int ddi = 141;
            _representationMapperMock.Setup(x => x.Map(enumeratedMeter1.Representation)).Returns(ddi);

            var enumeratedMeterCreatorMock = new Mock<IEnumeratedMeterCreator>();
            _enumeratedMeterFactoryMock.Setup(x => x.GetMeterCreator(ddi)).Returns(enumeratedMeterCreatorMock.Object);

            const uint value = 0x123456;
            enumeratedMeterCreatorMock.Setup(x => x.GetMetersValue(meters, spatialRecord)).Returns(value);

            var result = _enumeratedValueMapper.Map(enumeratedMeter1, meters, spatialRecord);
            Assert.AreEqual(value, result);
        }
    }
}
