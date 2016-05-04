using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
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
            _operationDataMapper.Setup(x => x.Map(tlgs, _dataPath, It.IsAny<int>())).Returns(operationDatas);

            var result = MapSingle();
            Assert.AreSame(operationDatas, result.OperationData);
        }

        [Test]
        public void GivenNullTsksWhenMapThenNull()
        {
            var result = _loggedDataMapper.Map(null, _dataPath, _documents);
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

        private LoggedData MapSingle()
        {
            return Map().First();
        }

        public List<LoggedData> Map()
        {
            return _loggedDataMapper.Map(_tsks, _dataPath, _documents).ToList();
        }

    }
}
