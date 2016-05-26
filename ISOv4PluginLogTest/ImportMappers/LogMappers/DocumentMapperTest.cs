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
        [Test]
        public void GivenTsksWhenMapThenLoggedDataAreMapped()
        {
            var tsks = new List<TSK>();
            var dataPath = Path.GetTempPath();
            var documents = new Documents();
            var dataModel = new ApplicationDataModel {Documents = documents};

            var loggedDataMapperMock = new Mock<ILoggedDataMapper>();

            var documentMapper = new DocumentMapper(loggedDataMapperMock.Object);
            var linkIds = new Dictionary<string, List<UniqueId>>();
            documentMapper.Map(tsks, dataPath, dataModel, linkIds);

            loggedDataMapperMock.Verify(x => x.Map(tsks, dataPath, dataModel, linkIds), Times.Once);
        }
    }
}
