using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class TlgMapperTest
    {

        private TlgMapper _tlgMapper;
        private List<OperationData> _operationDatas;
        private OperationData _operationData;
        private Mock<IBinaryWriter> _binaryWriterMock;
        private Mock<IXmlReader> _xmlReaderMock;  
        private Mock<ITimHeaderMapper> _timHeaderMock;
        private string _datacardPath;

        [SetUp]
        public void Setup()
        {
            _operationData = new OperationData();
            _operationDatas = new List<OperationData> { _operationData };

            _binaryWriterMock = new Mock<IBinaryWriter>();
            _xmlReaderMock = new Mock<IXmlReader>();
            _timHeaderMock = new Mock<ITimHeaderMapper>();
            _datacardPath = "";

            _tlgMapper = new TlgMapper(_xmlReaderMock.Object, _timHeaderMock.Object, _binaryWriterMock.Object);
        }

        [Test]
        public void GivenOperationDatasWhenMapThenTlgsAreMapped()
        {
            _operationDatas.Add(new OperationData());
            _operationDatas.Add(new OperationData());
            _operationDatas.Add(new OperationData());

            var result = Map();

            Assert.AreEqual(_operationDatas.Count, result.Count());
        }

        [Test]
        public void GivenOperationDatasWhenMapThenAIsMapped()
        {
            _operationData.Id.UniqueIds.Add(new UniqueId
            {
                Id = "TLG00016",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Source = UniqueIdMapper.IsoSource
            });

            var result = MapSingle();
            Assert.AreEqual("TLG00016", result.A);
        }

        [Test]
        public void GivenOperationDataWhenMapThenBinaryWriterIsCalled()
        {
            _operationData.Id.UniqueIds.Add(new UniqueId
            {
                Id = "TLG00016",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Source = UniqueIdMapper.IsoSource
            });

            var meters = new List<Meter>{ new NumericMeter() };
            var sections = new List<Section>
            {
                new Section
                {
                    GetMeters = () => meters
                }
            };
            var sectionsByDepth = new Dictionary<int, IEnumerable<Section>>
            {
                { 0, sections }
            };
            _operationData.GetSections = x => sectionsByDepth[x];
            _operationData.MaxDepth = 0;

            var spatialRecords = new List<SpatialRecord>{ new SpatialRecord() };
            _operationData.GetSpatialRecords = () => spatialRecords;

            MapSingle();

            var expectedPath = Path.Combine(_datacardPath, "TLG00016.bin");
            _binaryWriterMock.Verify(x => x.Write(expectedPath, meters, spatialRecords), Times.Once);
        }

        [Test]
        public void GivenOperationDataWhenMapThenTimHeaderIsMapped()
        {
            _operationData.Id.UniqueIds.Add(new UniqueId
            {
                Id = "TLG00016",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
            });

            var meters = new List<Meter> { new NumericMeter() };
            var sections = new List<Section>
            {
                new Section
                {
                    GetMeters = () => meters
                }
            };
            var sectionsByDepth = new Dictionary<int, IEnumerable<Section>>
            {
                { 0, sections }
            };
            _operationData.GetSections = x => sectionsByDepth[x];
            _operationData.MaxDepth = 0;

            MapSingle();

            _timHeaderMock.Verify(x => x.Map(meters), Times.Once);
        }

        [Test]
        public void GivenOperationDataWhenMapThenXmlReaderIsCalled()
        {
            _operationData.Id.UniqueIds.Add(new UniqueId
            {
                Id = "TLG00016",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Source = UniqueIdMapper.IsoSource
            });

            var meters = new List<Meter> { new NumericMeter() };
            var sections = new List<Section>
            {
                new Section
                {
                    GetMeters = () => meters
                }
            };
            var sectionsByDepth = new Dictionary<int, IEnumerable<Section>>
            {
                { 0, sections }
            };
            _operationData.GetSections = x => sectionsByDepth[x];
            _operationData.MaxDepth = 0;

            var timHeader = new TIMHeader();
            _timHeaderMock.Setup(x => x.Map(meters)).Returns(timHeader);

            MapSingle();

            _xmlReaderMock.Verify(x => x.WriteTlgXmlData(_datacardPath, "TLG00016.xml",  timHeader));
        }

        private TLG MapSingle()
        {
            return Map().First();
        }

        private IEnumerable<TLG> Map()
        {
            return _tlgMapper.Map(_operationDatas, _datacardPath);
        }
    }
}
