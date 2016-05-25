using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
        private MemoryStream _memStream;

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
            CreateTimMemStream("A", a.ToString(CultureInfo.InvariantCulture));
            var xpathDoc = CreateXDoc();

            var result = _timReader.Read(xpathDoc).First();

            Assert.AreEqual(a, Convert.ToDateTime(result.A.Value));
        }

        [Test]
        public void GivenXdocumentWhenReadThenStopIsMapped()
        {
            var b = new DateTime(2015, 05, 15);
            CreateTimMemStream("B", b.ToString(CultureInfo.InvariantCulture));
            var xpathDoc = CreateXDoc();

            var result = _timReader.Read(xpathDoc).First();

            Assert.AreEqual(b, Convert.ToDateTime(result.B.Value));
        }

        [Test]
        public void GivenXdocumentWhenReadThenDurationIsMapped()
        {
            var c = 1500;
            CreateTimMemStream("C", c.ToString(CultureInfo.InvariantCulture));
            var xpathDoc = CreateXDoc();

            var result = _timReader.Read(xpathDoc).First();

            Assert.AreEqual(c, result.C.Value);
        }

        [Test]
        public void GivenXdocumentWhenReadThenTypeIsMapped()
        {
            var d = TIMD.Item4;
            CreateTimMemStream("D", d.ToString());
            var xpathDoc = CreateXDoc();

            var result = _timReader.Read(xpathDoc).First();

            Assert.AreEqual(d, result.D.Value);
        }

        [Test]
        public void GivenXdocumentWhenReadThenPtnHeaderIsMapped()
        {
            _memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(_memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("TIM");
                xmlWriter.WriteStartElement("PTN");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            var xpathDoc = CreateXDoc();

            var ptn = new PTN();
            _ptnReaderMock.Setup(x => x.Read(It.IsAny<XPathNodeIterator>())).Returns(new List<PTN> {ptn});

            var result = _timReader.Read(xpathDoc).First();
            var ptnResult = result.Items[0];
            Assert.AreSame(ptn, ptnResult);
        }

        [Test]
        public void GivenXdocumentWhenReadThenDlvsAreMapped()
        {
            _memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(_memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("TIM");
                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            var xpathDoc = CreateXDoc();

            var dlvs = new List<DLV> { new DLV(), new DLV(), new DLV() };
            _dlvReaderMock.Setup(x => x.Read(It.Is<XPathNodeIterator>( y => y.Count == 3))).Returns(dlvs);

            var result = _timReader.Read(xpathDoc).First();
            Assert.AreSame(dlvs[0], result.Items[0]);
            Assert.AreSame(dlvs[1], result.Items[1]);
            Assert.AreSame(dlvs[2], result.Items[2]);
        }

        [Test]
        public void GivenXdocumentWhenReadThenDlvAndPtnsAreMapped()
        {
            _memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(_memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("TIM");
                xmlWriter.WriteStartElement("PTN");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            var xpathDoc = CreateXDoc();

            var ptn = new PTN();
            _ptnReaderMock.Setup(x => x.Read(It.IsAny<XPathNodeIterator>())).Returns(new List<PTN> { ptn });

            var dlvs = new List<DLV> { new DLV(), new DLV(), new DLV() };
            _dlvReaderMock.Setup(x => x.Read(It.Is<XPathNodeIterator>(y => y.Count == 3))).Returns(dlvs);

            var result = _timReader.Read(xpathDoc).First();
            Assert.AreSame(ptn, result.Items[0]);
            Assert.AreSame(dlvs[0], result.Items[1]);
            Assert.AreSame(dlvs[1], result.Items[2]);
            Assert.AreSame(dlvs[2], result.Items[3]);
        }

        //todo both ptn and dlvs test
        [Test]
        public void GivenXdocumentWithoutTimWhenReadThenIsNull()
        {
            _memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(_memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("NOTTIM");
                xmlWriter.WriteAttributeString("A", "AValue");
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            var xDocument = CreateXDoc();
            var result = _timReader.Read(xDocument);

            Assert.IsNull(result);
        }


        private void CreateTimMemStream(string attributeName, string attributeValue)
        {

            _memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(_memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("TIM");
                xmlWriter.WriteAttributeString(attributeName, attributeValue);
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }
        }

        private XPathDocument CreateXDoc()
        {
            _memStream.Position = 0;
            var xpathDoc = new XPathDocument(_memStream);
            return xpathDoc;
        }
    }
}
