using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class WorkerWriterTests
    {
        [TearDown]
        public void Cleanup()
        {
            var folderLocation = TestContext.CurrentContext.WorkDirectory + @"\TASKDATA";
            if (Directory.Exists(folderLocation))
                Directory.Delete(folderLocation, true);
        }

        [Test]
        public void ShouldWriteWorkersWithAllData()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Worker\WorkersWithAllData.json");

            // Act
            using (taskWriter)
            {
                var resultingXml = taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            
                // Verify
                Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Worker\WorkersWithAllDataOutput.xml"),
                    resultingXml.ToString());
            }
        }

        [Test]
        public void ShouldWriteWorkersWithNonExistentOrMissingContactInfo()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Worker\WorkersWithNoContacts.json");

            // Act
            using (taskWriter)
            {
                var result = taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);

                Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Worker\WorkersWithNoContactsOutput.xml"), result.ToString());
            }
        }

        [Test]
        public void ShouldNotWriteWorkersWhenNoneAreAvailable()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Worker\NoWorkersPresent.json");

            // Act
            using (taskWriter)
            {
                taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(false, File.Exists(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\WKR00000.XML"));
        }

        [Test]
        public void ShouldNotWriteWorkersWhenZeroAreAvailable()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Worker\ZeroWorkersPresent.json");

            // Act
            using (taskWriter)
            {
                taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(false, File.Exists(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\WKR00000.XML"));
        }
    }
}
