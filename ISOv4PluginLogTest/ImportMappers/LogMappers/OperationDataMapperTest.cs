using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class OperationDataMapperTest
    {
        private string _datacardPath;
        private TLG _tlg;
        private List<TLG> _tlgs;
        private Mock<IXmlReader> _xmlReaderMock;
        private Mock<ISpatialRecordMapper> _spatialRecordMapperMock; 
        private Mock<IBinaryReader> _binaryReaderMock;
        private OperationDataMapper _operationDataMapper;
        private Mock<ISectionMapper> _sectionMapperMock;
        private Mock<IUniqueIdMapper> _uniqueIdMapperMock;
        private TIM _tim;
        private List<TIM> _tims;
        private List<ISOSpatialRow> _isoSpatialRows;
        private List<DeviceElementUse> _sections;
        private Dictionary<string, List<UniqueId>> _linkedIds;

        [SetUp]
        public void Setup()
        {
            _datacardPath = "dataCardPath";
            _tlg = new TLG();
            _tlgs = new List<TLG>{ _tlg };
            _tim = new TIM();
            _tims = new List<TIM> {_tim};
            _linkedIds = new Dictionary<string, List<UniqueId>>();

            _spatialRecordMapperMock = new Mock<ISpatialRecordMapper>();
            _xmlReaderMock = new Mock<IXmlReader>();
            _binaryReaderMock = new Mock<IBinaryReader>();
            _sectionMapperMock = new Mock<ISectionMapper>();
            _uniqueIdMapperMock = new Mock<IUniqueIdMapper>();

            _tlg.A = "fileName";
            _xmlReaderMock.Setup(x => x.ReadTlgXmlData(_datacardPath, _tlg.A + ".xml")).Returns(_tims);

            _isoSpatialRows = new List<ISOSpatialRow>();
            _binaryReaderMock.Setup(x => x.Read(_datacardPath, _tlg.A + ".bin", _tim)).Returns(_isoSpatialRows);

            _sections = new List<DeviceElementUse>();
            _sectionMapperMock.Setup(x => x.Map(_tims, _isoSpatialRows)).Returns(_sections);
            _sectionMapperMock.Setup(x => x.ConvertToBaseTypes(_sections)).Returns(_sections);

            _operationDataMapper = new OperationDataMapper(_xmlReaderMock.Object, _binaryReaderMock.Object, _spatialRecordMapperMock.Object, _sectionMapperMock.Object, _uniqueIdMapperMock.Object);
        }

        [Test]
        public void GivenTlgsWhenMapThenListOfOperationData()
        {
            _tlg.A = "fileName";
            _tlgs.Add(_tlg);
            _tlgs.Add(_tlg);
            _tlgs.Add(_tlg);

            var result = Map();

            Assert.AreEqual(_tlgs.Count, result.Count());
        }

        [Test]
        public void GivenTskWhenMapThenXmlFileIsRead()
        {
            Map();
            _xmlReaderMock.Verify(x => x.ReadTlgXmlData(_datacardPath, _tlg.A + ".xml"), Times.Once());
        }
        
        [Test]
        public void GivenTskWhenMapThenBinaryFileIsRead()
        {
            Map();
            _binaryReaderMock.Verify(x => x.Read(_datacardPath, _tlg.A + ".bin", _tim), Times.Once());
        }

        [Test]
        public void GivenTlgWhenMapThenSpatialRecordsAreMapped()
        {
            var spatialRecords = new List<SpatialRecord>();
            _spatialRecordMapperMock.Setup(x => x.Map(_isoSpatialRows, new List<WorkingData>())).Returns(spatialRecords);

            var result = MapSingle();

            Assert.AreEqual(spatialRecords, result.GetSpatialRecords());
        }

        [Test]
        public void GivenTlgWhenMapThenGetSectionsIsMapped()
        {
            var result = MapSingle();
            Assert.AreSame(_sections, result.GetDeviceElementUses(0));
        }

        [Test]
        public void GivenTlgWhenMapThenIdIsMapped()
        {
            var uniqueId = new UniqueId();
            _uniqueIdMapperMock.Setup(x => x.Map(_tlg.A)).Returns(uniqueId);

            var result = MapSingle();

            Assert.Contains(uniqueId, result.Id.UniqueIds);
        }

        [Test]
        public void GivenTlgAndPrescriptionIdWhenMapThenPrescriptionIdIsSet()
        {
            var result = _operationDataMapper.Map(_tlgs, 5, _datacardPath, 0, _linkedIds).ToList().First();

            Assert.AreEqual(5, result.PrescriptionId);
        }

        [Test]
        public void GivenTlgAndNullPrescriptionIdWhenMapThenPrescriptionIdIsNull()
        {
            var result = MapSingle();
            Assert.IsNull(result.PrescriptionId);
        }

        [Test]
        public void GivenTlgWithIdsInLinkListThenIdsAreAdded()
        {
            var uniqueId = new UniqueId
            {
                IdType = IdTypeEnum.UUID,
                Source = "source1",
                Id = Guid.NewGuid().ToString(),
                SourceType = IdSourceTypeEnum.GLN
            };
            _linkedIds.Add(_tlg.A, new List<UniqueId>{ uniqueId });

            var result = MapSingle();

            Assert.Contains(uniqueId, result.Id.UniqueIds);

        }

        public OperationData MapSingle()
        {
            return Map().First();
        }

        public List<OperationData> Map()
        {
            return _operationDataMapper.Map(_tlgs, null, _datacardPath, 0, _linkedIds).ToList();
        }
    }
}
