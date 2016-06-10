using System;
using System.IO;
using System.Text;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class GuidancePatternWriterTests
    {
        private string _exportPath;

        [SetUp]
        public void Setup()
        {
            _exportPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_exportPath);
        }

        [Test]
        public void ShouldWriteAllTypesOfPatterns()
        {
            // Setup
            var taskWriter = new TaskDocumentWriter();
            var adaptDocument = TestHelpers.LoadFromJson<ApplicationDataModel>(TestData.TestData.AllPatterns);

            // Act
            using (taskWriter)
            {
                var actual = TestHelpers.Export(taskWriter, adaptDocument, _exportPath);

                Assert.AreEqual(TestData.TestData.AllPatternsOutput, actual);
            }
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(_exportPath))
                Directory.Delete(_exportPath, true);
        }
    }
}
