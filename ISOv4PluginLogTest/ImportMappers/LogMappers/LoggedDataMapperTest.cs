using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
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
        private List<TSK> _tsks;
        private string _dataPath;
        private ApplicationDataModel _dataModel;
        private Catalog _catalog;
        private Documents _documents;
        private Mock<IOperationDataMapper> _operationDataMapper;
        private LoggedDataMapper _loggedDataMapper;

        [SetUp]
        public void Setup()
        {
            _tsk = new TSK();
            _tsks = new List<TSK>{ _tsk };
            _dataPath = Path.GetTempPath();
            _documents = new Documents();
            _catalog = new Catalog();

            _dataModel = new ApplicationDataModel();
            _dataModel.Documents = _documents;
            _dataModel.Catalog = _catalog;

            _operationDataMapper = new Mock<IOperationDataMapper>();

            _loggedDataMapper = new LoggedDataMapper(_operationDataMapper.Object);
        }

        [Test]
        public void GivenMultipleTasksWhenMapThenMultipleMapped()
        {
            _tsks.Add(new TSK());
            _tsks.Add(new TSK());
            _tsks.Add(new TSK());
            _tsks.Add(new TSK());

            var result = Map();
            Assert.AreEqual(_tsks.Count, result.Count);
        }

        [Test]
        public void GivenTskWhenMapThenOperationDataAreMapped()
        {
            const string taskName = "TSK5";

            var existingLoggedData = new LoggedData();
            existingLoggedData.Id.UniqueIds.Add(new UniqueId{ CiTypeEnum = CompoundIdentifierTypeEnum.String, Id = taskName, Source = UniqueIdMapper.IsoSource});
            _documents.LoggedData = new List<LoggedData> {existingLoggedData};

            var tlgs = new List<TLG> {new TLG()};
            _tsk.Items = tlgs.ToArray();
            _tsk.A = taskName;

            var operationDatas = new List<OperationData>();
            _operationDataMapper.Setup(x => x.Map(tlgs, null, _dataPath, It.IsAny<int>())).Returns(operationDatas);

            var result = MapSingle();
            Assert.AreSame(operationDatas, result.OperationData);
        }

        [Test]
        public void GivenNullTsksWhenMapThenNull()
        {
            var result = _loggedDataMapper.Map(null, _dataPath, _dataModel);
            Assert.IsNull(result);
        }

        [Test]
        public void GivenNullTskWhenMapThenNull()
        {
            _tsks = new List<TSK>{ null };
            var result = MapSingle();
            Assert.IsNull(result);
        }

        [Test]
        public void GivenNullLoggedDataWhenMapThenNull()
        {
            _documents.LoggedData = null;

            var result = MapSingle();
            Assert.IsNull(result);
        }

        [Test]
        public void GivenNoMatcingLoggedDataInDocumentsWhenMapThenNull()
        {
            _tsk.A = "TSK9";
            _documents.LoggedData = new List<LoggedData>{ new LoggedData(), new LoggedData(), new LoggedData()};

            var result = MapSingle();
            Assert.IsNull(result);
        }

        [Test]
        public void GivenTskWithGrdWhenMapThenPrescriptionIdFromCatalogIsPassedToOperationMapper()
        {

            var prescription = new RasterGridPrescription();
            prescription.Id.UniqueIds = new List<UniqueId>
            {
                new UniqueId
                {
                    CiTypeEnum = CompoundIdentifierTypeEnum.String,
                    Id = "FIX1",
                    Source = "http://dictionary.isobus.net/isobus/"
                }
            };

            _catalog.Prescriptions = new List<Prescription> { prescription };


            var existingLoggedData = new LoggedData();
            existingLoggedData.Id.UniqueIds.Add(new UniqueId { CiTypeEnum = CompoundIdentifierTypeEnum.String, Id = "FIX1", Source = UniqueIdMapper.IsoSource });
            _documents.LoggedData = new List<LoggedData> { existingLoggedData };

            var grd = new GRD();
            _tsk.Items = new List<IWriter> {grd}.ToArray();
            _tsk.A = "FIX1";

            MapSingle();

            _operationDataMapper.Verify(x => x.Map(It.IsAny<List<TLG>>(), prescription.Id.ReferenceId, _dataPath,existingLoggedData.Id.ReferenceId ), Times.Once);
        }

        [Test]
        public void GivenTskWithNoGrdWhenMapThenPrescriptionIdIsNullToOperationMapper()
        {
            var existingLoggedData = new LoggedData();
            existingLoggedData.Id.UniqueIds.Add(new UniqueId { CiTypeEnum = CompoundIdentifierTypeEnum.String, Id = "TSK0", Source = UniqueIdMapper.IsoSource });
            _documents.LoggedData = new List<LoggedData> { existingLoggedData };
            _tsk.A = "TSK0";
            _tsk.Items = new IWriter[0];

            MapSingle();

            _operationDataMapper.Verify(x => x.Map(It.IsAny<List<TLG>>(), null, _dataPath, existingLoggedData.Id.ReferenceId), Times.Once);
        }

        [Test]
        public void GivenTskWithNullItemWhenMapThenPrescriptionIdIsNullToOperationMapper()
        {
            var existingLoggedData = new LoggedData();
            existingLoggedData.Id.UniqueIds.Add(new UniqueId { CiTypeEnum = CompoundIdentifierTypeEnum.String, Id = "TSK0", Source = UniqueIdMapper.IsoSource });
            _documents.LoggedData = new List<LoggedData> { existingLoggedData };
            _tsk.A = "TSK0";

            MapSingle();

            _operationDataMapper.Verify(x => x.Map(It.IsAny<List<TLG>>(), null, _dataPath, existingLoggedData.Id.ReferenceId), Times.Once);
        }

        

        private LoggedData MapSingle()
        {
            return Map().First();
        }

        public List<LoggedData> Map()
        {
            return _loggedDataMapper.Map(_tsks, _dataPath, _dataModel).ToList();
        }

    }
}
