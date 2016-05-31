using System;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Loaders;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class AllocationTimestampLoaderTests
    {
        [Test]
        public void TimestampStartTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(TestData.TestData.Timestamp1);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.TimeStamp1);
            Assert.IsNull(result.Location1);
            Assert.AreEqual(DateContextEnum.ProposedStart, result.DateContext);
            Assert.IsNull(result.TimeStamp2);
        }

        [Test]
        public void TimestampStartAndDurationTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Timestamp2);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.TimeStamp1);
            Assert.IsNull(result.Location1);
            Assert.AreEqual(DateContextEnum.ProposedStart, result.DateContext);

            Assert.AreEqual(TimeSpan.FromSeconds(1000), result.TimeStamp2 - result.TimeStamp1);
            Assert.IsNull(result.Location2);
        }

        [Test]
        public void TimestampStartAndStopTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Timestamp3);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.TimeStamp1);
            Assert.IsNull(result.Location1);
            Assert.AreEqual(DateContextEnum.ProposedStart, result.DateContext);

            Assert.AreEqual(DateTime.Parse("2016-02-02 14:02:01.111", CultureInfo.InvariantCulture), result.TimeStamp2);
            Assert.IsNull(result.Location2);
        }

        [Test]
        public void TimestampTypeTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Timestamp4);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.TimeStamp1);
            Assert.IsNull(result.Location1);
            Assert.AreEqual(DateContextEnum.ActualStart, result.DateContext);

            Assert.AreEqual(DateTime.Parse("2016-02-02 14:02:01.111", CultureInfo.InvariantCulture), result.TimeStamp2);
            Assert.IsNull(result.Location2);
        }

        [Test]
        public void MissingOrInvalidTimestampTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Timestamp5);
            var xmlNodes = xmlDoc.SelectNodes("//TestCase");

            if (xmlNodes == null) return;
            foreach (XmlNode xmlNode in xmlNodes)
            {
                // Act
                var result = AllocationTimestampLoader.Load(xmlNode);

                // Verify
                Assert.IsNull(result);
            }
        }

        [Test]
        public void TimestampWithLocationTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Timestamp6);
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Location1);
            Assert.AreEqual(9.989209, result.Location1.Position.X);
            Assert.AreEqual(54.588945, result.Location1.Position.Y);
            Assert.IsNotNull(result.Location1.GpsSource);
            Assert.AreEqual(2, result.Location1.GpsSource.NumberOfSatellites);
            Assert.AreEqual(GpsSourceEnum.PreciseGNSS, result.Location1.GpsSource.SourceType);
            Assert.AreEqual(DateTime.Parse("1981-05-15 00:00:10", CultureInfo.InvariantCulture), result.Location1.GpsSource.GpsUtcTime);
        }

        [Test]
        public void TimestampWithMissingOrInvalidLocationTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Timestamp7);

            var xmlNodes = xmlDoc.SelectNodes("//TestCase");

            if (xmlNodes == null) return;
            foreach (XmlNode xmlNode in xmlNodes)
            {
                // Act
                var result = AllocationTimestampLoader.Load(xmlNode);

                // Verify
                Assert.IsNotNull(result);
                Assert.IsNull(result.Location1);
            }
        }

        [Test]
        public void TimestampWithMissingOrInvalidLocationGpsTimeTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestData.TestData.Timestamp8);

            var xmlNodes = xmlDoc.SelectNodes("//TestCase");

            if (xmlNodes == null) return;
            foreach (XmlNode xmlNode in xmlNodes)
            {
                // Act
                var result = AllocationTimestampLoader.Load(xmlNode);

                // Verify
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Location1);
                Assert.AreEqual(DateTime.MinValue, result.Location1.GpsSource.GpsUtcTime);
            }
        }
    }
}
