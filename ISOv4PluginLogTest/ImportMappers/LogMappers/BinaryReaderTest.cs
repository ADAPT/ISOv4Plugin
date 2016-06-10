using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;
using BinaryReader = AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.BinaryReader;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class BinaryReaderTest
    {
        private byte _numberOfDlvs;
        private TIM _tim;
        private PTN _ptn;
        private BinaryReader _binaryReader;
        private string _dataPath;
        private string _fileName;
        private List<byte> _bytes;

        [SetUp]
        public void Setup()
        {
            _numberOfDlvs = 0;
            _bytes = new List<byte>();
            _ptn = new PTN
            {
                A = 94.1234,
                ASpecified = true,
                B = 49.4321,
                BSpecified = true,
                C = 5,
                CSpecified = true,
                D = 1,
                DSpecified = true,
                E = 33,
                ESpecified = true,
                F = 44,
                FSpecified = true,
                G = 13,
                GSpecified = true,
                H = 123123213,
                HSpecified = true,
                I = 13213,
                ISpecified = true
            };
            _tim = new TIM
            {
                A = DateTime.Today,
                ASpecified = true,
                Items = new List<IWriter>
                {
                    _ptn,
                    new DLV
                    {
                        A = "0075",
                        B = 32,
                        C = "DEC-1",
                        D = null,
                        E = null,
                        F = null
                    }
                }.ToArray()
            };

            _binaryReader = new BinaryReader();
        }

        [Test]
        public void GivenTimHeaderWithEmptyStartTimeWhenReadThenTimeStartIsReadFromFile()
        {
            _tim.A = null;
            
            const int milliseconds = 12321;
            const short daysFrom1980 = 4264;
            

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
            _tim.A = DateTime.Now;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(Convert.ToDateTime(_tim.A.Value), result.TimeStart);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionNorthWhenReadThenPositionNorthIsZero()
        {
            _ptn.ASpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(0, result.NorthPosition);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionNorthWhenReadThenPositionNorthIsReadFromFile()
        {
            _ptn.ASpecified = true;
            _ptn.A = null;
            const int positionNorth = 12321;
            _bytes.AddRange(BitConverter.GetBytes(positionNorth));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();
            
            Assert.AreEqual(positionNorth, result.NorthPosition);
        }

        [Test]
        public void GivenTimHeaderWithPositionNorthHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.ASpecified = true;
            const int positionNorthValue = 98765;
            _ptn.A = positionNorthValue;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionNorthValue, result.NorthPosition);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionEastWhenReadThenPositionEastIsZero()
        {
            _ptn.BSpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(0, result.EastPosition);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionEastWhenReadThenPositionEastIsReadFromFile()
        {
            _ptn.BSpecified = true;
            _ptn.B = null;
            const int positionEast = 12321;
            _bytes.AddRange(BitConverter.GetBytes(positionEast));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionEast, result.EastPosition);
        }

        [Test]
        public void GivenTimHeaderWithPositionEastHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.BSpecified = true;
            _ptn.B = 1564;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.B.Value, result.EastPosition);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionUpWhenReadThenPositionUpIsNull()
        {
            _ptn.CSpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.Elevation);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionUpWhenReadThenPositionUpIsReadFromFile()
        {
            _ptn.CSpecified = true;
            _ptn.C = null;
            const int positionUp = 846;
            _bytes.AddRange(BitConverter.GetBytes(positionUp));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionUp, result.Elevation);
        }

        [Test]
        public void GivenTimHeaderWithPositionUpHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.CSpecified = true;
            _ptn.C = 9515;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.C.Value, result.Elevation);
        }

        [Test]
        public void GivenTimHeaderWithNullPositionStatusWhenReadThenPositionStatusIsNull()
        {
            _ptn.DSpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.PositionStatus);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPositionStatusWhenReadThenPositionStatusIsReadFromFile()
        {
            _ptn.DSpecified = true;
            _ptn.D = null;
            const byte positionStatus = 48;
            _bytes.AddRange(BitConverter.GetBytes(positionStatus));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(positionStatus, result.PositionStatus);
        }

        [Test]
        public void GivenTimHeaderWithPositionStatusHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.DSpecified = true;
            _ptn.D = (byte) 53;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.D.Value, result.PositionStatus);
        }

        [Test]
        public void GivenTimHeaderWithNullPdopWhenReadThenPdopIsNull()
        {
            _ptn.ESpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.PDOP);
        }

        [Test]
        public void GivenTimHeaderWithEmptyPdopWhenReadThenPdopIsReadFromFile()
        {
            _ptn.ESpecified = true;
            _ptn.E = null;
            const short pdop = 6453;
            _bytes.AddRange(BitConverter.GetBytes(pdop));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(pdop, result.PDOP);
        }

        [Test]
        public void GivenTimHeaderWithPdopHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.ESpecified = true;
            _ptn.E = (short) 2835;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.E.Value, result.PDOP);
        }

        [Test]
        public void GivenTimHeaderWithNullHdopWhenReadThenHdopIsNull()
        {
            _ptn.FSpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.HDOP);
        }

        [Test]
        public void GivenTimHeaderWithEmptyHdopWhenReadThenHdopIsReadFromFile()
        {
            _ptn.FSpecified = true;
            _ptn.F = null;
            const short hdop = 23235;
            _bytes.AddRange(BitConverter.GetBytes(hdop));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(hdop, result.HDOP);
        }

        [Test]
        public void GivenTimHeaderWithHdopHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.FSpecified = true;
            _ptn.F = (short) 5473;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.F.Value, result.HDOP);
        }

        [Test]
        public void GivenTimHeaderWithNullNumberOfSatellitesWhenReadThenNumberOfSatellitesIsNull()
        {
            _ptn.GSpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.NumberOfSatellites);
        }

        [Test]
        public void GivenTimHeaderWithEmptyNumberOfSatellitesWhenReadThenNumberOfSatellitesIsReadFromFile()
        {
            _ptn.GSpecified = true;
            _ptn.G = null;
            const byte numberOfSatellites = 31;
            _bytes.AddRange(BitConverter.GetBytes(numberOfSatellites));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(numberOfSatellites, result.NumberOfSatellites);
        }

        [Test]
        public void GivenTimHeaderWithNumberOfSatellitesHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.GSpecified = true;
            _ptn.G = (byte) 121;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.G.Value, result.NumberOfSatellites);
        }

        [Test]
        public void GivenTimHeaderWithNullGpsUtcTimeWhenReadThenIsNull()
        {
            _ptn.HSpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.GpsUtcTime);
        }

        [Test]
        public void GivenTimHeaderWithEmptyGpsUtcTimeWhenReadThenIsReadFromFile()
        {
            _ptn.HSpecified = true;
            _ptn.H = null;
            const int gpsUtcTime = 45678;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcTime));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(gpsUtcTime, result.GpsUtcTime);
        }

        [Test]
        public void GivenTimHeaderWithGpsUtcTimeHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.HSpecified = true;
            _ptn.H = 45678;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.H.Value, result.GpsUtcTime);
        }

        [Test]
        public void GivenTimHeaderWithNullGpsUtcDateWhenReadThenIsNull()
        {
            _ptn.ISpecified = false;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.IsNull(result.GpsUtcDate);
        }

        [Test]
        public void GivenTimHeaderWithEmptyGpsUtcDateWhenReadThenIsReadFromFile()
        {
            _ptn.ISpecified = true;
            _ptn.I = null;
            const short gpsUtcDate = 9842;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcDate));
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(gpsUtcDate, result.GpsUtcDate);
        }

        [Test]
        public void GivenTimHeaderWithGpsUtcDateHasValueWhenReadThenUseDefaultValue()
        {
            _ptn.ISpecified = true;
            _ptn.I = (ushort) 5132;
            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();

            Assert.AreEqual(_ptn.I.Value, result.GpsUtcDate);
        }

        [Test]
        public void GivenTimHeaderWithGpsUtcDateAndGpsUtcTimeWhenReadThenGpsUtcDateTimeIsSet()
        {
            const int gpsUtcTime = 7894552;
            _ptn.HSpecified = true;
            _ptn.H = null;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcTime));


            const int gpsUtcDate = 6850;
            _ptn.ISpecified = true;
            _ptn.I = null;
            _bytes.AddRange(BitConverter.GetBytes(gpsUtcDate));

            _bytes.Add(_numberOfDlvs);

            var result = ReadSingle();
            var expectedDate = new DateTime(1980, 01, 01).AddDays(gpsUtcDate).AddMilliseconds(gpsUtcTime);

            Assert.AreEqual(expectedDate, result.GpsUtcDateTime);
        }

        [Test]
        public void GivenTimeHeaderWhenReadThenDlvsIsSet()
        {
            var dlv1 = new DLV {A = "6"};
            var dlv2 = new DLV {A = "7"};
            var dlv3 = new DLV {A = "8"};
            var dlv4 = new DLV {A = "9"};
            var dlv5 = new DLV {A = "10"};

            _tim.Items = new List<DLV> {dlv1, dlv2, dlv3, dlv4, dlv5}.ToArray();

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



            _ptn.ASpecified = true;
            _ptn.A = null;
            _bytes.AddRange(BitConverter.GetBytes(12321));
            _bytes.Add(_numberOfDlvs);


            _bytes.AddRange(BitConverter.GetBytes(54863));
            _bytes.Add(_numberOfDlvs);

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

            var result = _binaryReader.Read(_dataPath, _fileName, _tim);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenNullTimHeaderWhenReadThenReturnedEmpty()
        {
            _tim = null;
            _bytes.Add(_numberOfDlvs);

            var result = Read();

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenBinaryDlvDataWithoutMatchingDlvThenSpatialValueIsNull()
        {
            _tim.Items = new List<IWriter>{_ptn}.ToArray();

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

            var dlv1 = new DLV { A = "0A" };
            var dlv2 = new DLV { A = "0B" };
            
            _tim.Items = new List<DLV> { dlv1, dlv2 }.ToArray();

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
            

            return _binaryReader.Read(_dataPath, _fileName, _tim);
        }

        [TearDown]
        public void TearDown()
        {
            if(File.Exists(Path.Combine(_dataPath, _fileName)))
                File.Delete(Path.Combine(_dataPath, _fileName));
        }

    }
}
