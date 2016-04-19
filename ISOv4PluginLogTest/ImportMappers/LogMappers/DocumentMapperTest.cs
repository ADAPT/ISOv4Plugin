using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
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
        private DocumentMapper _documentMapper;
        private Mock<ILoggedDataMapper> _loggedDataMapperMock;
        private Documents _documents;
        private string _dataPath;
        private Catalog _catalog;
        private ISO11783_TaskData _isoTaskData;

        [SetUp]
        public void Setup()
        {
            _tsks = new List<TSK> { new TSK() };
            _documents = new Documents();
            _dataPath = "";
            _loggedDataMapperMock = new Mock<ILoggedDataMapper>();
            _catalog = new Catalog();
            _isoTaskData = new ISO11783_TaskData();
            _documentMapper = new DocumentMapper(_loggedDataMapperMock.Object);
        }

        [Test]
        public void GivenOneTlgWhenMapThenSomething()
        {
            _documentMapper.Map(_tsks, _documents, _dataPath, _catalog, _isoTaskData);

            Assert.AreEqual(1, _documents.LoggedData.Count);
        }

        [Test]
        public void GivenOneTlgWhenMapThenLoggedDataIsMapped()
        {
            var loggedData = new LoggedData();
            _loggedDataMapperMock.Setup(x => x.Map(_tsks.First(), _dataPath, _catalog)).Returns(loggedData);
            _documentMapper.Map(_tsks, _documents, _dataPath, _catalog, _isoTaskData);

            Assert.AreSame(loggedData, _documents.LoggedData.First());
        }
    }
}
