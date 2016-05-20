using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
        public void GivenNavigatorWithAWhenReadThenPositionNorthIsSet()
        {
            const long positionNorthValue = 12321;
            var navigator = CreateNavigator("A", positionNorthValue.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(positionNorthValue, Convert.ToInt32(result.A.Value));
        }

        [Test]
        public void GivenNavigatorWithBWhenReadThenPositionEastIsSet()
        {
            const long positionEastValue = 436346;
            var navigator = CreateNavigator("B", positionEastValue.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(positionEastValue, Convert.ToInt32(result.B.Value));
        }

        [Test]
        public void GivenNavigatorWithCWhenReadThenPositionUpIsSet()
        {
            const long positionSetValue = 332562;
            var navigator = CreateNavigator("C", positionSetValue.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(positionSetValue, Convert.ToInt32(result.C.Value));
        }

        [Test]
        public void GivenNavigatorWithDWhenReadThenPositionStatusIsSet()
        {
            const byte positionStatusValue = 1;
            var navigator = CreateNavigator("D", positionStatusValue.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(PTND.Item1, (PTND)Convert.ToInt16(result.D.Value));
        }

        [Test]
        public void GivenNavigatorWithEWhenReadThenPdopIsSet()
        {
            const short pdopValue = 2342;
            var navigator = CreateNavigator("E", pdopValue.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(pdopValue, Convert.ToInt16(result.E.Value));
        }

        [Test]
        public void GivenNavigatorWithFWhenReadThenHdopIsSet()
        {
            const short hdopValue = 1232;
            var navigator = CreateNavigator("F", hdopValue.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(hdopValue, Convert.ToInt16(result.F.Value));
        }

        [Test]
        public void GivenXelementWithGWhenReadThenNumberOfSatellitesIsSet()
        {
            const int value = 156;
            var navigator = CreateNavigator("G", value.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(value, Convert.ToInt32(result.G.Value));
        }

        [Test]
        public void GivenNavigatorWithHWhenReadThenGpsUtcTimeIsSet()
        {
            const int value = 15486632;
            var navigator = CreateNavigator("H", value.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);

            Assert.AreEqual(value, Convert.ToInt32(result.H.Value));
        }

        [Test]
        public void GivenNavigatorWithIWhenReadThenGpsUtcDateIsSet()
        {
            const int value = 23456;
            var navigator = CreateNavigator("I", value.ToString(CultureInfo.InvariantCulture));

            var result = _ptnReader.Read(navigator);
            Assert.AreEqual(value, Convert.ToInt32(result.I.Value));
        }

        [Test]
        public void GivenNullNavigatorWhenReadThenIsNull()
        {
            var result = _ptnReader.Read(null as XPathNavigator);
            Assert.IsNull(result);
        }

        private XPathNavigator CreateNavigator(string attributeName, string attributeValue)
        {
            var memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memStream, new XmlWriterSettings{ Encoding = new UTF8Encoding(false)}))
            {
                xmlWriter.WriteStartElement("PTN");
                xmlWriter.WriteAttributeString(attributeName, attributeValue);
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            memStream.Position = 0;
            var xpathDoc = new XPathDocument(memStream);
            return xpathDoc.CreateNavigator().SelectSingleNode("PTN");
        }

    }
}
