using System.Collections.Generic;
using System.IO;
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
        [Test]
        public void GivenTsksWhenMapThenLoggedDataAreMapped()
        {
            var tsks = new List<TSK>();
            var documents = new Documents();
            var catalog = new Catalog();
            var dataPath = Path.GetTempPath();

            var loggedDataMapperMock = new Mock<ILoggedDataMapper>();
            var loggedDatas = new List<LoggedData>();
            loggedDataMapperMock.Setup(x => x.Map(tsks, dataPath, catalog)).Returns(loggedDatas);

            var result = new DocumentMapper(loggedDataMapperMock.Object).Map(tsks, documents, catalog, dataPath);

            Assert.AreSame(loggedDatas, result.LoggedData);
        }
    }
}
