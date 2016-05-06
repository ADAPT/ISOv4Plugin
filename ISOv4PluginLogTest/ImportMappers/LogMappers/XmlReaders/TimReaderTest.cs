using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers.XmlReaders
{
    [TestFixture]
    public class TimReaderTest
    {
        private Mock<IPtnReader> _ptnReaderMock;
        private Mock<IDlvReader> _dlvReaderMock;
        private TimReader _timReader;

        [SetUp]
        public void Setup()
        {
            _ptnReaderMock = new Mock<IPtnReader>();
            _dlvReaderMock = new Mock<IDlvReader>();

            _timReader = new TimReader(_ptnReaderMock.Object, _dlvReaderMock.Object);
        }

        [Test]
        public void GivenXdocumentWhenReadThenStartTimeIsMapped()
        {
            var a = new DateTime(2015, 05, 15);
            var xDocument = new XDocument ( new XElement("TIM", new XAttribute("A", a)) );

            var result = _timReader.Read(xDocument);

            Assert.AreEqual(a, Convert.ToDateTime(result.Start.Value));
        }

        [Test]
        public void GivenXdocumentWhenReadThenStopIsMapped()
        {
            var b = new DateTime(2015, 05, 15);
            var xDocument = new XDocument ( new XElement("TIM", new XAttribute("B", b)) );

            var result = _timReader.Read(xDocument);

            Assert.AreEqual(b, Convert.ToDateTime(result.Stop.Value));
        }

        [Test]
        public void GivenXdocumentWhenReadThenDurationIsMapped()
        {
            var c = 1500;
            var xDocument = new XDocument ( new XElement("TIM", new XAttribute("C", c)) );

            var result = _timReader.Read(xDocument);

            Assert.AreEqual(c.ToString(), result.Duration.Value);
        }

        [Test]
        public void GivenXdocumentWhenReadThenTypeIsMapped()
        {
            var d = TIMD.Item4;
            var xDocument = new XDocument ( new XElement("TIM", new XAttribute("D", d)) );

            var result = _timReader.Read(xDocument);

            Assert.AreEqual(d.ToString(), result.Type.Value);
        }

        [Test]
        public void GivenXdocumentWhenReadThenPtnHeaderIsMapped()
        {
            var ptnElement = new XElement("PTN");
            var xDocument = new XDocument(new XElement("TIM", ptnElement));

            var ptnHeader = new PTNHeader();
            _ptnReaderMock.Setup(x => x.Read(ptnElement)).Returns(ptnHeader);

            var result = _timReader.Read(xDocument);
            Assert.AreSame(ptnHeader, result.PtnHeader);
        }

        [Test]
        public void GivenXdocumentWhenReadThenDlvsAreMapped()
        {
            var dlvElement1 = new XElement("DLV");
            var dlvElement2 = new XElement("DLV");
            var dlvElement3 = new XElement("DLV");
            var xDocument = new XDocument(new XElement("TIM", dlvElement1, dlvElement2, dlvElement3));

            var dlvs = new List<DLVHeader> { new DLVHeader(), new DLVHeader(), new DLVHeader() };
            _dlvReaderMock.Setup(x => x.Read(new List<XElement>{dlvElement1, dlvElement2, dlvElement3})).Returns(dlvs);

            var result = _timReader.Read(xDocument);
            Assert.AreSame(dlvs[0], result.DLVs[0]);
            Assert.AreSame(dlvs[1], result.DLVs[1]);
            Assert.AreSame(dlvs[2], result.DLVs[2]);
        }

        [Test]
        public void GivenXdocumentWithoutTimWhenReadThenIsNull()
        {
            var xDocument = new XDocument();

            var result = _timReader.Read(xDocument);

            Assert.IsNull(result);
        }
    }
}
