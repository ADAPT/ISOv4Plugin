using System;
using System.IO;
using System.Text;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class PrescriptionWriterTests
    {
        private string _exportPath;

        [SetUp]
        public void Setup()
        {
            _exportPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_exportPath);
        }

        [Test]
        public void ShouldWritePrescription()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.SingleProduct);
            var fertilizerProduct = new FertilizerProduct{ Description = "product"};
            fertilizerProduct.Id.ReferenceId = -1;
            adaptDocument.Catalog.Products.Add(fertilizerProduct);

            // Act
            using (taskWriter)
            {
                var actualXml = TestHelpers.Export(taskWriter, adaptDocument, _exportPath);
                Assert.AreEqual(TestData.TestData.SingleProductOutputXml, actualXml);
            }

            // Verify
            var expectedPath = Path.Combine(_exportPath, "TASKDATA", "GRD00000.BIN");
            Assert.AreEqual(TestData.TestData.SingleProductOutputTxt, TestHelpers.LoadFromFileAsHexString(expectedPath));
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(_exportPath))
                Directory.Delete(_exportPath, true);
        }
    }
}
