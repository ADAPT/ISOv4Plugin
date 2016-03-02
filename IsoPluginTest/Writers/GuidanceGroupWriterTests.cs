using System.IO;
using System.Runtime.Serialization.Formatters;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.IsoPlugin.Writers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IsoPluginTest.Writers
{
    [TestFixture]
    public class GuidanceGroupWriterTests
    {
        private static JsonSerializer _jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                NullValueHandling = NullValueHandling.Ignore
            });

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
            var adaptDocument = LoadApplicationModel(@"TestData\Guidance\GroupsNoPatternsOrBoundary.json");

            // Act
            using (taskWriter)
            {
                taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(LoadFromFile(@"TestData\Guidance\GroupsNoPatternsOrBoundaryOutput.xml"),
                LoadFromFile(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\PFD00000.XML"));
        }

        [Test]
        public void ShouldWriteGroupsWithBoundaryAndNoPatterns()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = LoadApplicationModel(@"TestData\Guidance\GroupsWithBoundaryAndNoPatterns.json");

            // Act
            using (taskWriter)
            {
                taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(LoadFromFile(@"TestData\Guidance\GroupsWithBoundaryAndNoPatternsOutput.xml"),
                LoadFromFile(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\PFD00000.XML"));
        }

        [Test]
        public void ShouldWriteGroupsWithPatternsAndBoundary()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = LoadApplicationModel(@"TestData\Guidance\GroupsWithBoundaryAndPatterns.json");

            // Act
            using (taskWriter)
            {
                taskWriter.Write(TestContext.CurrentContext.WorkDirectory, adaptDocument);
            }

            // Verify
            Assert.AreEqual(LoadFromFile(@"TestData\Guidance\GroupsWithBoundaryAndPatternsOutput.xml"),
                LoadFromFile(TestContext.CurrentContext.WorkDirectory + @"\TASKDATA\PFD00000.XML"));
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


        private static string LoadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        private static ApplicationDataModel LoadApplicationModel(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _jsonSerializer.Deserialize<ApplicationDataModel>(jsonReader);
                }
            }
        }
    }
}
