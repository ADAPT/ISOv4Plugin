using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;
using BinaryReader = AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.BinaryReader;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class BinaryReaderTest
    {
        private byte _numberOfDlvs;
        private TIMHeader _timHeader;
        private BinaryReader _binaryReader;
        private string _dataPath;
        private string _fileName;
        private List<byte> _bytes;

        [SetUp]
        public void Setup()
        {
            _numberOfDlvs = 0;
            _bytes = new List<byte>();
            _timHeader = new TIMHeader
            {
                Start = new HeaderProperty{State = HeaderPropertyState.HasValue, Value = DateTime.Today},
                PtnHeader = new PTNHeader
                {
                    GpsUtcDate = new HeaderProperty(),
                    GpsUtcTime = new HeaderProperty(),
                    HDOP = new HeaderProperty(),
                    NumberOfSatellites = new HeaderProperty(),
                    PDOP = new HeaderProperty(),
                    PositionEast = new HeaderProperty(),
                    PositionNorth = new HeaderProperty(),
                    PositionStatus = new HeaderProperty(),
                    PositionUp = new HeaderProperty(),
                },
                DLVs = new List<DLVHeader>()
            };

            _binaryReader = new BinaryReader();
        }

        [Test]
        public void GivenTimHeaderWithEmptyStartTimeWhenReadThenTimeStartIsReadFromFile()
        {
            _timHeader.Start.State = HeaderPropertyState.IsEmpty;
            
            const int milliseconds = 12321;
            const short daysFrom1980 = 4264;
            _timHeader.Start.Value = milliseconds;

            _bytes.AddRange(BitConverter.GetBytes(milliseconds));
            _bytes.AddRange(BitConverter.GetBytes(daysFrom1980));

            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();
            var expectedDateTime = new DateTime(1980, 01, 01).AddDays(daysFrom1980).AddMilliseconds(milliseconds);

            Assert.AreEqual(expectedDateTime, result.TimeStart);
        }

        [Test]
        public void GivenTimHeaderWithStartTimeWithValueWhenReadThenUseDefaultValue()
        {
            _timHeader.Start.State = HeaderPropertyState.HasValue;
            _timHeader.Start.Value = DateTime.Now;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(Convert.ToDateTime(_timHeader.Start.Value), result.TimeStart);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionNorthWhenReadThenPositionNorthIsZero()
        {
            _timHeader.PtnHeader.PositionNorth.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(0, result.NorthPosition);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionNorthWhenReadThenPositionNorthIsReadFromFile()
        {
            _timHeader.PtnHeader.PositionNorth.State = HeaderPropertyState.IsEmpty;
            const int positionNorth = 12321;
            _bytes.AddRange(BitConverter.GetBytes(positionNorth));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();
            
            Assert.AreEqual(positionNorth, result.NorthPosition);
        }

        [Test]
        public void GivenTimHeaderWithPositionNorthHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.PositionNorth.State = HeaderPropertyState.HasValue;
            const int positionNorthValue = 98765;
            _timHeader.PtnHeader.PositionNorth.Value = positionNorthValue;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionNorthValue, result.NorthPosition);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionEastWhenReadThenPositionEastIsZero()
        {
            _timHeader.PtnHeader.PositionEast.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(0, result.EastPosition);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionEastWhenReadThenPositionEastIsReadFromFile()
        {
            _timHeader.PtnHeader.PositionEast.State = HeaderPropertyState.IsEmpty;
            const int positionEast = 12321;
            _bytes.AddRange(BitConverter.GetBytes(positionEast));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionEast, result.EastPosition);
        }

        [Test]
        public void GivenTimHeaderWithPositionEastHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.PositionEast.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.PositionEast.Value = 1564;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.PositionEast.Value, result.EastPosition);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionUpWhenReadThenPositionUpIsNull()
        {
            _timHeader.PtnHeader.PositionUp.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.Elevation);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionUpWhenReadThenPositionUpIsReadFromFile()
        {
            _timHeader.PtnHeader.PositionUp.State = HeaderPropertyState.IsEmpty;
            const int positionUp = 846;
            _bytes.AddRange(BitConverter.GetBytes(positionUp));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionUp, result.Elevation);
        }

        [Test]
        public void GivenTimHeaderWithPositionUpHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.PositionUp.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.PositionUp.Value = 9515;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.PositionUp.Value, result.Elevation);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionStatusWhenReadThenPositionStatusIsNull()
        {
            _timHeader.PtnHeader.PositionStatus.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.PositionStatus);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionStatusWhenReadThenPositionStatusIsReadFromFile()
        {
            _timHeader.PtnHeader.PositionStatus.State = HeaderPropertyState.IsEmpty;
            const byte positionStatus = 48;
            _bytes.AddRange(BitConverter.GetBytes(positionStatus));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionStatus, result.PositionStatus);
        }

        [Test]
        public void GivenTimHeaderWithPositionStatusHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.PositionStatus.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.PositionStatus.Value = (byte)53;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.PositionStatus.Value, result.PositionStatus);
        }

        [Test]
        public void GivenTimHeaderWithNullPdopWhenReadThenPdopIsNull()
        {
            _timHeader.PtnHeader.PDOP.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.PDOP);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPdopWhenReadThenPdopIsReadFromFile()
        {
            _timHeader.PtnHeader.PDOP.State = HeaderPropertyState.IsEmpty;
            const short pdop = 6453;
            _bytes.AddRange(BitConverter.GetBytes(pdop));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(pdop, result.PDOP);
        }

        [Test]
        public void GivenTimHeaderWithPdopHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.PDOP.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.PDOP.Value = (short)2835;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.PDOP.Value, result.PDOP);
        }

        [Test]
        public void GivenTimHeaderWithNullHdopWhenReadThenHdopIsNull()
        {
            _timHeader.PtnHeader.HDOP.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.HDOP);
        }

        [Test]
        public void GivenTimHeaderWithEmptyHdopWhenReadThenHdopIsReadFromFile()
        {
            _timHeader.PtnHeader.HDOP.State = HeaderPropertyState.IsEmpty;
            const short hdop = 23235;
            _bytes.AddRange(BitConverter.GetBytes(hdop));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(hdop, result.HDOP);
        }

        [Test]
        public void GivenTimHeaderWithHdopHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.HDOP.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.HDOP.Value = (short)5473;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.HDOP.Value, result.HDOP);
        }

        [Test]
        public void GivenTimHeaderWithNullNumberOfSatellitesWhenReadThenNumberOfSatellitesIsNull()
        {
            _timHeader.PtnHeader.NumberOfSatellites.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.NumberOfSatellites);
        }

        [Test]
        public void GivenTimHeaderWithEmptyNumberOfSatellitesWhenReadThenNumberOfSatellitesIsReadFromFile()
        {
            _timHeader.PtnHeader.NumberOfSatellites.State = HeaderPropertyState.IsEmpty;
            const byte numberOfSatellites = 31;
            _bytes.AddRange(BitConverter.GetBytes(numberOfSatellites));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(numberOfSatellites, result.NumberOfSatellites);
        }

        [Test]
        public void GivenTimHeaderWithNumberOfSatellitesHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.NumberOfSatellites.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.NumberOfSatellites.Value = (byte)121;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.NumberOfSatellites.Value, result.NumberOfSatellites);
        }

        [Test]
        public void GivenTimHeaderWithNullGpsUtcTimeWhenReadThenIsNull()
        {
            _timHeader.PtnHeader.GpsUtcTime.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.GpsUtcTime);
        }

        [Test]
        public void GivenTimHeaderWithEmptyGpsUtcTimeWhenReadThenIsReadFromFile()
        {
            _timHeader.PtnHeader.GpsUtcTime.State = HeaderPropertyState.IsEmpty;
            const int gpsUtcTime = 45678;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcTime));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(gpsUtcTime, result.GpsUtcTime);
        }

        [Test]
        public void GivenTimHeaderWithGpsUtcTimeHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.GpsUtcTime.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.GpsUtcTime.Value = 45678;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.GpsUtcTime.Value, result.GpsUtcTime);
        }

        [Test]
        public void GivenTimHeaderWithNullGpsUtcDateWhenReadThenIsNull()
        {
            _timHeader.PtnHeader.GpsUtcDate.State = HeaderPropertyState.IsNull;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.GpsUtcDate);
        }

        [Test]
        public void GivenTimHeaderWithEmptyGpsUtcDateWhenReadThenIsReadFromFile()
        {
            _timHeader.PtnHeader.GpsUtcDate.State = HeaderPropertyState.IsEmpty;
            const short gpsUtcDate = 9842;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcDate));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(gpsUtcDate, result.GpsUtcDate);
        }

        [Test]
        public void GivenTimHeaderWithGpsUtcDateHasValueWhenReadThenUseDefaultValue()
        {
            _timHeader.PtnHeader.GpsUtcDate.State = HeaderPropertyState.HasValue;
            _timHeader.PtnHeader.GpsUtcDate.Value = (short)5132;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_timHeader.PtnHeader.GpsUtcDate.Value, result.GpsUtcDate);
        }

        [Test]
        public void GivenTimHeaderWithGpsUtcDateAndGpsUtcTimeWhenReadThenGpsUtcDateTimeIsSet()
        {
            _timHeader.PtnHeader.GpsUtcTime.State = HeaderPropertyState.IsEmpty;
            const int gpsUtcTime = 7894552;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcTime));

            _timHeader.PtnHeader.GpsUtcDate.State = HeaderPropertyState.IsEmpty;
            const int gpsUtcDate = 6850;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcDate));

            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();
            var expectedDate = new DateTime(1980, 01, 01).AddDays(gpsUtcDate).AddMilliseconds(gpsUtcTime);

            Assert.AreEqual(expectedDate, result.GpsUtcDateTime);
        }

        [Test]
        public void GivenTimeHeaderWhenReadThenDlvsIsSet()
        {

            _timHeader.DLVs.Add(new DLVHeader{ ProcessDataDDI = new HeaderProperty {Value =  6} });
            _timHeader.DLVs.Add(new DLVHeader { ProcessDataDDI = new HeaderProperty { Value = 7 } });
            _timHeader.DLVs.Add(new DLVHeader { ProcessDataDDI = new HeaderProperty { Value = 8 } });
            _timHeader.DLVs.Add(new DLVHeader { ProcessDataDDI = new HeaderProperty { Value = 9 } });
            _timHeader.DLVs.Add(new DLVHeader { ProcessDataDDI = new HeaderProperty { Value = 10 } });

            _numberOfDlvs = 5;
            _bytes.Add(_numberOfDlvs);

            var valueList = new List<int>();

            for (int i = 0; i < _numberOfDlvs; i++)
            {
                var order = (byte)i;
                var value = i + 123;

                valueList.Add(value);

                _bytes.Add(order);
                _bytes.AddRange(BitConverter.GetBytes(value));
            }

            var result = ReadSingle();

            Assert.AreEqual(_numberOfDlvs, result.SpatialValues.Count);
            for (int i = 0; i < _numberOfDlvs; i++)
            {
                Assert.AreEqual(valueList[i], result.SpatialValues[i].Value);
            }
        }

        [Test]
        public void GivenBinaryWithMultipleRecordsWhenReadThenMultipleRecordsReturned()
        {
            _timHeader.PtnHeader.PositionNorth.State = HeaderPropertyState.IsEmpty;
            _bytes.AddRange(BitConverter.GetBytes(12321));
            _bytes.Add(_numberOfDlvs);

            _timHeader.PtnHeader.PositionNorth.State = HeaderPropertyState.IsEmpty;
            _bytes.AddRange(BitConverter.GetBytes(54863));
            _bytes.Add(_numberOfDlvs);

            _timHeader.PtnHeader.PositionNorth.State = HeaderPropertyState.IsEmpty;
            _bytes.AddRange(BitConverter.GetBytes(875));
            _bytes.Add(_numberOfDlvs);

            var result = Read().ToList();

            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void GivenDataPathAndFileNameThatDoesntExistWhenReadThenReturnedEmpty()
        {
            _dataPath = "blarg";
            _fileName = "false";

            var result = _binaryReader.Read(_dataPath, _fileName, _timHeader);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenNullTimHeaderWhenReadThenReturnedEmpty()
        {
            _timHeader = null;
            _bytes.Add(_numberOfDlvs);

            var result = Read();

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenBinaryDlvDataWithoutMatchingDlvThenSpatialValueIsNull()
        {
            _numberOfDlvs = 1;
            _bytes.Add(_numberOfDlvs);

            var valueList = new List<int>();

            for (int i = 0; i < _numberOfDlvs; i++)
            {
                var order = (byte)i;
                var value = i + 123;

                valueList.Add(value);

                _bytes.Add(order);
                _bytes.AddRange(BitConverter.GetBytes(value));
            }

            var result = ReadSingle();

            Assert.IsNull(result.SpatialValues[0]);
        }

        [Test]
        public void GivenBinaryWhenReadThenResolutionIsApplied()
        {
            _timHeader.DLVs.Add(new DLVHeader { ProcessDataDDI = new HeaderProperty { Value = 10 } });
            _timHeader.DLVs.Add(new DLVHeader { ProcessDataDDI = new HeaderProperty { Value = 11 } });

            _numberOfDlvs = 2;
            _bytes.Add(_numberOfDlvs);

            const int value = 20000;

            _bytes.Add(0);
            _bytes.AddRange(BitConverter.GetBytes(value));
            _bytes.Add(1);
            _bytes.AddRange(BitConverter.GetBytes(value));
            
            var result = ReadSingle();

            Assert.AreEqual(_numberOfDlvs, result.SpatialValues.Count);
            Assert.AreEqual(20000, result.SpatialValues[0].Value);
            Assert.AreEqual(20, result.SpatialValues[1].Value);
        }

        private ISOSpatialRow ReadSingle()
        {
            return Read().First();
        }

        private IEnumerable<ISOSpatialRow> Read()
        {
            _dataPath = Path.GetTempPath();
            _fileName = "text.bin";

            var writestream = new FileStream(Path.Combine(_dataPath, _fileName), FileMode.Create);
            var binaryWriter = new BinaryWriter(writestream);

            binaryWriter.Write(_bytes.ToArray());

            binaryWriter.Close();
            

            return _binaryReader.Read(_dataPath, _fileName, _timHeader);
        }

        [TearDown]
        public void TearDown()
        {
            if(File.Exists(Path.Combine(_dataPath, _fileName)))
                File.Delete(Path.Combine(_dataPath, _fileName));
        }

    }
}
