using System;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class GuidanceGroupWriterTests
    {
        private string _exportPath;

        [SetUp]
        public void Setup()
        {
            _exportPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_exportPath);
        }

        [Test]
        public void ShouldWriteGroupsWithNoPatternsOrBoundary()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.GroupsNoPatternsOrBoundary);

            // Act
            using (taskWriter)
            {
                var actual = taskWriter.Write(_exportPath, adaptDocument);
                Assert.AreEqual(TestData.TestData.GroupsNoPatternsOrBoundaryOutput, actual.ToString());
            }
        }

        [Test]
        public void ShouldWriteGroupsWithBoundaryAndNoPatterns()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.GroupsWithBoundaryAndNoPatterns);

            // Act
            using (taskWriter)
            {
                var actual = taskWriter.Write(_exportPath, adaptDocument);
                Assert.AreEqual(TestData.TestData.GroupsWithBoundaryAndNoPatternsOutput, actual.ToString().Replace("{","").Replace("}",""));
            }
        }

        [Test]
        public void ShouldWriteGroupsWithPatternsAndBoundary()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.GroupsWithBoundaryAndPatterns);

            // Act
            using (taskWriter)
            {
                var actual = taskWriter.Write(_exportPath, adaptDocument);
            
                Assert.AreEqual(TestData.TestData.GroupsWithBoundaryAndPatternsOutput, actual.ToString());
            }
        }

        [Test]
        public void ShouldHandleNullGroup()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var writer = new GuidanceGroupWriter(taskWriter);

            // Act
            writer.Write(taskWriter.RootWriter, null);

            // Verify
            Assert.AreEqual(false, Directory.Exists(Path.Combine(_exportPath, "TASKDATA")));
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(_exportPath))
                Directory.Delete(_exportPath, true);
        }
    }
}
