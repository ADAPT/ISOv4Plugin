using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
        public void GivenNavigatorWhenReadThenProcessDataDdiIsMapped()
        {
            var a = "0084";
            var navigator = CreateNavigator("A", a);
            var nodeIterator = navigator.SelectChildren(XPathNodeType.All);
            var result = _dlvReader.Read(nodeIterator);
            var expected = Convert.ToInt32(Convert.ToByte(a, 16));

            Assert.AreEqual(expected.ToString("X4"), result.First().A);
        }

        [Test]
        public void GivenNavigatorWhenReadThenProcessDataValueIsMapped()
        {
            const long b = 84659;
            var navigator = CreateNavigator("B", b.ToString());
            var nodeIterator = navigator.SelectChildren(XPathNodeType.All);
            var result = _dlvReader.Read(nodeIterator);

            Assert.AreEqual(b.ToString(), result.First().B.Value.ToString());
        }

        [Test]
        public void GivenNavigatorWhenReadThenDeviceElementIdRefIsMapped()
        {
            const string c = "bob";
            var navigator = CreateNavigator("C", c);
            var nodeIterator = navigator.SelectChildren(XPathNodeType.All);
            var result = _dlvReader.Read(nodeIterator);

            Assert.AreEqual(c, result.First().C);
        }

        [Test]
        public void GivenNavigatorWhenReadThenDataLogPGNIsMapped()
        {
            const ulong d = 298632;
            var navigator = CreateNavigator("D", d.ToString());
            var nodeIterator = navigator.SelectChildren(XPathNodeType.All);
            var result = _dlvReader.Read(nodeIterator);

            Assert.AreEqual(d.ToString(), result.First().D.ToString());
        }

        [Test]
        public void GivenNavigatorWhenReadThenDataLogPGNStartBitIsMapped()
        {
            const byte e = 4;
            var navigator = CreateNavigator("E", e.ToString());
            var nodeIterator = navigator.SelectChildren(XPathNodeType.All);
            var result = _dlvReader.Read(nodeIterator);

            Assert.AreEqual(e.ToString(), result.First().E.Value.ToString());
        }

        [Test]
        public void GivenNavigatorWhenReadThenDataLogPGNStopBitIsMapped()
        {
            const byte f = 6;
            var navigator = CreateNavigator("F", f.ToString());
            var nodeIterator = navigator.SelectChildren(XPathNodeType.All);
            var result = _dlvReader.Read(nodeIterator);

            Assert.AreEqual(f.ToString(), result.First().F.Value.ToString());
        }

        [Test]
        public void GivenNullNavigatorWhenReadThenIsNull()
        {
            var result = _dlvReader.Read(null);
            Assert.IsNull(result);
        }

        private XPathNavigator CreateNavigator(string attributeName, string attributeValue)
        {
            var memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("DLV");
                xmlWriter.WriteAttributeString(attributeName, attributeValue);
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            memStream.Position = 0;
            var xpathDoc = new XPathDocument(memStream);
            return xpathDoc.CreateNavigator();
        }
    }
}
