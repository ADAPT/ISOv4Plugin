using System;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Readers;
using NUnit.Framework;
using TestUtilities;

namespace ISOv4PluginLogTest.Readers
{
    [TestFixture]
    public class TaskdataTimReaderTest
    {
        private XPathNodeIterator _iterator;
        private TaskdataTimReader _taskdataTimReader;
        private XPathNodeIterator _children;

        [SetUp]
        public void Setup()
        {
            var cardPath = DataCardUtility.WriteDataCard("KV");
            var taskDataPath = Path.Combine(cardPath, "TASKDATA.XML");
            var xDocument = new XPathDocument(taskDataPath);
            var navigator = xDocument.CreateNavigator();
            var taskDataNode = navigator.SelectSingleNode("ISO11783_TaskData");
            _children = taskDataNode.SelectChildren(XPathNodeType.Element);
            _iterator = _children.Current.Select("./TSK/TIM");

            _taskdataTimReader = new TaskdataTimReader();
        }

        [Test]
        public void GivenIteratorWhenEmptyThenEmptyCollectionReturned()
        {
            _iterator = _children.Current.Select("./zzzzzz");

            var result = _taskdataTimReader.Read(_iterator);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenIteratorWhenReadThenAllItemsAreRead()
        {
            var result = _taskdataTimReader.Read(_iterator);
            Assert.AreEqual(6, result.Count());
        }

        [Test]
        public void GivenIteratorWhenReadThenAIsRead()
        {
            var result = _taskdataTimReader.Read(_iterator).First();
            Assert.AreEqual(DateTime.Parse("2015-06-12 12:43:29.000"), result.A);
        }

        [Test]
        public void GivenIteratorWhenReadthenBIsRead()
        {
            var result = _taskdataTimReader.Read(_iterator).First();
            Assert.AreEqual(DateTime.Parse("2015-06-12 12:44:16.000"), result.B);
        }

        [Test]
        public void GivenIteratorWhenReadThenDIsRead()
        {
            var result = _taskdataTimReader.Read(_iterator).First();
            Assert.AreEqual(TIMD.Item4, result.D);
        }
    }
}
