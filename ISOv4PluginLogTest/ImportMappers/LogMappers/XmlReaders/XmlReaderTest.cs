using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Readers;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers.XmlReaders
{
    [TestFixture]
    public class XmlReaderTest
    {
        private string _fileName;
        private string _xml;
        private string _dataPath;
        private Mock<ITimReader> _timReaderMock;
        private Mock<ITaskDataReader> _taskDataReaderMock;
        private XmlReader _xmlReader;

        [SetUp]
        public void Setup()
        {
            _dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(_dataPath, "TASKDATA"));
            _fileName = "test.xml";

            _timReaderMock = new Mock<ITimReader>();
            _taskDataReaderMock = new Mock<ITaskDataReader>();
            _xmlReader = new XmlReader(_timReaderMock.Object, _taskDataReaderMock.Object);
        }

        [Test]
        public void GivenDataPathAndFileNameWhenReadTlgXmlDataThenTimReturned()
        {
            _xml = "<TIM></TIM>";
            File.AppendAllText(Path.Combine(_dataPath, "TASKDATA", _fileName), _xml);

            var tim = new TIM();
            _timReaderMock.Setup(x => x.Read(It.IsAny<XPathDocument>())).Returns(new List<TIM>{tim});

            var filePath = Path.Combine(_dataPath, "TASKDATA");
            var result = _xmlReader.ReadTlgXmlData(filePath, _fileName);
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(tim, result[0]);
        }

        [Test]
        public void GivenXattributeOfDoubleWhenGetValueThenDoubleReturned()
        {
            var attribute = new XAttribute("something", "2.36548");

            var result = XmlReader.GetValue<double>(attribute);

            Assert.AreEqual(2.36548, result);
        }

        [Test]
        public void GivenEmptyXattributeOfDoubleWhenGetValueThenDefaultDoubleReturned()
        {
            var attribute = new XAttribute("something", "");

            var result = XmlReader.GetValue<double>(attribute);

            Assert.AreEqual(0.0, result);
        }

        [Test]
        public void GivenXattributeOfULongWhenGetValueThenULongReturned()
        {
            var attribute = new XAttribute("something", "8465");

            var result = XmlReader.GetValue<long>(attribute);

            Assert.AreEqual(8465, result);
        }

        [Test]
        public void GivenEmptyXattributeOfULongWhenGetValueThenDefaultULongReturned()
        {
            var attribute = new XAttribute("something", "");

            var result = XmlReader.GetValue<long>(attribute);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void GivenXattributeOfStringWhenGetValueThenStringReturned()
        {
            var attribute = new XAttribute("something", "doplar");

            var result = XmlReader.GetValue<string>(attribute);

            Assert.AreEqual("doplar", result);
        }

        [Test]
        public void GivenEmptyXattributeOfStringWhenGetValueThenEmptyStringReturned()
        {
            var attribute = new XAttribute("something", "");

            var result = XmlReader.GetValue<string>(attribute);

            Assert.AreEqual("", result);
        }

        [Test]
        public void GivenXattributeOfDateTimeWhenGetValueThenDateTimeReturned()
        {
            var attribute = new XAttribute("something", "02/20/2016");

            var result = XmlReader.GetValue<DateTime>(attribute);

            Assert.AreEqual(new DateTime(2016, 02, 20), result);
        }

        [Test]
        public void GivenEmptyXattributeOfDateTimeWhenGetValueThenDefaultDateTimeReturned()
        {
            var attribute = new XAttribute("something", "");

            var result = XmlReader.GetValue<DateTime>(attribute);

            Assert.AreEqual(DateTime.MinValue, result);
        }

        [Test]
        public void GivenEnumXattributeWhenGetValueThenEnumIsReturned()
        {
            var attribute = new XAttribute("something", "2");

            var result = XmlReader.GetValue<TIMD>(attribute);

            Assert.AreEqual(TIMD.Item2, result);
        }

        [Test]
        public void GivenEmptyTimdEnumXattributeWhenGetValueThenEnumDefaultReturned()
        {
            var attribute = new XAttribute("something", "");

            var result = XmlReader.GetValue<TIMD>(attribute);

            Assert.AreEqual(TIMD.Item1, result);
        }

        [Test]
        public void GivenEmptyPtndEnumXattributeWhenGetValueThenEnumDefaultReturned()
        {
            var attribute = new XAttribute("something", "");

            var result = XmlReader.GetValue<PTND>(attribute);

            Assert.AreEqual(PTND.Item0, result);
        }

        [Test]
        public void GivenNullStringWhenGetValueThenIsNullString()
        {
            var result = XmlReader.GetValue<string>(null);

            Assert.IsNull(result);
        }

        [Test]
        public void GivenPathAndTimHeaderWhenWriteTlgXmlDataThenTlgFileIsCreated()
        {
            var tim = new TIM();
            _xmlReader.WriteTlgXmlData(Path.Combine(_dataPath, "TASKDATA"), _fileName, tim);

            var expectedPath = Path.Combine(_dataPath, "TASKDATA", _fileName);
            Assert.IsTrue(File.Exists(expectedPath));
        }

        [Test]
        public void GivenPathAndTimHeaderWithMultipleDlvsWhenWriteTheOneDlvElementPerDlv()
        {
            var dlvs = new List<DLV>
            {
                new DLV(),
                new DLV(),
                new DLV(),
            };
            var tim = new TIM
            {
                Items = dlvs.ToArray()
            };

            var xdoc =_xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var expectedTim = xdoc.Element("TIM");

            Assert.AreEqual(dlvs.Count, expectedTim.Elements("DLV").Count());
        }

        [Test]
        public void GivenTimHeaderWhenWriteThenDlvHasAttributes()
        {
            var dlv = new DLV
            {
                A = "123",
                B = 456,
                C = "DLV-1",
                D = 2,
                E = new byte(),
                F = new byte()
            };

            var timHeader = new TIM
            {
                Items = new List<DLV> { dlv }.ToArray()
            };
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var actualDlv = xdoc.Element("TIM").Elements("DLV").First();

            var expected = "007B";
            Assert.AreEqual(expected, actualDlv.Attribute("A").Value);
            Assert.AreEqual(dlv.B.Value.ToString(), actualDlv.Attribute("B").Value);
            Assert.AreEqual(dlv.C, actualDlv.Attribute("C").Value);
            Assert.AreEqual(dlv.D.Value.ToString(), actualDlv.Attribute("D").Value);
            Assert.AreEqual(dlv.E.Value.ToString(), actualDlv.Attribute("E").Value);
            Assert.AreEqual(dlv.F.Value.ToString(), actualDlv.Attribute("F").Value);
        }

        [Test]
        public void GivenNullProcessDataValueWhenWriteThenAttributeIsNull()
        {
            var dlv = new DLV
            {
                A = "123",
                B = null,
                C = "DLV-1",
                D = 2,
                E = new byte(),
                F = new byte()
            };

            var tim = new TIM
            {
                Items = new List<DLV> { dlv }.ToArray()
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("B"));
        }

        [Test]
        public void GivenNullDataLogPGNWhenWriteThenDIsNull()
        {
            var dlv = new DLV
            {
                A = "123",
                B = null,
                C = "DLV-1",
                D = null,
                E = new byte(),
                F = new byte()
            };
            var tim = new TIM
            {
                Items = new List<DLV> { dlv }.ToArray()
            };
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("D"));
        }

        [Test]
        public void GivenNullDataLogStartBitWhenWriteThenEIsNull()
        {
            var dlv = new DLV
            {
                A = "123",
                B = null,
                C = "DLV-1",
                D = 5,
                E = null,
                F = new byte()
            };
            var tim = new TIM
            {
                Items = new List<DLV> { dlv }.ToArray()
            };

            
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("E"));
        }

        [Test]
        public void GivenNullDataLogStartBitWhenWriteThenFIsNull()
        {
            var dlv = new DLV
            {
                A = "123",
                B = 5,
                C = "DLV-1",
                D = 5,
                E = new byte(),
                F = null
            };
            var tim = new TIM
            {
                Items = new List<DLV> { dlv }.ToArray()
            };
            
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("F"));
        }

        [Test]
        public void GivenTimHeaderWhenWriteThenPtnHasAtributes()
        {
            var ptn = new PTN
            {
                ASpecified = true,
                A = null,

                BSpecified = true,
                B = null,

                CSpecified = true,
                C = null,

                DSpecified = true,
                D = null,

                ESpecified = true,
                E = null,

                FSpecified = true,
                F = null,

                GSpecified = true,
                G = null,

                HSpecified = true,
                H = null,

                ISpecified = true,
                I = null

            };
            var tim = new TIM
            {
                Items = new List<IWriter> {ptn}.ToArray()
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var actualPtn = xdoc.Element("TIM").Element("PTN");

            Assert.AreEqual("", actualPtn.Attribute("A").Value);
            Assert.AreEqual("", actualPtn.Attribute("B").Value);
            Assert.AreEqual("", actualPtn.Attribute("C").Value);
            Assert.AreEqual("", actualPtn.Attribute("D").Value);
            Assert.AreEqual("", actualPtn.Attribute("E").Value);
            Assert.AreEqual("", actualPtn.Attribute("F").Value);
            Assert.AreEqual("", actualPtn.Attribute("G").Value);
            Assert.AreEqual("", actualPtn.Attribute("H").Value);
            Assert.AreEqual("", actualPtn.Attribute("I").Value);
        }

        [Test]
        public void GivenTimHeaderWhenWriteThenPtnHasValues()
        {
            var ptn = new PTN
            {
                ASpecified = true,
                A = -123,

                BSpecified = true,
                B = 145,

                CSpecified = true,
                C = 123415,

                DSpecified = true,
                D = 7,

                ESpecified = true,
                E = 98.3,

                FSpecified = true,
                F = 33.2,

                GSpecified = true,
                G = 231,

                HSpecified = true,
                H = 21344,

                ISpecified = true,
                I = 231

            };
            var tim = new TIM
            {
                Items = new List<IWriter> { ptn }.ToArray()
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var actualPtn = xdoc.Element("TIM").Element("PTN");

            Assert.AreEqual(ptn.A.Value.ToString(), actualPtn.Attribute("A").Value);
            Assert.AreEqual(ptn.B.Value.ToString(), actualPtn.Attribute("B").Value);
            Assert.AreEqual(ptn.C.Value.ToString(), actualPtn.Attribute("C").Value);
            Assert.AreEqual(ptn.D.Value.ToString(), actualPtn.Attribute("D").Value);
            Assert.AreEqual(ptn.E.Value.ToString(), actualPtn.Attribute("E").Value);
            Assert.AreEqual(ptn.F.Value.ToString(), actualPtn.Attribute("F").Value);
            Assert.AreEqual(ptn.G.Value.ToString(), actualPtn.Attribute("G").Value);
            Assert.AreEqual(ptn.H.Value.ToString(), actualPtn.Attribute("H").Value);
            Assert.AreEqual(ptn.I.Value.ToString(), actualPtn.Attribute("I").Value);
        }

        [Test]
        public void GivenTimHeaderWithIsNullStateWhenWriteThenPtnDoesntAddAtributes()
        {
            var ptn = new PTN
            {
                ASpecified = false,
                A = null,

                BSpecified = false,
                B = null,

                CSpecified = false,
                C = null,

                DSpecified = false,
                D = null,

                ESpecified = false,
                E = null,

                FSpecified = false,
                F = null,

                GSpecified = false,
                G = null,

                HSpecified = false,
                H = null,

                ISpecified = false,
                I = null

            };
            var tim = new TIM
            {
                Items = new List<IWriter> { ptn }.ToArray()
            };

           
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var actualPtn = xdoc.Element("TIM").Element("PTN");

            Assert.IsNull(actualPtn.Attribute("A"));
            Assert.IsNull(actualPtn.Attribute("B"));
            Assert.IsNull(actualPtn.Attribute("C"));
            Assert.IsNull(actualPtn.Attribute("D"));
            Assert.IsNull(actualPtn.Attribute("E"));
            Assert.IsNull(actualPtn.Attribute("F"));
            Assert.IsNull(actualPtn.Attribute("G"));
            Assert.IsNull(actualPtn.Attribute("H"));
            Assert.IsNull(actualPtn.Attribute("I"));
        }

        [Test]
        public void GivenTimHeaderWhenMapThenTimHasAttributes()
        {
            var tim = new TIM
            {
                ASpecified = true,
                A = null,
                BSpecified = true,
                B = null,
                CSpecified = true,
                C = null,
                DSpecified = true,
                D = null
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var actualTim = xdoc.Element("TIM");

            Assert.AreEqual("", actualTim.Attribute("A").Value);
            Assert.AreEqual("", actualTim.Attribute("B").Value);
            Assert.AreEqual("", actualTim.Attribute("C").Value);
            Assert.AreEqual("", actualTim.Attribute("D").Value);
        }

        [Test]
        public void GivenTimHeaderWithIsNullStatesWhenMapThenTimAttributesAreNull()
        {
            var tim = new TIM
            {
                ASpecified = false,
                A = null,
                BSpecified = false,
                B = null,
                CSpecified = false,
                C = null,
                DSpecified = false,
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var actualTim = xdoc.Element("TIM");

            Assert.IsNull(actualTim.Attribute("A"));
            Assert.IsNull(actualTim.Attribute("B"));
            Assert.IsNull(actualTim.Attribute("C"));
            Assert.IsNull(actualTim.Attribute("D"));
        }

        [Test]
        public void GivenTimHeaderWithIsNullStatesWhenMapThenTimAttributeHasValues()
        {
            var tim = new TIM
            {
                ASpecified = true,
                A = DateTime.Now,
                BSpecified = true,
                B = DateTime.Now.AddMinutes(15),
                CSpecified = true,
                C = 12354127851,
                DSpecified = true,
                D = TIMD.Item5
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, tim);
            var actualTim = xdoc.Element("TIM");

            Assert.AreEqual(tim.A.Value.ToString(), actualTim.Attribute("A").Value);
            Assert.AreEqual(tim.B.Value.ToString(), actualTim.Attribute("B").Value);
            Assert.AreEqual(tim.C.Value.ToString(), actualTim.Attribute("C").Value);
            Assert.AreEqual(((int)(tim.D.Value)).ToString(), actualTim.Attribute("D").Value);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(Path.Combine(_dataPath, _fileName));
        }
    }
}
