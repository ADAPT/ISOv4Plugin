using System;
using System.IO;
using System.Text;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class WorkerWriterTests
    {
        private string _directory;
        private TaskDocumentWriter _taskWriter;

        [SetUp]
        public void Setup()
        {
            _taskWriter = new TaskDocumentWriter();
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void ShouldWriteWorkersWithAllData()
        {
            // Setup
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.WorkersWithAllData);

            // Act
            using (_taskWriter)
            {
                var actualXml = TestHelpers.Export(_taskWriter, adaptDocument, _directory);
                
                // Verify
                Assert.AreEqual(TestData.TestData.WorkersWithAllDataOutput, actualXml);
            }
        }

        [Test]
        public void ShouldWriteWorkersWithNonExistentOrMissingContactInfo()
        {
            // Setup
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.WorkersWithNoContacts);

            // Act
            using (_taskWriter)
            {
                var result = TestHelpers.Export(_taskWriter, adaptDocument, _directory);

                Assert.AreEqual(TestData.TestData.WorkersWithNoContactsOutput, result);
            }
        }

        [Test]
        public void ShouldNotWriteWorkersWhenNoneAreAvailable()
        {
            // Setup
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.NoWorkersPresent);

            // Act
            using (_taskWriter)
            {
                _taskWriter.Write(_directory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(false, File.Exists(Path.Combine(_directory, "TASKDATA", "WKR00000.XML")));
        }

        [Test]
        public void ShouldNotWriteWorkersWhenZeroAreAvailable()
        {
            // Setup
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.ZeroWorkersPresent);

            // Act
            using (_taskWriter)
            {
                _taskWriter.Write(_directory, adaptDocument);
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
