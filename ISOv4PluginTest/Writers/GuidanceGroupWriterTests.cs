using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class GuidanceGroupWriterTests
    {
        [TearDown]
        public void Cleanup()
        {
            var folderLocation = TestContext.CurrentContext.WorkDirectory + @"\TASKDATA";
            if (Directory.Exists(folderLocation))
                Directory.Delete(folderLocation, true);
        }

        [Test]
        public void ShouldWriteGroupsWithNoPatternsOrBoundary()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Guidance\GroupsNoPatternsOrBoundary.json");

            // Act
            using (taskWriter)
            {
                var actual = taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
                Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Guidance\GroupsNoPatternsOrBoundaryOutput.xml"),
                    actual.ToString());
            }
        }

        [Test]
        public void ShouldWriteGroupsWithBoundaryAndNoPatterns()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Guidance\GroupsWithBoundaryAndNoPatterns.json");

            // Act
            using (taskWriter)
            {
                var actual = taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
                var loadFromFile = TestHelpers.LoadFromFile(@"TestData\Guidance\GroupsWithBoundaryAndNoPatternsOutput.xml");
                Assert.AreEqual(loadFromFile, actual.ToString().Replace("{","").Replace("}",""));
            }
        }

        [Test]
        public void ShouldWriteGroupsWithPatternsAndBoundary()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Guidance\GroupsWithBoundaryAndPatterns.json");

            // Act
            using (taskWriter)
            {
                var actual = taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            
                Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Guidance\GroupsWithBoundaryAndPatternsOutput.xml"),
                    actual.ToString());
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
            Assert.AreEqual(false, Directory.Exists(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA"));
        }
    }
}
