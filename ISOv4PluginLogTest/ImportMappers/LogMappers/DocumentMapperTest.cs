using System.Collections.Generic;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class DocumentMapperTest
    {
        [Test]
        public void GivenTsksWhenMapThenLoggedDataAreMapped()
        {
            var tsks = new List<TSK>();
            var dataPath = Path.GetTempPath();
            var documents = new Documents();

            var loggedDataMapperMock = new Mock<ILoggedDataMapper>();

            var documentMapper = new DocumentMapper(loggedDataMapperMock.Object);
            documentMapper.Map(tsks, dataPath, documents);

            loggedDataMapperMock.Verify(x => x.Map(tsks, dataPath, documents), Times.Once);
        }
    }
}
