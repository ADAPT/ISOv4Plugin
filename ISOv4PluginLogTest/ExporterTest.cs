using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest
{
    [TestFixture]
    public class ExporterTest
    {
        private ApplicationDataModel _applicationDataModel;
        private Mock<IGrowerFarmFieldMapper> _growerFarmFieldMapperMock;
        private Mock<ICropZoneMapper> _cropZoneMapperMock;
        private Mock<ICropTypeMapper> _cropTypeMapperMock;
        private Mock<ITaskMapper> _taskMapperMock;
        private Exporter _exporter;
        private string _datacardPath;

        [SetUp]
        public void Setup()
        {
            _applicationDataModel = new ApplicationDataModel
            {
                Catalog = new Catalog(),
                Documents = new Documents()
            };
            _datacardPath = Path.GetTempPath();

            _growerFarmFieldMapperMock = new Mock<IGrowerFarmFieldMapper>();
            _cropZoneMapperMock = new Mock<ICropZoneMapper>();
            _cropTypeMapperMock = new Mock<ICropTypeMapper>();
            _taskMapperMock = new Mock<ITaskMapper>();

            _exporter = new Exporter(_taskMapperMock.Object);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenCtrItemsAreMapped()
        {
            _applicationDataModel.Catalog.Growers = new List<Grower>();

            var ctrs = new List<CTR>{ new CTR(), new CTR() };
            _growerFarmFieldMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Growers, It.IsAny<Dictionary<int, string>>())).Returns(ctrs);

            var result = Export();
            var ctrXml = new StringBuilder();
            ctrs[0].WriteXML(XmlWriter.Create(ctrXml));
            Assert.IsTrue(result.Contains(ctrXml.ToString()));
            
            ctrXml = new StringBuilder();
            ctrs[1].WriteXML(XmlWriter.Create(ctrXml));
            Assert.IsTrue(result.Contains(ctrXml.ToString()));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenFrmItemsAreMapped()
        {
            _applicationDataModel.Catalog.Farms = new List<Farm>();

            var frms = new List<FRM> {new FRM(), new FRM()};
            _growerFarmFieldMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Farms, It.IsAny<Dictionary<int, string>>())).Returns(frms);

            var result = Export();

            var frmXml = new StringBuilder();
            frms[0].WriteXML(XmlWriter.Create(frmXml));
            Assert.IsTrue(result.Contains(frmXml.ToString()));
            
            frmXml = new StringBuilder();
            frms[1].WriteXML(XmlWriter.Create(frmXml));
            Assert.IsTrue(result.Contains(frmXml.ToString()));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenPfdItemsAreMapped()
        {
            _applicationDataModel.Catalog.Fields = new List<Field>();

            var pfds = new List<PFD> { new PFD(), new PFD() };
            _growerFarmFieldMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Fields, It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog)).Returns(pfds);

            var result = Export();
            var pfdXml = new StringBuilder();
            pfds[0].WriteXML(XmlWriter.Create(pfdXml));
            Assert.IsTrue(result.Contains(pfdXml.ToString()));
            
            pfdXml = new StringBuilder();
            pfds[1].WriteXML(XmlWriter.Create(pfdXml));
            Assert.IsTrue(result.Contains(pfdXml.ToString()));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenCtpItemsAreMapped()
        {
            _applicationDataModel.Catalog.Crops = new List<Crop>();

            var ctps = new List<CTP>{ new CTP(), new CTP()};
            _cropTypeMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Crops, It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog)).Returns(ctps);

            var result = Export();
            var ctpXml = new StringBuilder();
            ctps[0].WriteXML(XmlWriter.Create(ctpXml));
            Assert.IsTrue(result.Contains(ctpXml.ToString()));
            
            ctpXml = new StringBuilder();
            ctps[1].WriteXML(XmlWriter.Create(ctpXml));
            Assert.IsTrue(result.Contains(ctpXml.ToString()));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenCropZonePfdItemsAreMapped()
        {
            _applicationDataModel.Catalog.Fields = new List<Field>();
            _applicationDataModel.Catalog.CropZones = new List<CropZone>();

            var fieldPfds = new List<PFD> { new PFD(), new PFD() };
            var cropPfds = new List<PFD> { new PFD(), new PFD() };
            _growerFarmFieldMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Fields, It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog)).Returns(fieldPfds);
            _cropZoneMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.CropZones, fieldPfds, It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog)).Returns(cropPfds);

            var result = Export();
            var cropXml = new StringBuilder();
            cropPfds[0].WriteXML(XmlWriter.Create(cropXml));
            Assert.IsTrue(result.Contains(cropXml.ToString()));

            cropXml = new StringBuilder();
            cropPfds[0].WriteXML(XmlWriter.Create(cropXml));
            Assert.IsTrue(result.Contains(cropXml.ToString()));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenTskItemsAreMapped()
        {
            _applicationDataModel.Documents.LoggedData = new List<LoggedData>();

            var tasks = new List<TSK>{ new TSK(), new TSK()};
            _taskMapperMock.Setup(x => x.Map(_applicationDataModel.Documents.LoggedData, _applicationDataModel.Catalog, _datacardPath, It.IsAny<int>())).Returns(tasks);

            var result = Export();
            var tskXml = new StringBuilder();
            tasks[0].WriteXML(XmlWriter.Create(tskXml));
            Assert.IsTrue(result.Contains(tskXml.ToString()));
            
            tskXml = new StringBuilder();
            tasks[1].WriteXML(XmlWriter.Create(tskXml));
            Assert.IsTrue(result.Contains(tskXml.ToString()));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenDataTransferOriginIsMapped()
        {
            var result = Export();
            Assert.IsTrue(result.Contains("DataTransferOrigin=\"1\""));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenManagementSoftwareVersionIsMapped()
        {
            var result = Export();
            Assert.IsTrue(result.Contains("ManagementSoftwareVersion=\"1.0\""));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenManagementSoftwareManufacturerIsMapped()
        {
            var result = Export();
            Assert.IsTrue(result.Contains("ManagementSoftwareManufacturer=\"AgGateway\""));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenVersionMajorIsMapped()
        {
            var result = Export();
            Assert.IsTrue(result.Contains("VersionMajor=\"4\""));
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenVersionMinorIsMapped()
        {
            var result = Export();
            Assert.IsTrue(result.Contains("VersionMinor=\"0\""));
        }

        [Test]
        public void GivenNullApplicationDataModelWhenExportThenIsNotChanged()
        {
            var iso11783TaskData = XmlWriter.Create(new StringBuilder());
            var result = _exporter.Export(null, _datacardPath, iso11783TaskData, new MemoryStream());
            Assert.AreEqual(iso11783TaskData, result);
        }

        [Test]
        public void GivenApplicationDataModelWithNullCatalogThenIsNull()
        {
            _applicationDataModel.Catalog = null;

            Export();

            _growerFarmFieldMapperMock.Verify(x => x.Map(It.IsAny<List<Grower>>(), It.IsAny<Dictionary<int, string>>()), Times.Never());
            _growerFarmFieldMapperMock.Verify(x => x.Map(It.IsAny<List<Field>>(), It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog), Times.Never());
            _growerFarmFieldMapperMock.Verify(x => x.Map(It.IsAny<List<Farm>>(), It.IsAny<Dictionary<int, string>>()), Times.Never());
            _cropTypeMapperMock.Verify(x => x.Map(It.IsAny<List<Crop>>(), It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog), Times.Never());
            _cropZoneMapperMock.Verify(x => x.Map(It.IsAny<List<CropZone>>(), It.IsAny<List<PFD>>(), It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog), Times.Never());
        }

        [Test]
        public void GivenApplicationDataModelWithTaskMapperNotCalled()
        {
            _applicationDataModel.Documents = null;

            Export();

            _taskMapperMock.Verify(x => x.Map(null, null, _datacardPath, It.IsAny<int>()), Times.Never());
        }

        private string Export()
        {
            using (var taskDocumentWriter = new TaskDocumentWriter())
            {
                var isoTaskData = taskDocumentWriter.Write(Path.GetTempPath(), _applicationDataModel);
                var xmlWriter = _exporter.Export(_applicationDataModel, _datacardPath, isoTaskData, taskDocumentWriter.XmlStream);
                xmlWriter.Flush();
                return Encoding.UTF8.GetString(taskDocumentWriter.XmlStream.ToArray());
            }
        }
    }
}
