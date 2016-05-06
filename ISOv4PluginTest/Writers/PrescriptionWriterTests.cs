using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class PrescriptionWriterTests
    {
        [TearDown]
        public void Cleanup()
        {
            var folderLocation = TestContext.CurrentContext.WorkDirectory + @"\TASKDATA";
            if (Directory.Exists(folderLocation))
                Directory.Delete(folderLocation, true);
        }

        [Test]
        public void ShouldWritePrescription()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Prescription\SingleProduct.json");

            // Act
            using (taskWriter)
            {
                var actualXml = taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            
                Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Prescription\SingleProductOutput.xml"),
                    actualXml.ToString());
            }

            // Verify
            Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Prescription\SingleProductOutput.TXT"),
                TestHelpers.LoadFromFileAsHexString(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\GRD00000.BIN"));
        }
    }
}
