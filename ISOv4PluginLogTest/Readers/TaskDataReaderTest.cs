using System.Collections.Generic;
using System.IO;
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
            var taskDataPath = Path.Combine(_cardPath, "TASKDATA.XML");
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
            var result = _taskDataReader.Read(_navigator);
            Assert.AreEqual(3, result.VersionMajor);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenVersionMinorIsPopulated()
        {
            var result = _taskDataReader.Read(_navigator);
            Assert.AreEqual(0, result.VersionMinor);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenManagementSoftwareManufacturerIsRead()
        {
            var result = _taskDataReader.Read(_navigator);
            Assert.AreEqual("AEF104_20150612_123745", result.ManagementSoftwareManufacturer);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenManagementSoftwareVersionIsRead()
        {
            var result = _taskDataReader.Read(_navigator);
            Assert.AreEqual("1.0", result.ManagementSoftwareVersion);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenTaskControllerManufacturerIsRead()
        {
            var result = _taskDataReader.Read(_navigator);
            Assert.AreEqual("Kverneland Group Mechatronics", result.TaskControllerManufacturer);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenTaskControllerVersionIsRead()
        {
            var result = _taskDataReader.Read(_navigator);
            Assert.AreEqual("0.9.67", result.TaskControllerVersion);
        }

        [Test]
        public void GivenXPathNavigatorWhenReadThenDataTransferOriginIsRead()
        {
            var result = _taskDataReader.Read(_navigator);
            Assert.AreEqual(ISO11783_TaskDataDataTransferOrigin.Item2, result.DataTransferOrigin);
        }
    }
}
