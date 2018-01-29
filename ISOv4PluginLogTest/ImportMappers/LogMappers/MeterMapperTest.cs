using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
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
        private TIM _tim;
        private Mock<IRepresentationMapper> _representationMapperMock;
        private Mock<IEnumeratedMeterFactory> _enumeratorMeterFactoryMock;
        private Mock<IUniqueIdMapper> _uniqueIdMapperMock;
        private List<ISOSpatialRow> _isoSpatialRows;

        [SetUp]
        public void Setup()
        {
            _tim = new TIM();
            _isoSpatialRows = new List<ISOSpatialRow>();
            _representationMapperMock = new Mock<IRepresentationMapper>();
            _enumeratorMeterFactoryMock = new Mock<IEnumeratedMeterFactory>();
            _uniqueIdMapperMock = new Mock<IUniqueIdMapper>();
            _meterMapper = new MeterMapper(_representationMapperMock.Object, _enumeratorMeterFactoryMock.Object, _uniqueIdMapperMock.Object);
        }

        [Test]
        public void GivenTimHeaderWithDlvsWhenMapThenEachDlvIsMeter()
        {
            _tim.Items = new List<DLV>
            {
                new DLV { A = "1" },
                new DLV { A = "1" },
                new DLV { A = "1" },
            }.ToArray();

            var result = Map();

            Assert.AreEqual(_tim.Items.Length, result.Count);
        }

        [Test]
        public void GivenTimHeaderWithDlvWhenMapThenUnitOfMeasureIsMapped()
        {
            var dlv = new DLV {A = "1"};
            _tim.Items = new List<DLV> { dlv }.ToArray();

            _representationMapperMock.Setup(r => r.GetUnitForDdi(1)).Returns(UnitSystemManager.GetUnitOfMeasure("m"));

            var result = (NumericWorkingData)MapSingle();

            Assert.AreEqual("m", result.UnitOfMeasure.Code);
        }

        [Test]
        public void GivenTimeHeaderAndSectionIdWhenMapThenSectionIdSetOnMeter()
        {
            var dlv = new DLV { A = "1"};
            _tim.Items = new List<DLV> {dlv}.ToArray();

            var result = MapSingle(-45);

            Assert.AreEqual(-45, result.DeviceElementUseId);
        }

        [Test]
        public void GivenDlvWithDdiWhenMapThenCallsRepresentationMapper()
        {
            var dlv = new DLV { A = "7" };
            _tim.Items = new List<DLV> { dlv }.ToArray();

            MapSingle();

            _representationMapperMock.Verify(x => x.Map(7));
        }

        [Test]
        public void GivenDlvWhenMapThenStoresOrderInMeterCompoundId()
        {
            var dlv1 = new DLV { A = "1" };
            var dlv2 = new DLV { A = "7" };
            _tim.Items = new List<DLV> { dlv1, dlv2 }.ToArray();

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
            var dlv = new DLV { A = ddivalue.ToString("x4") };
            _tim.Items = new List<DLV> { dlv }.ToArray();
            Map();

            _enumeratorMeterFactoryMock.Verify(x => x.GetMeterCreator(ddivalue), Times.Once);
        }

        [Test]
        public void GivenEnumeratedDlvWhenMapThenEnumerateMeterIsMapped()
        {
            const int ddiValue = 131;
            var dlv = new DLV { A = ddiValue.ToString("x4") };
            _tim.Items = new List<DLV> { dlv }.ToArray();

            var expected = new ISOEnumeratedMeter { DeviceElementUseId = 8675309 };
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
            var dlv = new DLV { A = ddiValue.ToString("x4") };
            _tim.Items = new List<DLV> { dlv }.ToArray();

            var expected = new ISOEnumeratedMeter { DeviceElementUseId = 8675309 };
            var meterCreator = new Mock<IEnumeratedMeterCreator>();
            meterCreator.Setup(m => m.CreateMeters(It.IsAny<IEnumerable<ISOSpatialRow>>())).Returns(new List<ISOEnumeratedMeter> {expected});

            _enumeratorMeterFactoryMock.Setup(m => m.GetMeterCreator(ddiValue)).Returns(meterCreator.Object);
            var uniqueId = new UniqueId();
            _uniqueIdMapperMock.Setup(x => x.Map("DLV0")).Returns(uniqueId);

            var result = MapSingle();

            Assert.AreSame(expected, result);
            Assert.Contains(uniqueId, result.Id.UniqueIds);
        }

        private WorkingData MapSingle(int sectionId = 0)
        {
            return Map(sectionId).First();
        }

        private List<WorkingData> Map(int sectionId = 0)
        {
            return _meterMapper.Map(_tim, _isoSpatialRows, sectionId);
        }
    }
}
