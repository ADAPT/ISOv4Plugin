using System;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.IsoPlugin;
using NUnit.Framework;

namespace IsoPluginTest
{
    [TestFixture]
    public class AllocationTimestampLoaderTests
    {
        [Test]
        public void TimestampStartTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp1.xml");
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.Stamp1.TimeStamp);
            Assert.IsNull(result.Stamp1.Location);
            Assert.AreEqual(DateContextEnum.ProposedStart, result.Stamp1.DateContext);
            Assert.IsNull(result.Stamp2);
        }

        [Test]
        public void TimestampStartAndDurationTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp2.xml");
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.Stamp1.TimeStamp);
            Assert.IsNull(result.Stamp1.Location);
            Assert.AreEqual(DateContextEnum.ProposedStart, result.Stamp1.DateContext);

            Assert.AreEqual(TimeSpan.FromSeconds(1000), result.Stamp2.TimeStamp - result.Stamp1.TimeStamp);
            Assert.IsNull(result.Stamp2.Location);
            Assert.AreEqual(DateContextEnum.ProposedEnd, result.Stamp2.DateContext);
        }

        [Test]
        public void TimestampStartAndStopTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp3.xml");
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.Stamp1.TimeStamp);
            Assert.IsNull(result.Stamp1.Location);
            Assert.AreEqual(DateContextEnum.ProposedStart, result.Stamp1.DateContext);

            Assert.AreEqual(DateTime.Parse("2016-02-02 14:02:01.111", CultureInfo.InvariantCulture), result.Stamp2.TimeStamp);
            Assert.IsNull(result.Stamp2.Location);
            Assert.AreEqual(DateContextEnum.ProposedEnd, result.Stamp2.DateContext);
        }

        [Test]
        public void TimestampTypeTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp4.xml");
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Parse("2016-02-02 14:01:01.111", CultureInfo.InvariantCulture), result.Stamp1.TimeStamp);
            Assert.IsNull(result.Stamp1.Location);
            Assert.AreEqual(DateContextEnum.ActualStart, result.Stamp1.DateContext);

            Assert.AreEqual(DateTime.Parse("2016-02-02 14:02:01.111", CultureInfo.InvariantCulture), result.Stamp2.TimeStamp);
            Assert.IsNull(result.Stamp2.Location);
            Assert.AreEqual(DateContextEnum.ActualEnd, result.Stamp2.DateContext);
        }

        [Test]
        public void MissingOrInvalidTimestampTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp5.xml");
            var xmlNodes = xmlDoc.SelectNodes("//TestCase");

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
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp6.xml");
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            // Act
            var result = AllocationTimestampLoader.Load(inputXmlNode);

            // Verify
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Stamp1.Location);
            Assert.AreEqual(9.989209, result.Stamp1.Location.Position.X);
            Assert.AreEqual(54.588945, result.Stamp1.Location.Position.Y);
            Assert.IsNotNull(result.Stamp1.Location.GpsSource);
            Assert.AreEqual(2, result.Stamp1.Location.GpsSource.NumberOfSatellites);
            Assert.AreEqual(GpsSourceEnum.PreciseGNSS, result.Stamp1.Location.GpsSource.SourceType);
            Assert.AreEqual(DateTime.Parse("1981-05-15 00:00:10", CultureInfo.InvariantCulture), result.Stamp1.Location.GpsSource.GpsUtcTime);
        }

        [Test]
        public void TimestampWithMissingOrInvalidLocationTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp7.xml");
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            var xmlNodes = xmlDoc.SelectNodes("//TestCase");

            foreach (XmlNode xmlNode in xmlNodes)
            {
                // Act
                var result = AllocationTimestampLoader.Load(xmlNode);

                // Verify
                Assert.IsNotNull(result);
                Assert.IsNull(result.Stamp1.Location);
            }
        }

        [Test]
        public void TimestampWithMissingOrInvalidLocationGpsTimeTest()
        {
            // Setup
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(@"TestData\AllocationTimestamp\Timestamp8.xml");
            var inputXmlNode = xmlDoc.SelectSingleNode("ISO11783_TaskData");

            var xmlNodes = xmlDoc.SelectNodes("//TestCase");

            foreach (XmlNode xmlNode in xmlNodes)
            {
                // Act
                var result = AllocationTimestampLoader.Load(xmlNode);

                // Verify
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Stamp1.Location);
                Assert.AreEqual(DateTime.MinValue, result.Stamp1.Location.GpsSource.GpsUtcTime);
            }
        }
    }
}
