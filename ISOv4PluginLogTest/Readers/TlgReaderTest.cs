using System.IO;
using System.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Readers;
using NUnit.Framework;
using TestUtilities;

namespace ISOv4PluginLogTest.Readers
{
    [TestFixture]
    public class TlgReaderTest
    {
        private TlgReader _tlgReader;
        private XPathNodeIterator _children;
        private XPathNodeIterator _iterator;

        [SetUp]
        public void Setup()
        {
            var cardPath = DataCardUtility.WriteDataCard("KV");
            var taskDataPath = Path.Combine(cardPath, "TASKDATA", "TASKDATA.XML");
            var xDocument = new XPathDocument(taskDataPath);
            var navigator = xDocument.CreateNavigator();
            var taskDataNode = navigator.SelectSingleNode("ISO11783_TaskData");
            _children = taskDataNode.SelectChildren(XPathNodeType.Element);
            _iterator = _children.Current.Select("./TSK/TLG");

            _tlgReader = new TlgReader();
        }

        [Test]
        public void GivenIteratorWithEmptyCollectionWhenReadThenEmpty()
        {
            _iterator = _children.Current.Select("./zzzzzz");
        }

        [Test]
        public void GivenIteratorWhenReadThenAllElementsAreRead()
        {
            var result = _tlgReader.Read(_iterator);
            Assert.AreEqual(6, result.Count());
        }

        [Test]
        public void GivenIteratorWhenReadThenAIsRead()
        {
            var result = _tlgReader.Read(_iterator).First();
            Assert.AreEqual("TLG00001", result.A);
        }
    }
}
