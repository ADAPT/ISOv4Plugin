using System;
using System.Globalization;
using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers.XmlReaders
{
    [TestFixture]
    public class PtnReaderTest
    {
        private PtnReader _ptnReader;

        [SetUp]
        public void Setup()
        {
            _ptnReader = new PtnReader();
        }

        [Test]
        public void GivenXelementWithAWhenReadThenPositionNorthIsSet()
        {
            const long positionNorthValue = 12321;
            var xElement = new XElement("PTN", new XAttribute("A", positionNorthValue.ToString(CultureInfo.InvariantCulture)));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(positionNorthValue, Convert.ToInt32(result.PositionNorth.Value));
        }

        [Test]
        public void GivenXelementWithBWhenReadThenPositionEastIsSet()
        {
            const long positionEastValue = 436346;
            var xElement = new XElement("PTN", new XAttribute("B", positionEastValue.ToString(CultureInfo.InvariantCulture)));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(positionEastValue, Convert.ToInt32(result.PositionEast.Value));
        }

        [Test]
        public void GivenXelementWithCWhenReadThenPositionUpIsSet()
        {
            const long positionSetValue = 332562;
            var xElement = new XElement("PTN", new XAttribute("C", positionSetValue.ToString(CultureInfo.InvariantCulture)));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(positionSetValue, Convert.ToInt32(result.PositionUp.Value));
        }

        [Test]
        public void GivenXelementWithDWhenReadThenPositionStatusIsSet()
        {
            const byte positionStatusValue = 1;
            var xElement = new XElement("PTN", new XAttribute("D", positionStatusValue.ToString(CultureInfo.InvariantCulture)));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(PTND.Item1, (PTND)Convert.ToInt16(result.PositionStatus.Value));
        }

        [Test]
        public void GivenXelementWithEWhenReadThenPdopIsSet()
        {
            const short pdopValue = 2342;
            var xElement = new XElement("PTN", new XAttribute("E", pdopValue.ToString(CultureInfo.InvariantCulture)));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(pdopValue, Convert.ToInt16(result.PDOP.Value));
        }

        [Test]
        public void GivenXelementWithFWhenReadThenHdopIsSet()
        {
            const short hdopValue = 1232;
            var xElement = new XElement("PTN", new XAttribute("F", hdopValue.ToString(CultureInfo.InvariantCulture)));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(hdopValue, Convert.ToInt16(result.HDOP.Value));
        }

        [Test]
        public void GivenXelementWithGWhenReadThenNumberOfSatellitesIsSet()
        {
            const int value = 156;
            var xElement = new XElement("PTN", new XAttribute("G", value));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(value, Convert.ToInt32(result.NumberOfSatellites.Value));
        }

        [Test]
        public void GivenXelementWithHWhenReadThenGpsUtcTimeIsSet()
        {
            const int value = 15486632;
            var xElement = new XElement("PTN", new XAttribute("H", value));

            var result = _ptnReader.Read(xElement);

            Assert.AreEqual(value, Convert.ToInt32(result.GpsUtcTime.Value));
        }

        [Test]
        public void GivenXelementWithIWhenReadThenGpsUtcDateIsSet()
        {
            const int value = 23456;
            var xElement = new XElement("PTN", new XAttribute("I", value));

            var result = _ptnReader.Read(xElement);
            Assert.AreEqual(value, Convert.ToInt32(result.GpsUtcDate.Value));
        }

        [Test]
        public void GivenNullXelementWhenReadThenIsNull()
        {
            var result = _ptnReader.Read(null);
            Assert.IsNull(result);
        }
    }
}
