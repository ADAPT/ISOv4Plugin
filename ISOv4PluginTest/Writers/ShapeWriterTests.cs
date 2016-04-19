using System.IO;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class ShapeWriterTests
    {
        [TearDown]
        public void Cleanup()
        {
            var folderLocation = TestContext.CurrentContext.WorkDirectory + @"\TASKDATA";
            if (Directory.Exists(folderLocation))
                Directory.Delete(folderLocation, true);
        }
    }
}
