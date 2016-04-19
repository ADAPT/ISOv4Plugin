using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class LoggedDataMapperTest
    {
        private TSK _tsk;
        private LoggedDataMapper _loggedDataMapper;
        private Mock<IOperationDataMapper> _operationDataMapper;
        private Mock<ITimeScopeMapper> _timeScopeMapperMock;
        private string _dataPath;
        private Catalog _catalog;
        private Mock<IUniqueIdMapper> _uniqueIdMapperMock;

        [SetUp]
        public void Setup()
        {
            _tsk = new TSK();
            _dataPath = "";
            _catalog = new Catalog
            {
                Growers = new List<Grower>(),
                Fields = new List<Field>(),
                Farms = new List<Farm>(),
            };

            _operationDataMapper = new Mock<IOperationDataMapper>();
            _uniqueIdMapperMock = new Mock<IUniqueIdMapper>();
            _timeScopeMapperMock = new Mock<ITimeScopeMapper>();

            _loggedDataMapper = new LoggedDataMapper(_operationDataMapper.Object, _uniqueIdMapperMock.Object, _timeScopeMapperMock.Object);
        }

        [Test]
        public void GivenTskWhenMapThenLoggedData()
        {
            var result = Map();

            Assert.IsNotNull(result);
        }

        [Test]
        public void GivenNullTskTWhenMapThenNull()
        {
            var result = _loggedDataMapper.Map(null, _dataPath, _catalog);

            Assert.IsNull(result);
        }

        [Test]
        public void GivenTskWhenMapThenOperationDataAreMapped()
        {
            var tlgs = new List<TLG> {new TLG()};
            _tsk.Items = tlgs.ToArray();

            var operationDatas = new List<OperationData>();
            _operationDataMapper.Setup(x => x.Map(tlgs, _dataPath)).Returns(operationDatas);

            var result = Map();
            Assert.AreSame(operationDatas, result.OperationData);
        }

        [Test]
        public void GivenTskWithCustomerIdWhenMapThenSetsGrowerId()
        {
            var grower = new Grower();
            grower.Id.UniqueIds.Add(new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = "CTR1",
                Source = UniqueIdMapper.IsoSource
            });
            _catalog.Growers.Add(grower);
            _tsk.C = "CTR1";

            var result = Map();
            Assert.AreEqual(grower.Id.ReferenceId, result.GrowerId);
        }

        [Test]
        public void GivenTskWithFramIdWhenMapThenSetsFarmId()
        {
            var farm = new Farm();
            farm.Id.UniqueIds.Add(new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = "FRM1",
                Source = UniqueIdMapper.IsoSource
            });
            _catalog.Farms.Add(farm);
            _tsk.D = "FRM1";

            var result = Map();
            Assert.AreEqual(farm.Id.ReferenceId, result.FarmId);
        }

        [Test]
        public void GivenTskWithFieldIdWhenMapThenSetsFieldId()
        {
            var field = new Field();
            field.Id.UniqueIds.Add(new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = "PFD1",
                Source = UniqueIdMapper.IsoSource
            });
            _catalog.Fields.Add(field);
            _tsk.E = "PFD1";

            var result = Map();
            Assert.AreEqual(field.Id.ReferenceId, result.FieldId);
        }

        [Test]
        public void GivenTskWithTimeWhenMapThenSetsTimeScopeId()
        {
            var tim = new TIM {A = DateTime.Now, B = DateTime.Now.AddHours(5)};
            var items = new List<object> { tim };
            _tsk.Items = items.ToArray();

            var timeScopes = new List<TimeScope>{ new TimeScope(), new TimeScope()};
            _timeScopeMapperMock.Setup(x => x.Map(new List<TIM> {tim}, _catalog)).Returns(timeScopes);

            var result = Map();

            Assert.Contains(timeScopes[0].Id.ReferenceId, result.TimeScopeIds);
            Assert.Contains(timeScopes[1].Id.ReferenceId, result.TimeScopeIds);
        }

        [Test]
        public void GivenTskWhenMapThenIdIsMapped()
        {
            _tsk.A = "TSK1";

            var uniqueId = new UniqueId();
            _uniqueIdMapperMock.Setup(x => x.Map(_tsk.A)).Returns(uniqueId);

            var result = Map();

            Assert.Contains(uniqueId, result.Id.UniqueIds);
        }

        public LoggedData Map()
        {
            return _loggedDataMapper.Map(_tsk, _dataPath, _catalog);
        }

    }
}
