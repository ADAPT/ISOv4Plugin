using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class ISO11783_TaskDataTest
    {
        private ISO11783_TaskData _taskData;

        [SetUp]
        public void Setup()
        {
            _taskData = new ISO11783_TaskData();
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("<ISO11783_TaskData xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" "));
            Assert.True(result.Contains("</ISO11783_TaskData>"));
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenVersionMajorIsWritten()
        {
            _taskData.VersionMajor = 5;
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("VersionMajor=\"5\""));
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenVersionMinorIsWritten()
        {
            _taskData.VersionMinor = 5;
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("VersionMinor=\"5\""));
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenManagementSoftwareManufacturerIsWritten()
        {
            _taskData.ManagementSoftwareManufacturer = "slinky";
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("ManagementSoftwareManufacturer=\"slinky\""));
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenManagementSoftwareVersionIsWritten()
        {
            _taskData.ManagementSoftwareVersion = "wink";
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("ManagementSoftwareVersion=\"wink\""));
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenDataTransferOriginIsWritten()
        {
            _taskData.DataTransferOrigin = ISO11783_TaskDataDataTransferOrigin.Item2;
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("DataTransferOrigin=\"2\""));
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenTaskControllerManufacturerIsWritten()
        {
            _taskData.TaskControllerManufacturer = "bob";
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("TaskControllerManufacturer=\"bob\""));
        }

        [Test]
        public void GivenTaskDataWhenWriteXmlThenTaskControllerVersionIsWritten()
        {
            _taskData.TaskControllerVersion = "roberto";
            var result = _taskData.WriteXML();
            Assert.True(result.Contains("TaskControllerVersion=\"roberto\""));
        }

        [Test]
        public void GivenTaskDataWithItemsWhenWriteXmlThenItemsAreWritten()
        {
            var mockItem = new Mock<IWriter>();
            _taskData.Items = new object[]{ mockItem.Object, mockItem.Object };

            _taskData.WriteXML();
            mockItem.Verify(x => x.WriteXML(), Times.Exactly(2));
        }
    }
}
