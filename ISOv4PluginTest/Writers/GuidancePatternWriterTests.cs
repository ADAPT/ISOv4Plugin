using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class GuidancePatternWriterTests
    {
        [TearDown]
        public void Cleanup()
        {
            var folderLocation = TestContext.CurrentContext.WorkDirectory + @"\TASKDATA";
            if (Directory.Exists(folderLocation))
                Directory.Delete(folderLocation, true);
        }

        [Test]
        public void ShouldWriteAllTypesOfPatterns()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(@"TestData\Guidance\AllPatterns.json");

            // Act
            using (taskWriter)
            {
                var actual = taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);

                Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\Guidance\AllPatternsOutput.xml"), actual.ToString());
            }
        }
    }
}
