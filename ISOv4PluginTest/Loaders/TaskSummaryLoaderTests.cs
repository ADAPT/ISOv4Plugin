using System;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Loaders;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class TaskSummaryLoaderTests
    {
        [Test]
        public void ShouldHandleZeroTimestampNodes()
        {
            // Setup
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(TestData.TestData.Summary1);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = TaskSummaryLoader.Load(inputXmlNode.SelectNodes("TIM"));

            // Verify
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldHandleMissingTimeNodeAttributes()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Summary2);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = TaskSummaryLoader.Load(inputXmlNode.SelectNodes("TIM"));

            // Verify
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldLoadSingleTimeWithMultipleValues()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Summary3);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = TaskSummaryLoader.Load(inputXmlNode.SelectNodes("TIM"));

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(4, result[0].Values.Count);
            Assert.AreEqual(DateTime.Parse("2017-05-31T18:58:50.535"), result[0].Stamp.TimeStamp1);
            Assert.AreEqual(DateTime.Parse("2017-05-31T20:55:18.543"), result[0].Stamp.TimeStamp2);
        }

        [Test]
        public void ShouldHandleMultipleTims()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Summary4);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = TaskSummaryLoader.Load(inputXmlNode.SelectNodes("TIM"));

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(4, result[0].Values.Count);
            Assert.AreEqual(DateTime.Parse("2017-05-31T18:58:50.535"), result[0].Stamp.TimeStamp1);
            Assert.AreEqual(DateTime.Parse("2017-05-31T20:55:18.543"), result[0].Stamp.TimeStamp2);
            Assert.AreEqual(4, result[1].Values.Count);
            Assert.AreEqual(DateTime.Parse("2017-05-31T20:56:19.710"), result[1].Stamp.TimeStamp1);
            Assert.AreEqual(DateTime.Parse("2017-05-31T20:57:42.932"), result[1].Stamp.TimeStamp2);
        }

        [Test]
        public void ShouldHandleDlvValues()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Summary5);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = TaskSummaryLoader.Load(inputXmlNode.SelectNodes("TIM"));

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].Values.Count);
            Assert.AreEqual(DateTime.Parse("2017-05-31T18:58:50.535"), result[0].Stamp.TimeStamp1);
            Assert.AreEqual(DateTime.Parse("2017-05-31T20:55:18.543"), result[0].Stamp.TimeStamp2);
        }
    }
}
