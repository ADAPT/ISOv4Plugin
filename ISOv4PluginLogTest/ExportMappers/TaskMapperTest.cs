using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class TaskMapperTest
    {
        private LoggedData _loggedData;
        private List<LoggedData> _loggedDatas;
        private Catalog _catalog;
        private string _datacardPath;
        private Mock<ITimeMapper> _timeMapperMock;
        private TaskMapper _taskMapper;
        private Mock<ITlgMapper> _tlgMapperMock;

        [SetUp]
        public void Setup()
        {
            _loggedData = new LoggedData();
            _loggedDatas = new List<LoggedData>{ _loggedData };
            _catalog = new Catalog();
            _datacardPath = "";

            _timeMapperMock = new Mock<ITimeMapper>();
            _tlgMapperMock = new Mock<ITlgMapper>();

            _taskMapper = new TaskMapper(_timeMapperMock.Object, _tlgMapperMock.Object);
        }

        [Test]
        public void GivenLoggedDatasWhenMapThenTsksAreCreated()
        {
            _loggedDatas.Add(new LoggedData());
            _loggedDatas.Add(new LoggedData());
            _loggedDatas.Add(new LoggedData());
            _loggedDatas.Add(new LoggedData());

            var result = Map();
            Assert.AreEqual(_loggedDatas.Count, result.Count());
        }

        [Test]
        public void GivenLoggedDataWithGrowerIdWhenMapThenTskCIsGrowerId()
        {
            var grower = new Grower();
            grower.Id.UniqueIds.Add(new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = "CTR2",
                Source = UniqueIdMapper.IsoSource,
            });
            _catalog.Growers = new List<Grower>{ grower };

            _loggedData.GrowerId = grower.Id.ReferenceId;

            var result = MapSingle();
            
            Assert.AreEqual("CTR2", result.C);
        }

        [Test]
        public void GIvenLoggedDataWithFarmIdWhenMapThenDisMapped()
        {
            var farm = new Farm();
            farm.Id.UniqueIds.Add(new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = "FRM2",
                Source = UniqueIdMapper.IsoSource,
            });
            _catalog.Farms = new List<Farm> { farm };

            _loggedData.FarmId = farm.Id.ReferenceId;

            var result = MapSingle();

            Assert.AreEqual("FRM2", result.D);
        }

        [Test]
        public void GivenLoggedDataWithFieldIdWhenMapThenEIsMapped()
        {
            var field = new Field();
            field.Id.UniqueIds.Add(new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = "PFD3",
                Source = UniqueIdMapper.IsoSource,
            });
            _catalog.Fields = new List<Field> {field};

            _loggedData.FieldId = field.Id.ReferenceId;

            var result = MapSingle();

            Assert.AreEqual("PFD3", result.E);
        }

        [Test]
        public void GivenLoggedDataWithTimescopesWhenMapThenTimItemsAreMapped()
        {
            var timescope = new TimeScope();
            _loggedData.TimeScopeIds = new List<int>{ timescope.Id.ReferenceId };
            _catalog.TimeScopes = new List<TimeScope>{ timescope };

            var tims = new List<TIM>{ new TIM(), new TIM()};
            _timeMapperMock.Setup(x => x.Map(new List<TimeScope> {timescope})).Returns(tims);

            var result = MapSingle();
            Assert.Contains(tims[0], result.Items);
            Assert.Contains(tims[1], result.Items);
        }

        [Test]
        public void GivenLoggedDataWithOperationDataWhenMapThenTlgItemsAreMapped()
        {
            _loggedData.OperationData = new List<OperationData>();

            var tlgs = new List<TLG>{ new TLG(), new TLG() };
            _tlgMapperMock.Setup(x => x.Map(_loggedData.OperationData, _datacardPath)).Returns(tlgs);

            var result = MapSingle();
            Assert.Contains(tlgs[0], result.Items);
            Assert.Contains(tlgs[1], result.Items);
        }

        [Test]
        public void GivenLoggedDataWhenMapThenAIsExistingTasksPlusOne()
        {
            var result = _taskMapper.Map(_loggedDatas, _catalog, _datacardPath, 0).First();
            Assert.AreEqual("TSK1", result.A);
        }

        [Test]
        public void GivenLoggedDataWhenMapThenBIsMapped()
        {
            _loggedData.Description = "Winston";
            var result = _taskMapper.Map(_loggedDatas, _catalog, _datacardPath, 0).First();
            Assert.AreEqual(_loggedData.Description, result.B);
        }


        [Test]
        public void GivenLoggedDataWhenMapThenGIsCompleted()
        {
            var result = MapSingle();
            Assert.AreEqual(TSKG.Item4, result.G);
        }

        [Test]
        public void GivenNullLoggedDataWhenMapThenIsEmpty()
        {
            var result = _taskMapper.Map(null, _catalog, _datacardPath, 0);
            Assert.IsEmpty(result);
        }

        private TSK MapSingle()
        {
            return Map().First();
        }

        private IEnumerable<TSK> Map()
        {
            return _taskMapper.Map(_loggedDatas, _catalog, _datacardPath, 0);
        }
    }
}
