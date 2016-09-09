using System.Collections.Generic;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class DocumentMapperTest
    {
        private List<TSK> _tsks;
        private string _dataPath;
        private Documents _documents;
        private ApplicationDataModel _dataModel;
        private Mock<ILoggedDataMapper> _loggedDataMapperMock;
        private DocumentMapper _documentMapper;
        private Dictionary<string, List<UniqueId>> _linkedIds;
        private Mock<IWorkOrderMapper> _workOrderMapperMock;

        [SetUp]
        public void Setup()
        {
            _tsks = new List<TSK>();
            _dataPath = Path.GetTempPath();
            _documents = new Documents();
            _dataModel = new ApplicationDataModel { Documents = _documents };

            _loggedDataMapperMock = new Mock<ILoggedDataMapper>();
            _workOrderMapperMock = new Mock<IWorkOrderMapper>();

            _documentMapper = new DocumentMapper(_loggedDataMapperMock.Object, _workOrderMapperMock.Object);
            _linkedIds = new Dictionary<string, List<UniqueId>>();            
        }

        [Test]
        public void GivenTsksWhenMapThenLoggedDataAreMappedFromTskWithTlg()
        {
            _tsks.Add(new TSK{Items = new []{new TLG()}});
            _documentMapper.Map(_tsks, _dataPath, _dataModel, _linkedIds);

            _loggedDataMapperMock.Verify(x => x.Map(_tsks, _dataPath, _dataModel, _linkedIds), Times.Once);
        }

        [Test]
        public void GivenTsksWhenMapThenWorkOrdersAreMappedFromTskWithoutTlg()
        {
            _tsks.Add(new TSK{Items = new IWriter[]{}});
            _documentMapper.Map(_tsks, _dataPath, _dataModel, _linkedIds);

            _workOrderMapperMock.Verify(x => x.Map(_tsks, _dataModel), Times.Once);
        }
    }
}
