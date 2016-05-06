using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers.XmlReaders
{
    [TestFixture]
    public class DlvReaderTest
    {
        private DlvReader _dlvReader;

        [SetUp]
        public void Setup()
        {
            _dlvReader = new DlvReader();

        }

        [Test]
        public void GivenXelementsWhenReadThenProcessDataDdiIsMapped()
        {
            var a = "0084";

            var xElement = new XElement("DLV", new XAttribute("A", a));
            var xElements = new List<XElement> {xElement};

            var result = _dlvReader.Read(xElements);
            var expected = Convert.ToInt32(Convert.ToByte(a, 16));

            Assert.AreEqual(expected, result.First().ProcessDataDDI.Value);
        }

        [Test]
        public void GivenXelementWhenReadThenProcessDataValueIsMapped()
        {
            const long b = 84659;
            var xElement = new XElement("DLV", new XAttribute("B", b));
            var xElements = new List<XElement> {xElement};

            var result = _dlvReader.Read(xElements);

            Assert.AreEqual(b.ToString(), result.First().ProcessDataValue.Value);
        }

        [Test]
        public void GivenXelementWhenReadThenDeviceElementIdRefIsMapped()
        {
            const string c = "bob";
            var xElement = new XElement("DLV", new XAttribute("C", c));
            var xElements = new List<XElement> {xElement};

            var result = _dlvReader.Read(xElements);

            Assert.AreEqual(c, result.First().DeviceElementIdRef.Value);
        }

        [Test]
        public void GivenXelementWhenReadThenDataLogPGNIsMapped()
        {
            const ulong d = 298632;
            var xElement = new XElement("DLV", new XAttribute("D", d));
            var xElements = new List<XElement> { xElement };

            var result = _dlvReader.Read(xElements);

            Assert.AreEqual(d.ToString(), result.First().DataLogPGN.Value);
        }

        [Test]
        public void GivenXelementWhenReadThenDataLogPGNStartBitIsMapped()
        {
            const byte e = 4;
            var xElement = new XElement("DLV", new XAttribute("E", e));
            var xElements = new List<XElement> { xElement };

            var result = _dlvReader.Read(xElements);

            Assert.AreEqual(e.ToString(), result.First().DataLogPGNStartBit.Value);
        }

        [Test]
        public void GivenXelementWhenReadThenDataLogPGNStopBitIsMapped()
        {
            const byte f = 6;
            var xElement = new XElement("DLV", new XAttribute("F", f));
            var xElements = new List<XElement> { xElement };

            var result = _dlvReader.Read(xElements);

            Assert.AreEqual(f.ToString(), result.First().DataLogPGNStopBit.Value);
        }

        [Test]
        public void GivenNullXelementWhenReadThenIsNull()
        {
            var result = _dlvReader.Read(null);
            Assert.IsNull(result);
        }
    }
}
