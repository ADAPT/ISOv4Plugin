using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest
{
    [TestFixture]
    public class ImporterTest
    {
        private string _dataPath;
        private ApplicationDataModel _applicationDataModel;
        private ISO11783_TaskData _taskData;
        private Mock<IDocumentMapper> _documentMapperMock;
        private Importer _importer;


        [SetUp]
        public void Setup()
        {
            _taskData = new ISO11783_TaskData();
            _dataPath = Path.GetTempPath();
            _applicationDataModel = new ApplicationDataModel();
            _documentMapperMock = new Mock<IDocumentMapper>();

            _importer = new Importer(_documentMapperMock.Object);
        }

        [Test]
        public void GivenTaskDataWhenImportThenDocumentsAreMapped()
        {
            _taskData.Items = new IWriter[] {new TSK(), new TSK(), new PDT(), new TSK(), new TLG()};
            var tasks = _taskData.Items.GetItemsOfType<TSK>();

            _importer.Import(_taskData, _dataPath, _applicationDataModel);

            _documentMapperMock.Verify(x => x.Map(tasks, _dataPath, _applicationDataModel.Documents), Times.Once());
        }

        [Test]
        public void GivenTaskDataWithNullItemsWhenImportThenDocumentsNotMapped()
        {
            _taskData.Items = null;

            _importer.Import(_taskData, _dataPath, _applicationDataModel);

            _documentMapperMock.Verify(x => x.Map(It.IsAny<List<TSK>>(), _dataPath, _applicationDataModel.Documents), Times.Never);
        }

        [Test]
        public void GivenTaskDataWithEmptyItemsWhenImportThenDocumentsNotMapped()
        {
            _taskData.Items = new IWriter[] {};

            _importer.Import(_taskData, _dataPath, _applicationDataModel);

            _documentMapperMock.Verify(x => x.Map(It.IsAny<List<TSK>>(), _dataPath, _applicationDataModel.Documents), Times.Never);
        }

        [Test]
        public void GivenAdmWithNullCatalogWhenImportThenCatalogCreated()
        {
            _applicationDataModel.Catalog = null;

            _importer.Import(_taskData, _dataPath, _applicationDataModel);

            Assert.IsEmpty(_applicationDataModel.Catalog.Brands);
            Assert.IsEmpty(_applicationDataModel.Catalog.Connectors);
            Assert.IsEmpty(_applicationDataModel.Catalog.ContactInfo);
            Assert.IsEmpty(_applicationDataModel.Catalog.Containers);
            Assert.IsEmpty(_applicationDataModel.Catalog.CropProtectionProducts);
            Assert.IsEmpty(_applicationDataModel.Catalog.CropVarieties);
            Assert.IsEmpty(_applicationDataModel.Catalog.CropZones);
            Assert.IsEmpty(_applicationDataModel.Catalog.Crops);
            Assert.IsEmpty(_applicationDataModel.Catalog.EquipmentConfigs);
            Assert.IsEmpty(_applicationDataModel.Catalog.Farms);
            Assert.IsEmpty(_applicationDataModel.Catalog.FertilizerProducts);
            Assert.IsEmpty(_applicationDataModel.Catalog.FieldBoundaries);
            Assert.IsEmpty(_applicationDataModel.Catalog.Fields);
            Assert.IsEmpty(_applicationDataModel.Catalog.Growers);
            Assert.IsEmpty(_applicationDataModel.Catalog.GuidanceGroups);
            Assert.IsEmpty(_applicationDataModel.Catalog.GuidancePatterns);
            Assert.IsEmpty(_applicationDataModel.Catalog.ImplementConfigurations);
            Assert.IsEmpty(_applicationDataModel.Catalog.ImplementModels);
            Assert.IsEmpty(_applicationDataModel.Catalog.ImplementTypes);
            Assert.IsEmpty(_applicationDataModel.Catalog.Implements);
            Assert.IsEmpty(_applicationDataModel.Catalog.Ingredients);
            Assert.IsEmpty(_applicationDataModel.Catalog.MachineConfigurations);
            Assert.IsEmpty(_applicationDataModel.Catalog.MachineModels);
            Assert.IsEmpty(_applicationDataModel.Catalog.MachineSeries);
            Assert.IsEmpty(_applicationDataModel.Catalog.MachineTypes);
            Assert.IsEmpty(_applicationDataModel.Catalog.Machines);
            Assert.IsEmpty(_applicationDataModel.Catalog.PersonRoles);
            Assert.IsEmpty(_applicationDataModel.Catalog.Persons);
            Assert.IsEmpty(_applicationDataModel.Catalog.Prescriptions);
            Assert.IsEmpty(_applicationDataModel.Catalog.ProductMixes);
        }

        [Test]
        public void GivenAdmWithNullDocumentsWhenImportThenDocumentsCreated()
        {
            _applicationDataModel.Documents = null;

            _importer.Import(_taskData, _dataPath, _applicationDataModel);

            Assert.IsEmpty(_applicationDataModel.Documents.GuidanceAllocations);
            Assert.IsEmpty(_applicationDataModel.Documents.LoggedData);
            Assert.IsEmpty(_applicationDataModel.Documents.Plans);
            Assert.IsEmpty(_applicationDataModel.Documents.Recommendations);
            Assert.IsEmpty(_applicationDataModel.Documents.Summaries);
            Assert.IsEmpty(_applicationDataModel.Documents.WorkItemOperations);
            Assert.IsEmpty(_applicationDataModel.Documents.WorkItems);
            Assert.IsEmpty(_applicationDataModel.Documents.WorkOrders);
        }
    }
}
