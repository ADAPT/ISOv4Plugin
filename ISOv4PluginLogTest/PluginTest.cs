using System;
using System.IO;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest
{
    [TestFixture]
    public class PluginTest
    {
        private string _dataPath;
        private Mock<IXmlReader> _xmlReaderMock;
        private Mock<IImporter> _importerMock;
        private Mock<IExporter> _exporterMock;
        private Plugin _plugin;

        [SetUp]
        public void Setup()
        {
            _dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            CreateDirectory();

            _xmlReaderMock = new Mock<IXmlReader>();
            _importerMock = new Mock<IImporter>();
            _exporterMock = new Mock<IExporter>();

            _plugin = new Plugin(_xmlReaderMock.Object, _importerMock.Object, _exporterMock.Object);
        }

        [Test]
        public void GivenDataPathWhenImportThenApplicationDataModelsReturned()
        {
            var iso11783TaskData = new ISO11783_TaskData();
            _xmlReaderMock.Setup(x => x.Read(Path.Combine(_dataPath, "taskdata.xml"))).Returns(iso11783TaskData);

            _plugin.Import(_dataPath);
            _importerMock.Verify(x => x.Import(iso11783TaskData, _dataPath, It.IsAny<ApplicationDataModel>()), Times.Once);
        }

        [Test]
        public void GivenDataPathAndAdmWhenExportThenAdmIsExported()
        {
            var adm = new ApplicationDataModel();

            _plugin.Export(adm, _dataPath);
            _exporterMock.Verify(x => x.Export(adm, _dataPath, It.IsAny<XmlWriter>(), It.IsAny<MemoryStream>()), Times.Once);
        }

        [TearDown]
        public void Teardown()
        {
            if(Directory.Exists(_dataPath))
                Directory.Delete(_dataPath, true);
        }

        private void CreateDirectory()
        {
            Directory.CreateDirectory(_dataPath);
            File.Create(Path.Combine(_dataPath, "taskdata.xml")).Dispose();
        }
    }
}
