using System;
using System.IO;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class WorkerWriterTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void ShouldWriteWorkersWithAllData()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.WorkersWithAllData);

            // Act
            using (taskWriter)
            {
                var resultingXml = taskWriter.Write(_directory, adaptDocument);
                
                // Verify
                Assert.AreEqual(TestData.TestData.WorkersWithAllDataOutput, resultingXml.ToString());
            }
        }

        [Test]
        public void ShouldWriteWorkersWithNonExistentOrMissingContactInfo()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.WorkersWithNoContacts);

            // Act
            using (taskWriter)
            {
                var result = taskWriter.Write(_directory, adaptDocument);

                Assert.AreEqual(TestData.TestData.WorkersWithNoContactsOutput, result.ToString());
            }
        }

        [Test]
        public void ShouldNotWriteWorkersWhenNoneAreAvailable()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.NoWorkersPresent);

            // Act
            using (taskWriter)
            {
                taskWriter.Write(_directory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(false, File.Exists(Path.Combine(_directory, "TASKDATA", "WKR00000.XML")));
        }

        [Test]
        public void ShouldNotWriteWorkersWhenZeroAreAvailable()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.ZeroWorkersPresent);

            // Act
            using (taskWriter)
            {
                taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(false, File.Exists(Path.Combine(_directory, "TASKDATA", "WKR00000.XML")));
        }

        [TearDown]
        public void Cleanup()
        {
            if(Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }
    }
}
