using System;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
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

            // Act
            using (taskWriter)
            {
                var actualXml = taskWriter.Write(_exportPath, adaptDocument);
                Assert.AreEqual(TestData.TestData.SingleProductOutputXml, actualXml.ToString());
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
