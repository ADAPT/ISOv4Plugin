using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Readers;
using Moq;
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
        private Mock<IDlvReader> _dlvReaderMock;

        [SetUp]
        public void Setup()
        {
            var cardPath = DataCardUtility.WriteDataCard("KV");
            var taskDataPath = Path.Combine(cardPath, "TASKDATA", "TASKDATA.XML");
            var xDocument = new XPathDocument(taskDataPath);
            var navigator = xDocument.CreateNavigator();
            var taskDataNode = navigator.SelectSingleNode("ISO11783_TaskData");
            _children = taskDataNode.SelectChildren(XPathNodeType.Element);
            _iterator = _children.Current.Select("./TSK/TIM");
            _dlvReaderMock = new Mock<IDlvReader>();

            _taskdataTimReader = new TaskdataTimReader(_dlvReaderMock.Object);
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

        [Test]
        public void GivenIteratorWhenReadThenDLVsAreMapped()
        {
            var navigator = CreateXmlMemoryStream();
          //  var children = navigator.SelectChildren(XPathNodeType.Element);
            var children = navigator.SelectDescendants("TIM", navigator.NamespaceURI, true);

            var result = _taskdataTimReader.Read(children).First();

            _dlvReaderMock.Verify(x => x.Read(It.Is<XPathNodeIterator>(y => y.Count == 1)), Times.Once);
            
        }

        private XPathNavigator CreateXmlMemoryStream()
        {
            var memoryStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Encoding = Encoding.ASCII }))
            {
                xmlWriter.WriteStartElement("ISO11783_TaskData");
                xmlWriter.WriteAttributeString("VersionMajor", "3");
                xmlWriter.WriteAttributeString("VersionMinor", "0");
                xmlWriter.WriteAttributeString("DataTransferOrigin", "2");

                xmlWriter.WriteStartElement("TSK");
                xmlWriter.WriteAttributeString("A", "TSK1");

                xmlWriter.WriteStartElement("TIM");
                xmlWriter.WriteAttributeString("A", "2015-06-12 12:43:29.000");
                xmlWriter.WriteAttributeString("B", "2015-06-12 12:53:29.000");
                xmlWriter.WriteAttributeString("D", "1");

                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteAttributeString("A", "0075");
                xmlWriter.WriteAttributeString("B", "0");
                xmlWriter.WriteAttributeString("C", "DET-1");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            memoryStream.Position = 0;
            var xDocument = new XPathDocument(memoryStream);
            return xDocument.CreateNavigator();
        }
    }
}
