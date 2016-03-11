using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.IsoPlugin.Writers;
using NUnit.Framework;

namespace IsoPluginTest.Writers
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
                taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Prescription\SingleProductOutput.xml"),
                TestHelpers.LoadFromFile(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\TSK00000.XML"));
            Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Prescription\SingleProductOutput.TXT"),
                TestHelpers.LoadFromFileAsHexString(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\GRD00000.BIN"));
        }
    }
}
