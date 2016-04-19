using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
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
            _dataPath = Path.GetTempPath();
            _fileName = "test.xml";

            _timReaderMock = new Mock<ITimReader>();
            _taskDataReaderMock = new Mock<ITaskDataReader>();
            _xmlReader = new XmlReader(_timReaderMock.Object, _taskDataReaderMock.Object);
        }

        [Test]
        public void GivenDataPathAndFileNameWhenReadThenTaskDataReturned()
        {
            _xml = "<ISO11783_TaskData></ISO11783_TaskData>";
            File.AppendAllText(Path.Combine(_dataPath, _fileName), _xml);

            var taskData = new ISO11783_TaskData();
            _taskDataReaderMock.Setup(x => x.Read(It.IsAny<XPathNavigator>())).Returns(taskData);

            var result = _xmlReader.Read(_dataPath, _fileName);
            Assert.AreSame(taskData, result);
        }

        [Test]
        public void GivenDataPathAndFileNameWhenReadTlgXmlDataThenTimReturned()
        {
            _xml = "<TIM></TIM>";
            File.AppendAllText(Path.Combine(_dataPath, _fileName), _xml);

            var timHeader = new TIMHeader();
            _timReaderMock.Setup(x => x.Read(It.IsAny<XDocument>())).Returns(timHeader);

            var result = _xmlReader.ReadTlgXmlData(_dataPath, _fileName);
            Assert.AreSame(timHeader, result);
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
            var timHeader = new TIMHeader();
            _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);

            var expectedPath = Path.Combine(_dataPath, _fileName);
            Assert.IsTrue(File.Exists(expectedPath));
        }

        [Test]
        public void GivenPathAndTimHeaderWithMultipleDlvsWhenWriteTheOneDlvElementPerDlv()
        {
            var dlvHeaders = new List<DLVHeader>
            {
                new DLVHeader(),
                new DLVHeader(),
                new DLVHeader(),
            };
            var timHeader = new TIMHeader
            {
                DLVs = dlvHeaders
            };

            var xdoc =_xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var expectedTim = xdoc.Element("TIM");

            Assert.AreEqual(dlvHeaders.Count, expectedTim.Elements("DLV").Count());
        }

        [Test]
        public void GivenTimHeaderWhenWriteThenDlvHasAttributes()
        {
            var dlvHeader = new DLVHeader
            {
                ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 123 },
                DataLogPGN = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 2 },
                DataLogPGNStartBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
                DataLogPGNStopBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
                DeviceElementIdRef = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = "DLV-1" },
                ProcessDataValue = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 456 },
            };
            var timHeader = new TIMHeader
            {
                DLVs = new List<DLVHeader>
                {
                    dlvHeader
                }
            };
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var actualDlv = xdoc.Element("TIM").Elements("DLV").First();

            var expected = "007B";
            Assert.AreEqual(expected, actualDlv.Attribute("A").Value);
            Assert.AreEqual(dlvHeader.ProcessDataValue.Value.ToString(), actualDlv.Attribute("B").Value);
            Assert.AreEqual(dlvHeader.DeviceElementIdRef.Value, actualDlv.Attribute("C").Value);
            Assert.AreEqual(dlvHeader.DataLogPGN.Value.ToString(), actualDlv.Attribute("D").Value);
            Assert.AreEqual(dlvHeader.DataLogPGNStartBit.Value.ToString(), actualDlv.Attribute("E").Value);
            Assert.AreEqual(dlvHeader.DataLogPGNStopBit.Value.ToString(), actualDlv.Attribute("F").Value);
        }

        [Test]
        public void GivenNullProcessDataValueWhenWriteThenAttributeIsNull()
        {
            var dlvHeader = new DLVHeader
            {
                ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 123 },
                ProcessDataValue = new HeaderProperty { State = HeaderPropertyState.IsNull, Value = null },
                DeviceElementIdRef = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = "DLV-1" },
                DataLogPGN = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 2 },
                DataLogPGNStartBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
                DataLogPGNStopBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
            };
            var timHeader = new TIMHeader
            {
                DLVs = new List<DLVHeader>
                {
                    dlvHeader
                }
            };
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("B"));
        }

        [Test]
        public void GivenNullDataLogPGNWhenWriteThenDIsNull()
        {
            var dlvHeader = new DLVHeader
            {
                ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 123 },
                ProcessDataValue = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 5 },
                DeviceElementIdRef = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = "DLV-1" },
                DataLogPGN = new HeaderProperty { State = HeaderPropertyState.IsNull, Value = null},
                DataLogPGNStartBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
                DataLogPGNStopBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
            };
            var timHeader = new TIMHeader
            {
                DLVs = new List<DLVHeader>
                {
                    dlvHeader
                }
            };
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("D"));
        }

        [Test]
        public void GivenNullDataLogStartBitWhenWriteThenEIsNull()
        {
            var dlvHeader = new DLVHeader
            {
                ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 123 },
                ProcessDataValue = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 5},
                DeviceElementIdRef = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = "DLV-1" },
                DataLogPGN = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 5 },
                DataLogPGNStartBit = new HeaderProperty { State = HeaderPropertyState.IsNull, Value = null },
                DataLogPGNStopBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
            };
            var timHeader = new TIMHeader
            {
                DLVs = new List<DLVHeader>
                {
                    dlvHeader
                }
            };
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("E"));
        }

        [Test]
        public void GivenNullDataLogStartBitWhenWriteThenFIsNull()
        {
            var dlvHeader = new DLVHeader
            {
                ProcessDataDDI = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 123 },
                ProcessDataValue = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 5 },
                DeviceElementIdRef = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = "DLV-1" },
                DataLogPGN = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 5 },
                DataLogPGNStartBit = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = new byte() },
                DataLogPGNStopBit = new HeaderProperty { State = HeaderPropertyState.IsNull, Value = null },
            };
            var timHeader = new TIMHeader
            {
                DLVs = new List<DLVHeader>
                {
                    dlvHeader
                }
            };
            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var expectedDlv = xdoc.Element("TIM").Elements("DLV").First();

            Assert.IsNull(expectedDlv.Attribute("F"));
        }

        [Test]
        public void GivenTimHeaderWhenWriteThenPtnHasAtributes()
        {
            var ptnHeader = new PTNHeader
            {
                GpsUtcDate = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                GpsUtcTime = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                HDOP = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                NumberOfSatellites = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                PDOP = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                PositionEast = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                PositionNorth = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                PositionStatus = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                PositionUp = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
            };
            var timHeader = new TIMHeader
            {
                PtnHeader = ptnHeader
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
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
            var ptnHeader = new PTNHeader
            {
                PositionNorth = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = -123 },
                PositionEast = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 145},
                PositionUp = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 123415 },
                PositionStatus = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 7 },
                PDOP = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 98.3 },
                HDOP = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 33.2 },
                NumberOfSatellites = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 231 },
                GpsUtcTime = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 21344 },
                GpsUtcDate = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 231 },
            };
            var timHeader = new TIMHeader
            {
                PtnHeader = ptnHeader
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var actualPtn = xdoc.Element("TIM").Element("PTN");

            Assert.AreEqual(ptnHeader.PositionNorth.Value.ToString(), actualPtn.Attribute("A").Value);
            Assert.AreEqual(ptnHeader.PositionEast.Value.ToString(), actualPtn.Attribute("B").Value);
            Assert.AreEqual(ptnHeader.PositionUp.Value.ToString(), actualPtn.Attribute("C").Value);
            Assert.AreEqual(ptnHeader.PositionStatus.Value.ToString(), actualPtn.Attribute("D").Value);
            Assert.AreEqual(ptnHeader.PDOP.Value.ToString(), actualPtn.Attribute("E").Value);
            Assert.AreEqual(ptnHeader.HDOP.Value.ToString(), actualPtn.Attribute("F").Value);
            Assert.AreEqual(ptnHeader.NumberOfSatellites.Value.ToString(), actualPtn.Attribute("G").Value);
            Assert.AreEqual(ptnHeader.GpsUtcTime.Value.ToString(), actualPtn.Attribute("H").Value);
            Assert.AreEqual(ptnHeader.GpsUtcDate.Value.ToString(), actualPtn.Attribute("I").Value);
        }

        [Test]
        public void GivenTimHeaderWithIsNullStateWhenWriteThenPtnDoesntAddAtributes()
        {
            var ptnHeader = new PTNHeader
            {
                GpsUtcDate = new HeaderProperty { State = HeaderPropertyState.IsNull },
                GpsUtcTime = new HeaderProperty { State = HeaderPropertyState.IsNull },
                HDOP = new HeaderProperty { State = HeaderPropertyState.IsNull },
                NumberOfSatellites = new HeaderProperty { State = HeaderPropertyState.IsNull },
                PDOP = new HeaderProperty { State = HeaderPropertyState.IsNull },
                PositionEast = new HeaderProperty { State = HeaderPropertyState.IsNull },
                PositionNorth = new HeaderProperty { State = HeaderPropertyState.IsNull },
                PositionStatus = new HeaderProperty { State = HeaderPropertyState.IsNull },
                PositionUp = new HeaderProperty { State = HeaderPropertyState.IsNull },
            };
            var timHeader = new TIMHeader
            {
                PtnHeader = ptnHeader
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
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
            var timHeader = new TIMHeader
            {
                Start = new HeaderProperty { State = HeaderPropertyState.IsEmpty },
                Stop = new HeaderProperty { State = HeaderPropertyState.IsEmpty },
                Duration = new HeaderProperty { State = HeaderPropertyState.IsEmpty },
                Type = new HeaderProperty { State = HeaderPropertyState.IsEmpty },
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var actualTim = xdoc.Element("TIM");

            Assert.AreEqual("", actualTim.Attribute("A").Value);
            Assert.AreEqual("", actualTim.Attribute("B").Value);
            Assert.AreEqual("", actualTim.Attribute("C").Value);
            Assert.AreEqual("", actualTim.Attribute("D").Value);
        }

        [Test]
        public void GivenTimHeaderWithIsNullStatesWhenMapThenTimAttributesAreNull()
        {
            var timHeader = new TIMHeader
            {
                Start = new HeaderProperty { State = HeaderPropertyState.IsNull },
                Stop = new HeaderProperty { State = HeaderPropertyState.IsNull },
                Duration = new HeaderProperty { State = HeaderPropertyState.IsNull },
                Type = new HeaderProperty { State = HeaderPropertyState.IsNull },
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var actualTim = xdoc.Element("TIM");

            Assert.IsNull(actualTim.Attribute("A"));
            Assert.IsNull(actualTim.Attribute("B"));
            Assert.IsNull(actualTim.Attribute("C"));
            Assert.IsNull(actualTim.Attribute("D"));
        }

        [Test]
        public void GivenTimHeaderWithIsNullStatesWhenMapThenTimAttributeHasValues()
        {
            var timHeader = new TIMHeader
            {
                Start = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = DateTime.Now},
                Stop = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = DateTime.Now.AddMinutes(15) },
                Duration = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = 12354127851 },
                Type = new HeaderProperty { State = HeaderPropertyState.HasValue, Value = TIMD.Item5 },
            };

            var xdoc = _xmlReader.WriteTlgXmlData(_dataPath, _fileName, timHeader);
            var actualTim = xdoc.Element("TIM");

            Assert.AreEqual(timHeader.Start.Value.ToString(), actualTim.Attribute("A").Value);
            Assert.AreEqual(timHeader.Stop.Value.ToString(), actualTim.Attribute("B").Value);
            Assert.AreEqual(timHeader.Duration.Value.ToString(), actualTim.Attribute("C").Value);
            Assert.AreEqual(((int)((TIMD)timHeader.Type.Value)).ToString(), actualTim.Attribute("D").Value);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(Path.Combine(_dataPath, _fileName));
        }
    }
}
