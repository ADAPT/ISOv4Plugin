using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Readers;
using Moq;
using NUnit.Framework;
using TestUtilities;

namespace ISOv4PluginLogTest.Readers
{
    [TestFixture]
    public class TaskDataReaderTest
    {
        private TaskDataReader _taskDataReader;
        private XPathNavigator _navigator;
        private XPathDocument _xDocument;
        private Mock<ITsksReader> _tsksReaderMock;
        private string _cardPath;

        [SetUp]
        public void Setup()
        {
            _cardPath = DataCardUtility.WriteDataCard("KV");
            var taskDataPath = Path.Combine(_cardPath, "TASKDATA", "TASKDATA.XML");
            _xDocument = new XPathDocument(taskDataPath);
            _navigator = _xDocument.CreateNavigator();

            _tsksReaderMock = new Mock<ITsksReader>();

            SetupMocks();
            _taskDataReader = new TaskDataReader(_tsksReaderMock.Object);
        }

        private void SetupMocks()
        {
            _tsksReaderMock.Setup(x => x.Read(It.IsAny<XPathNodeIterator>())).Returns(new List<TSK>());
        }

        [TearDown]
        public void Teardown()
        {
            Directory.Delete(_cardPath, true);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenVersionMajorIsPopulated()
        {
            var result = _taskDataReader.Read(_navigator, _cardPath);
            Assert.AreEqual(3, result.VersionMajor);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenVersionMinorIsPopulated()
        {
            var result = _taskDataReader.Read(_navigator, _cardPath);
            Assert.AreEqual(0, result.VersionMinor);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenManagementSoftwareManufacturerIsRead()
        {
            var result = _taskDataReader.Read(_navigator, _cardPath);
            Assert.AreEqual("AEF104_20150612_123745", result.ManagementSoftwareManufacturer);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenManagementSoftwareVersionIsRead()
        {
            var result = _taskDataReader.Read(_navigator, _cardPath);
            Assert.AreEqual("1.0", result.ManagementSoftwareVersion);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenTaskControllerManufacturerIsRead()
        {
            var result = _taskDataReader.Read(_navigator, _cardPath);
            Assert.AreEqual("Kverneland Group Mechatronics", result.TaskControllerManufacturer);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenTaskControllerVersionIsRead()
        {
            var result = _taskDataReader.Read(_navigator, _cardPath);
            Assert.AreEqual("0.9.67", result.TaskControllerVersion);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenDataTransferOriginIsRead()
        {
            var result = _taskDataReader.Read(_navigator, _cardPath);
            Assert.AreEqual(ISO11783_TaskDataDataTransferOrigin.Item2, result.DataTransferOrigin);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenTasksReaderIsCalledWithCorrectTasks()
        {
            var navigator = CreateXmlMemoryStream(2, 0);
            
            XPathNodeIterator iterator = null;
            _tsksReaderMock.Setup(x => x.Read(It.IsAny<XPathNodeIterator>()))
                .Callback<XPathNodeIterator>(x => iterator = x).Returns(new List<TSK>());

            _taskDataReader.Read(navigator, _cardPath);

            Assert.AreEqual(2, iterator.Count);
            AssertIterators(new List<XPathNodeIterator> {iterator});
        }

        [Test]
        public void GivenXPathNavigatorWithExternalTskWhenReadThenTasksReaderIsCalledWithCorrectTasks()
        {
            var navigator = CreateXmlMemoryStream(2, 1);
            var iterators = new List<XPathNodeIterator>();
            
            _tsksReaderMock.Setup(x => x.Read(It.IsAny<XPathNodeIterator>()))
                .Callback<XPathNodeIterator>(iterators.Add).Returns(() =>
                {
                    var tsks = new List<TSK>();
                    tsks.AddRange(Enumerable.Repeat(new TSK(), iterators.Last().Count));
                    return tsks;
                });

            WriteExternalTskFile(3);

            var result = _taskDataReader.Read(navigator, _cardPath);

            Assert.AreEqual(3 , result.Items.Length);
            AssertIterators(iterators);
        }

        [Test]
        public void GivenXPathNavigatorWithTwoExternalTskWhenReadThenTasksReaderIsCalledWithCorrectTasks()
        {
            var navigator = CreateXmlMemoryStream(2, 2);
            var iterators = new List<XPathNodeIterator>();

            _tsksReaderMock.Setup(x => x.Read(It.IsAny<XPathNodeIterator>()))
                .Callback<XPathNodeIterator>(iterators.Add).Returns(() =>
                {
                    var tsks = new List<TSK>();
                    tsks.AddRange(Enumerable.Repeat(new TSK(), iterators.Last().Count));
                    return tsks;
                });

            WriteExternalTskFile(3);
            WriteExternalTskFile(4);

            var result = _taskDataReader.Read(navigator, _cardPath);

            Assert.AreEqual(4, result.Items.Length);
            AssertIterators(iterators);
        }

        [Test]
        public void GivenXPathNavigatorWithExternalDoesNotExistkWhenReadThenTasksReaderIsCalledForOtherTasks()
        {
            var navigator = CreateXmlMemoryStream(2, 2);
            var iterators = new List<XPathNodeIterator>();

            _tsksReaderMock.Setup(x => x.Read(It.IsAny<XPathNodeIterator>()))
                .Callback<XPathNodeIterator>(iterators.Add).Returns(() =>
                {
                    var tsks = new List<TSK>();
                    tsks.AddRange(Enumerable.Repeat(new TSK(), iterators.Last().Count));
                    return tsks;
                });

            WriteExternalTskFile(3);

            var result = _taskDataReader.Read(navigator, _cardPath);

            Assert.AreEqual(3, result.Items.Length);
            AssertIterators(iterators);
        }

        private XPathNavigator CreateXmlMemoryStream(int numberOfTaskElements, int numberOfXfrElements)
        {
            var memoryStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings {Encoding = Encoding.ASCII}))
            {
                xmlWriter.WriteStartElement("ISO11783_TaskData");
                xmlWriter.WriteAttributeString("VersionMajor", "3");
                xmlWriter.WriteAttributeString("VersionMinor", "0");
                xmlWriter.WriteAttributeString("DataTransferOrigin", "2");

                int taskNumber = 1;

                for (int i = 0; i < numberOfTaskElements; i++)
                {
                    xmlWriter.WriteStartElement("TSK");
                    xmlWriter.WriteAttributeString("A", "TSK" + taskNumber++);
                    xmlWriter.WriteElementString("BLAH", "3");
                    xmlWriter.WriteEndElement();    
                }

                for (int i = 0; i < numberOfXfrElements; i++)
                {
                    xmlWriter.WriteStartElement("XFR");
                    xmlWriter.WriteAttributeString("A", "TSK" + taskNumber++);
                    xmlWriter.WriteAttributeString("B", "1");
                    xmlWriter.WriteEndElement();    
                }
                
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            memoryStream.Position = 0;
            var xDocument = new XPathDocument(memoryStream);
            return xDocument.CreateNavigator();
        }

        private void WriteExternalTskFile(int i)
        {
            var memoryStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings {Encoding = Encoding.ASCII}))
            {
                xmlWriter.WriteStartElement("XFC");
                xmlWriter.WriteStartElement("TSK");
                xmlWriter.WriteAttributeString("A", "TSK" + i);
                xmlWriter.WriteElementString("BLAH", "3");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            File.WriteAllBytes(Path.Combine(_cardPath, "TSK" + i + ".xml"), memoryStream.ToArray());

        }

        private void AssertIterators(IEnumerable<XPathNodeIterator> xPathNodeIterators)
        {
            int index = 1;
            foreach (var iterator in xPathNodeIterators)
            {
                foreach (XPathNavigator node in iterator)
                {
                    var aAttribute = node.GetAttribute("A", node.NamespaceURI);
                    Assert.AreEqual("TSK" + index, aAttribute);

                    index++;
                }
            }
        }
    }
}
