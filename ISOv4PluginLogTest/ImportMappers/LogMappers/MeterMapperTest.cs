using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.UnitSystem;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class MeterMapperTest
    {
        private MeterMapper _meterMapper;
        private TIMHeader _timHeader;
        private Mock<IRepresentationMapper> _representationMapperMock;
        private Mock<IEnumeratedMeterFactory> _enumeratorMeterFactoryMock;
        private Mock<IUniqueIdMapper> _uniqueIdMapperMock;
        private List<ISOSpatialRow> _isoSpatialRows;

        [SetUp]
        public void Setup()
        {
            _timHeader = new TIMHeader();
            _isoSpatialRows = new List<ISOSpatialRow>();
            _representationMapperMock = new Mock<IRepresentationMapper>();
            _enumeratorMeterFactoryMock = new Mock<IEnumeratedMeterFactory>();
            _uniqueIdMapperMock = new Mock<IUniqueIdMapper>();
            _meterMapper = new MeterMapper(_representationMapperMock.Object, _enumeratorMeterFactoryMock.Object, _uniqueIdMapperMock.Object);
        }

        [Test]
        public void GivenTimHeaderWithDlvsWhenMapThenEachDlvIsMeter()
        {
            _timHeader.DLVs = new List<DLVHeader>
            {
                new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 1 } },
                new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 1 } },
                new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 1 } }
            };

            var result = Map();

            Assert.AreEqual(_timHeader.DLVs.Count, result.Count);
        }

        [Test]
        public void GivenTimHeaderWithDlvWhenMapThenUnitOfMeasureIsMapped()
        {
            var dlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 1 } };
            _timHeader.DLVs = new List<DLVHeader> { dlvHeader };

            _representationMapperMock.Setup(r => r.GetUnitForDdi(1)).Returns(UnitSystemManager.GetUnitOfMeasure("m"));

            var result = (NumericMeter)MapSingle();

            Assert.AreEqual("m", result.UnitOfMeasure.Code);
        }

        [Test]
        public void GivenTimeHeaderAndSectionIdWhenMapThenSectionIdSetOnMeter()
        {
            var dlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 1 } };
            _timHeader.DLVs = new List<DLVHeader>{dlvHeader};

            var result = MapSingle(-45);

            Assert.AreEqual(-45, result.SectionId);
        }

        [Test]
        public void GivenDlvWithDdiWhenMapThenCallsRepresentationMapper()
        {
            var dlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 7 }, };
            _timHeader.DLVs = new List<DLVHeader>{dlvHeader};

            MapSingle();

            _representationMapperMock.Verify(x => x.Map(7));
        }

        [Test]
        public void GivenDlvWhenMapThenStoresOrderInMeterCompoundId()
        {
            var dlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 1 }, };
            var secondHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 7 }, };
            _timHeader.DLVs = new List<DLVHeader>{dlvHeader, secondHeader};

            var uniqueId1 = new UniqueId();
            _uniqueIdMapperMock.Setup(x => x.Map("DLV0")).Returns(uniqueId1);

            var uniqueId2 = new UniqueId();
            _uniqueIdMapperMock.Setup(x => x.Map("DLV1")).Returns(uniqueId2);

            var result = Map();

            Assert.Contains(uniqueId1, result[0].Id.UniqueIds);
            Assert.Contains(uniqueId2, result[1].Id.UniqueIds);
        }

        [Test]
        public void GivenEnumeratedDlvWhenMapThenEnumeratatedMeterCreatorReturnsFactory()
        {
            const int ddivalue = 98282;
            var dlvHeader = new DLVHeader {ProcessDataDDI = new HeaderProperty{ Value =  ddivalue}};
            _timHeader.DLVs = new List<DLVHeader>{dlvHeader};
            Map();

            _enumeratorMeterFactoryMock.Verify(x => x.GetMeterCreator(ddivalue), Times.Once);
        }

        [Test]
        public void GivenEnumeratedDlvWhenMapThenEnumerateMeterIsMapped()
        {
            const int ddiValue = 131;
            var dlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty {Value = ddiValue} };
            _timHeader.DLVs = new List<DLVHeader> { dlvHeader };

            var expected = new ISOEnumeratedMeter { TriggerId = 8675309 };
            var meterCreator = new Mock<IEnumeratedMeterCreator>();
            meterCreator.Setup(m => m.CreateMeters(It.IsAny<IEnumerable<ISOSpatialRow>>())).Returns(new List<ISOEnumeratedMeter> {expected});

            _enumeratorMeterFactoryMock.Setup(m => m.GetMeterCreator(ddiValue)).Returns(meterCreator.Object);

            var result = MapSingle();

            Assert.AreSame(expected, result);
        }

        [Test]
        public void GivenEnumeratedDlvWhenMapThenEnumerateMeterHasId()
        {
            const int ddiValue = 131;
            var dlvHeader = new DLVHeader { ProcessDataDDI = new HeaderProperty {Value = ddiValue} };
            _timHeader.DLVs = new List<DLVHeader> { dlvHeader };

            var expected = new ISOEnumeratedMeter { TriggerId = 8675309 };
            var meterCreator = new Mock<IEnumeratedMeterCreator>();
            meterCreator.Setup(m => m.CreateMeters(It.IsAny<IEnumerable<ISOSpatialRow>>())).Returns(new List<ISOEnumeratedMeter> {expected});

            _enumeratorMeterFactoryMock.Setup(m => m.GetMeterCreator(ddiValue)).Returns(meterCreator.Object);
            var uniqueId = new UniqueId();
            _uniqueIdMapperMock.Setup(x => x.Map("DLV0")).Returns(uniqueId);

            var result = MapSingle();

            Assert.AreSame(expected, result);
            Assert.Contains(uniqueId, result.Id.UniqueIds);
        }

        private Meter MapSingle(int sectionId = 0)
        {
            return Map(sectionId).First();
        }

        private List<Meter> Map(int sectionId = 0)
        {
            return _meterMapper.Map(_timHeader, _isoSpatialRows, sectionId);
        }
    }
}
