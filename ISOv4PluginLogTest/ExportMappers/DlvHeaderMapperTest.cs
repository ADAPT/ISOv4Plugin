using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class DlvHeaderMapperTest
    {
        private Meter _meter;
        private List<Meter> _meters; 
        private DlvHeaderMapper _dlvHeaderMapper;

        private Mock<IRepresentationMapper> _representationMapperMock;


        [SetUp]
        public void Setup()
        {
            _meter = new NumericMeter();
            _meters = new List<Meter>();

            _representationMapperMock = new Mock<IRepresentationMapper>();
            _dlvHeaderMapper = new DlvHeaderMapper(_representationMapperMock.Object);
        }

        [Test]
        public void GivenMetersWhenMapThenDlvForEachMeter()
        {
            _meters.Add(new NumericMeter());
            _meters.Add(new NumericMeter());
            _meters.Add(new NumericMeter());
            _meters.Add(new NumericMeter());

            var result = Map();
            Assert.AreEqual(_meters.Count, result.Count());
        }

        [Test]
        public void GivenMeterWhenMapThenProcessDataDdiIsMapped()
        {
            _meters = new List<Meter> { _meter };

            _representationMapperMock.Setup(x => x.Map(_meter.Representation)).Returns(5);

            var result = MapSingle();

            Assert.AreEqual(5, result.ProcessDataDDI.Value);
        }

        [Test]
        public void GivenMetersWhenMapThenMetersAreSorted()
        {
            _meter.Id.UniqueIds.Add(new UniqueId
            {
                Id = "DLV3",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Source = UniqueIdMapper.IsoSource
           });
            _meter.Representation = RepresentationInstanceList.vrAppRateVolumeControl.ToModelRepresentation();
            _representationMapperMock.Setup(x => x.Map(_meter.Representation)).Returns(5);
            _meters.Add(_meter);

            var meter1 = new NumericMeter();
            meter1.Id.UniqueIds.Add(new UniqueId
            {
                Id = "DLV1",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Source = UniqueIdMapper.IsoSource
            });
            meter1.Representation = RepresentationInstanceList.vrAppRateMassMetered.ToModelRepresentation();
            _representationMapperMock.Setup(x => x.Map(meter1.Representation)).Returns(8);
            _meters.Add(meter1);

            var result = Map().ToList();
            Assert.AreEqual(8, result[0].ProcessDataDDI.Value);
            Assert.AreEqual(5, result[1].ProcessDataDDI.Value);
        }

        [Test]
        public void GivenNullWhenMapThenReturnsNull()
        {
            _meters = null;

            var result = Map();

            Assert.IsNull(result);
        }

        private DLVHeader MapSingle()
        {
            return Map().First();
        }

        private IEnumerable<DLVHeader> Map()
        {
            return _dlvHeaderMapper.Map(_meters);
        }
    }
}
