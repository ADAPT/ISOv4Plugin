using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Readers;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Readers
{
    [TestFixture]
    public class GrdReaderTest
    {
        private GrdReader _grdReader;
        private XPathNodeIterator _children;
        private XPathNodeIterator _iterator;

        [SetUp]
        public void Setup()
        {
            _grdReader = new GrdReader();
        }

        [Test]
        public void GivenIteratorWithEmptyCollectionWhenReadThenEmpty()
        {
            var grd1 = new GRD
            {
                A = 123,
                B = 234,
                C = 345,
                D = 456,
                E = 567,
                F = 678,
                G = "789",
                H = 890,
                I = 3,
                J = 4
            };

            var grds = new List<GRD> { grd1 };
            var grdElements = CreateNavigator(grds);
            _iterator = grdElements.Select("./TSK/NOTGRD");

            var result = _grdReader.Read(_iterator);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenIteratorWhenReadThenGrdAreRead()
        {
            var grd1 = new GRD();
            var grd2 = new GRD();

            var grds = new List<GRD> {grd1, grd2};
            var grdElements = CreateNavigator(grds);

            _iterator = grdElements.Select("./TSK/GRD");

            var result = _grdReader.Read(_iterator);

            Assert.AreEqual(grds.Count, result.Count());
        }

        [Test]
        public void GivenIteratorWhenReadThenElementsAreRead()
        {
            var grd1 = new GRD
            {
                A = 123,
                B = 234,
                C = 345,
                D = 456,
                E = 567,
                F = 678,
                G = "789",
                H = 890,
                I = 3,
                J = 4
            };

            var grds = new List<GRD> { grd1 };
            var grdElements = CreateNavigator(grds);

            _iterator = grdElements.Select("./TSK/GRD");

            var result = _grdReader.Read(_iterator).First();

            Assert.AreEqual(grd1.A, result.A);
            Assert.AreEqual(grd1.B, result.B);
            Assert.AreEqual(grd1.C, result.C);
            Assert.AreEqual(grd1.D, result.D);
            Assert.AreEqual(grd1.E, result.E);
            Assert.AreEqual(grd1.F, result.F);
            Assert.AreEqual(grd1.G, result.G);
            Assert.AreEqual(grd1.H, result.H);
            Assert.AreEqual(grd1.I, result.I);
            Assert.AreEqual(grd1.J, result.J);
        }

        [Test]
        public void GivenIteratorWithNoHWhenReadThenHIsNull()
        {
            var grd1 = new GRD();
            var grds = new List<GRD> { grd1 };
            var grdElements = CreateNavigator(grds);

            _iterator = grdElements.Select("./TSK/GRD");

            var result = _grdReader.Read(_iterator).First();

            Assert.IsNull(result.H);
        }

        [Test]
        public void GivenIteratorWithNoJWhenReadThenJIsNull()
        {
            var grd1 = new GRD();
            var grds = new List<GRD> { grd1 };
            var grdElements = CreateNavigator(grds);

            _iterator = grdElements.Select("./TSK/GRD");

            var result = _grdReader.Read(_iterator).First();

            Assert.IsNull(result.J);
        }


        private XPathNavigator CreateNavigator(List<GRD> grds)
        {
            var memStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("TSK");

                foreach (var grd in grds)
                {
                    xmlWriter.WriteStartElement("GRD");    

                    xmlWriter.WriteAttributeString("A", grd.A.ToString());
                    xmlWriter.WriteAttributeString("B", grd.B.ToString());
                    xmlWriter.WriteAttributeString("C", grd.C.ToString());
                    xmlWriter.WriteAttributeString("D", grd.D.ToString());
                    xmlWriter.WriteAttributeString("E", grd.E.ToString());
                    xmlWriter.WriteAttributeString("F", grd.F.ToString());
                    xmlWriter.WriteAttributeString("G", grd.G);
                    xmlWriter.WriteAttributeString("H", grd.H.ToString());
                    xmlWriter.WriteAttributeString("I", grd.I.ToString());
                    xmlWriter.WriteAttributeString("J", grd.J.ToString());

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.Flush();
                xmlWriter.Close();
            }

            memStream.Position = 0;
            var xpathDoc = new XPathDocument(memStream);
            return xpathDoc.CreateNavigator();
        
        }
    }
}
