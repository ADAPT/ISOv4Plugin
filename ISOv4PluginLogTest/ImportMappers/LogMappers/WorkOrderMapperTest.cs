using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class WorkOrderMapperTest
    {
        private TSK _task;
        private List<TSK> _taskList;
        private string _dataPath;
        private ApplicationDataModel _dataModel;
        private Dictionary<string, List<UniqueId>> _linkedIds;
        private WorkOrderMapper _mapper;
        private Mock<IStatusUpdateMapper> _statusUpdateMapperMock;

        [SetUp]
        public void Setup()
        {
            _task = new TSK();
            _taskList = new List<TSK> {_task};
            _dataPath = Path.GetTempPath();
            _dataModel = new ApplicationDataModel {Catalog = new Catalog(), Documents = new Documents()};
            _linkedIds = new Dictionary<string, List<UniqueId>>();

            _statusUpdateMapperMock = new Mock<IStatusUpdateMapper>();

            _mapper = new WorkOrderMapper(_statusUpdateMapperMock.Object);
        }

        [Test]
        public void GivenNullTaskWhenMappedThenNoWorkOrderIsMapped()
        {
            _taskList = new List<TSK>{null};

            var results = Map();
            Assert.IsEmpty(results);
        }

        [Test]
        public void GivenTaskWhenMapThenIdIsMapped()
        {
            _task.A = "TSK1";

            var result = MapSingle();
            Assert.AreEqual(_task.A, result.Id.UniqueIds.Single().Id);
        }

        [Test]
        public void GivenTaskWhenMapThenDescriptionSet()
        {
            _task.B = "Task Name";

            var result = MapSingle();
            Assert.AreEqual(_task.B, result.Description);
        }

        [Test]
        public void GivenTaskWhenMapThenGrowerIdIsMapped()
        {
            _task.C = "CTR3";
            var grower = new Grower();
            grower.Id.UniqueIds.Add(new UniqueId
            {
                Source = UniqueIdMapper.IsoSource,
                IdType = IdTypeEnum.String,
                Id = _task.C,
            });
            _dataModel.Catalog.Growers.Add(grower);

            var result = MapSingle();
            Assert.AreEqual(grower.Id.ReferenceId, result.GrowerId);
        }

        [Test]
        public void GivenTaskWhenMapThenFarmIdIsMapped()
        {
            _task.D = "FRM4";
            var farm = new Farm();
            farm.Id.UniqueIds.Add(new UniqueId
            {
                Source = UniqueIdMapper.IsoSource,
                IdType = IdTypeEnum.String,
                Id = _task.D,
            });
            _dataModel.Catalog.Farms.Add(farm);

            var result = MapSingle();
            Assert.AreEqual(farm.Id.ReferenceId, result.FarmIds.First());
        }

        [Test]
        public void GivenTaskWhenMapThenFieldIdIsMapped()
        {
            _task.E = "PFD1";
            var field = new Field();
            field.Id.UniqueIds.Add(new UniqueId
            {
                Source = UniqueIdMapper.IsoSource,
                IdType = IdTypeEnum.String,
                Id = _task.E,
            });
            _dataModel.Catalog.Fields.Add(field);

            var result = MapSingle();
            Assert.AreEqual(field.Id.ReferenceId, result.FieldIds.Single());
        }

        [Test]
        public void GivenTaskWhenMapThenPersonRoleIsMapped()
        {
            _task.F = "WRK4";

            var person = new Person();
            person.Id.UniqueIds.Add(new UniqueId
            {
                Source = UniqueIdMapper.IsoSource,
                IdType = IdTypeEnum.String,
                Id = _task.F,
            });
            _dataModel.Catalog.Persons.Add(person);

            var result = MapSingle();
            var personRole = _dataModel.Catalog.PersonRoles.Single(r => r.Id.ReferenceId == result.PersonRoleIds.Single());
            Assert.AreEqual(personRole.PersonId, person.Id.ReferenceId);
        }

        [Test]
        public void GivenTaskWhenMapThenStatusUpdateIsMapped()
        {
            _task.G = TSKG.Item2;

            var statusUpdate = new StatusUpdate();
            _statusUpdateMapperMock.Setup(x => x.Map(_task.G)).Returns(statusUpdate);
            var statusUpdates = new List<StatusUpdate>();
            statusUpdates.Add(statusUpdate);

            var result = MapSingle();
            Assert.AreSame(statusUpdates[0], result.StatusUpdates[0]);
        }

        private WorkOrder MapSingle()
        {
            return Map().FirstOrDefault();
        }

        private List<WorkOrder> Map()
        {
            return _mapper.Map(_taskList, _dataModel);
        }
    }
}
