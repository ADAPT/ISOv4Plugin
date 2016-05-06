using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
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
        private int _loggedDataId;

        [SetUp]
        public void Setup()
        {
            _datacardPath = "dataCardPath";
            _tlg = new TLG();
            _tlgs = new List<TLG>{ _tlg };

            _spatialRecordMapperMock = new Mock<ISpatialRecordMapper>();
            _xmlReaderMock = new Mock<IXmlReader>();
            _binaryReaderMock = new Mock<IBinaryReader>();
            _sectionMapperMock = new Mock<ISectionMapper>();
            _uniqueIdMapperMock = new Mock<IUniqueIdMapper>();

            _operationDataMapper = new OperationDataMapper(_xmlReaderMock.Object, _binaryReaderMock.Object, _spatialRecordMapperMock.Object, _sectionMapperMock.Object, _uniqueIdMapperMock.Object);
        }

        [Test]
        public void GivenTlgsWhenMapThenListOfOperationData()
        {
            _tlgs.Add(new TLG());
            _tlgs.Add(new TLG());
            _tlgs.Add(new TLG());

            var result = Map();

            Assert.AreEqual(_tlgs.Count, result.Count());
        }

        [Test]
        public void GivenTskWhenMapThenXmlFileIsRead()
        {
            _tlg.A = "fileName";

            Map();

            _xmlReaderMock.Verify(x => x.ReadTlgXmlData(_datacardPath, _tlg.A + ".xml"), Times.Once());
        }
        
        [Test]
        public void GivenTskWhenMapThenBinaryFileIsRead()
        {
            _tlg.A = "fileName";

            var timHeader = new TIMHeader();
            _xmlReaderMock.Setup(x => x.ReadTlgXmlData(_datacardPath, _tlg.A + ".xml")).Returns(timHeader);

            Map();

            _binaryReaderMock.Verify(x => x.Read(_datacardPath, _tlg.A + ".bin", timHeader), Times.Once());
        }

        [Test]
        public void GivenTlgWhenMapThenSpatialRecordsAreMapped()
        {
            _tlg.A = "fileName";

            var timHeader = new TIMHeader();
            _xmlReaderMock.Setup(x => x.ReadTlgXmlData(_datacardPath, _tlg.A + ".xml")).Returns(timHeader);

            var isoSpatialRows = new List<ISOSpatialRow>();
            _binaryReaderMock.Setup(x => x.Read(_datacardPath, _tlg.A + ".bin", timHeader)).Returns(isoSpatialRows);

            var spatialRecords = new List<SpatialRecord>();
            _spatialRecordMapperMock.Setup(x => x.Map(isoSpatialRows, new List<Meter>())).Returns(spatialRecords);

            var result = MapSingle();

            Assert.AreEqual(spatialRecords, result.GetSpatialRecords());
        }

        [Test]
        public void GivenTlgWhenMapThenGetSectionsIsMapped()
        {
            _tlg.A = "fileName";

            var timHeader = new TIMHeader();
            _xmlReaderMock.Setup(x => x.ReadTlgXmlData(_datacardPath, _tlg.A + ".xml")).Returns(timHeader);

            var isoSpatialRows = new List<ISOSpatialRow>();
            _binaryReaderMock.Setup(x => x.Read(_datacardPath, _tlg.A + ".bin", timHeader)).Returns(isoSpatialRows);

            var sections = new List<Section>();
            _sectionMapperMock.Setup(x => x.Map(timHeader, isoSpatialRows)).Returns(sections);

            var result = MapSingle();

            Assert.AreSame(sections, result.GetSections(0));
        }

        [Test]
        public void GivenTlgWhenMapThenIdIsMapped()
        {
            _tlg.A = "fileName";

            var uniqueId = new UniqueId();
            _uniqueIdMapperMock.Setup(x => x.Map(_tlg.A)).Returns(uniqueId);

            var result = MapSingle();

            Assert.Contains(uniqueId, result.Id.UniqueIds);
        }

        [Test]
        public void GivenTlgAndLoggedDataIdWhenMapThenLoggedDataIdIsMapped()
        {
            _loggedDataId = 123;

            var result = MapSingle();

            Assert.AreEqual(_loggedDataId, result.LoggedDataId);
        }

        public OperationData MapSingle()
        {
            return Map().First();
        }

        public List<OperationData> Map()
        {
            return _operationDataMapper.Map(_tlgs, _datacardPath, _loggedDataId).ToList();
        }
    }
}
