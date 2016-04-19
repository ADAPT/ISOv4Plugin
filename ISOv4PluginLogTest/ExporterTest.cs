using System.Collections.Generic;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.ReferenceLayers;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
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

            _exporter = new Exporter(_growerFarmFieldMapperMock.Object, _cropZoneMapperMock.Object, _cropTypeMapperMock.Object, _taskMapperMock.Object);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenCtrItemsAreMapped()
        {
            _applicationDataModel.Catalog.Growers = new List<Grower>();

            var ctrs = new List<CTR>{ new CTR(), new CTR() };
            _growerFarmFieldMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Growers, It.IsAny<Dictionary<int, string>>())).Returns(ctrs);

            var result = Export();
            Assert.Contains(ctrs[0], result.Items);
            Assert.Contains(ctrs[1], result.Items);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenFrmItemsAreMapped()
        {
            _applicationDataModel.Catalog.Farms = new List<Farm>();

            var frms = new List<FRM> {new FRM(), new FRM()};
            _growerFarmFieldMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Farms, It.IsAny<Dictionary<int, string>>())).Returns(frms);

            var result = Export();
            Assert.Contains(frms[0], result.Items);
            Assert.Contains(frms[1], result.Items);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenPfdItemsAreMapped()
        {
            _applicationDataModel.Catalog.Fields = new List<Field>();

            var pfds = new List<PFD> { new PFD(), new PFD() };
            _growerFarmFieldMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Fields, It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog)).Returns(pfds);

            var result = Export();
            Assert.Contains(pfds[0], result.Items);
            Assert.Contains(pfds[1], result.Items);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenCtpItemsAreMapped()
        {
            _applicationDataModel.Catalog.Crops = new List<Crop>();

            var ctps = new List<CTP>{ new CTP(), new CTP()};
            _cropTypeMapperMock.Setup(x => x.Map(_applicationDataModel.Catalog.Crops, It.IsAny<Dictionary<int, string>>(), _applicationDataModel.Catalog)).Returns(ctps);

            var result = Export();
            Assert.Contains(ctps[0], result.Items);
            Assert.Contains(ctps[1], result.Items);
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
            Assert.Contains(cropPfds[0], result.Items);
            Assert.Contains(cropPfds[1], result.Items);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenTskItemsAreMapped()
        {
            _applicationDataModel.Documents.LoggedData = new List<LoggedData>();

            var tasks = new List<TSK>{ new TSK(), new TSK()};
            _taskMapperMock.Setup(x => x.Map(_applicationDataModel.Documents.LoggedData, _applicationDataModel.Catalog, _datacardPath, It.IsAny<int>())).Returns(tasks);

            var result = Export();
            Assert.Contains(tasks[0], result.Items);
            Assert.Contains(tasks[1], result.Items);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenDataTransferOriginIsMapped()
        {
            var result = Export();
            Assert.AreEqual(ISO11783_TaskDataDataTransferOrigin.Item1, result.DataTransferOrigin);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenManagementSoftwareVersionIsMapped()
        {
            var result = Export();
            Assert.AreEqual("0.1", result.ManagementSoftwareVersion);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenManagementSoftwareManufacturerIsMapped()
        {
            var result = Export();
            Assert.AreEqual("AgGateway ADAPT", result.ManagementSoftwareManufacturer);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenVersionMajorIsMapped()
        {
            var result = Export();
            Assert.AreEqual(4, result.VersionMajor);
        }

        [Test]
        public void GivenApplicationDataModelWhenExportThenVersionMinorIsMapped()
        {
            var result = Export();
            Assert.AreEqual(1, result.VersionMinor);
        }

        [Test]
        public void GivenNullApplicationDataModelWhenExportThenIsNotChanged()
        {
            var iso11783TaskData = new ISO11783_TaskData();
            var result = _exporter.Export(null, _datacardPath, iso11783TaskData);
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

        private ISO11783_TaskData Export()
        {
            return _exporter.Export(_applicationDataModel, _datacardPath, new ISO11783_TaskData());
        }
    }
}
