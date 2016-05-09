using System;
using System.Collections.Generic;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using Moq;
using NUnit.Framework;
using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = AgGateway.ADAPT.ISOv4Plugin.ExportMappers.BinaryWriter;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class BinaryWriterTest
    {
        private string _dataPath;
        private string _fileName;
        private BinaryWriter _binaryWriter;
        private List<Meter> _meters;
        private List<SpatialRecord> _spatialRecords;
        private NumericMeter _numericMeter;
        private ISOEnumeratedMeter _enumeratedMeter;
        private Mock<INumericValueMapper> _numericValueMapperMock;
        private Mock<IEnumeratedValueMapper> _enumeratedMeterMapperMock;
        private SpatialRecord _spatialRecord;

        [SetUp]
        public void Setup()
        {
            _meters = new List<Meter>();
            _spatialRecord = new SpatialRecord{Geometry = new Point { X = 93.6208, Y = 41.5908 }};
            _spatialRecords = new List<SpatialRecord>{ _spatialRecord };

            _numericMeter = new NumericMeter
            {
                Representation = RepresentationInstanceList.vrAvgHarvestMoisture.ToModelRepresentation(),
                UnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("prcnt"),
                SectionId = 1
            };

            _enumeratedMeter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtSkyCondition.ToModelRepresentation(),
                SectionId = 1,
                ValueCodes = new List<int> { 0, 1, 2 }
            };

            _numericValueMapperMock = new Mock<INumericValueMapper>();
            _enumeratedMeterMapperMock = new Mock<IEnumeratedValueMapper>();
            _binaryWriter = new BinaryWriter(_enumeratedMeterMapperMock.Object, _numericValueMapperMock.Object);
        }

        [Test]
        public void GivenFilenameWhenWriteThenFileIsCreated()
        {
            _dataPath = Path.GetTempPath();
            _fileName = "test.bin";
            var filename = Path.Combine(_dataPath, _fileName);

            _binaryWriter.Write(filename, new List<Meter>(), new List<SpatialRecord>());

            Assert.IsTrue(File.Exists(filename));
        }

        [Test]
        public void GivenSpatialRowWhenWriteThenTimeStartTimeIsWritten()
        {
            // number of seconds since midnight is 8hr * 60min/hr * 60s/min * 1000ms/s = 28800000 = 0x01B77400
            // number of days since Jan 1 1980 is 7305 = 0x1C89
            _spatialRecord.Timestamp = new DateTime(2000, 1, 1, 8, 0, 0);

            Write();

            var expectedBytes = new List<byte> { 0x00, 0x74, 0xB7, 0x01, 0x89, 0x1C };
            VerifyFileContents(expectedBytes, 0);
        }

        [Test]
        public void GivenSpatialRowWhenWriteThenPositionIsWritten()
        {
            // North = (41.5908 / .0000001) = 415908000 = 0x18CA40A0
            // East =  (93.6208 / .0000001) = 936208000 = 0x37CD6680
            _spatialRecord.Geometry = new Point { X = 93.6208, Y = 41.5908 };

            Write();

            var expectedBytes = new List<byte> { 0xA0, 0x40, 0xCA, 0x18, 
                                                 0x80, 0x66, 0xCD, 0x37 };

            VerifyFileContents(expectedBytes, 6);
        }

        [Test]
        public void GivenSpatialRowWithNumericMeterWhenWriteThenDlvIsWritten()
        {
            _numericMeter.Id.UniqueIds.Add(GenerateUniqueId(0));
            _meters.AddRange(new List<Meter> { _numericMeter });

            _spatialRecord.SetMeterValue(_numericMeter, new NumericRepresentationValue());

            _numericValueMapperMock.Setup(x => x.Map(_numericMeter, _spatialRecord)).Returns(20);

            Write();

            var expectedBytes = new List<byte> { 0x01, 0x00, 0x14, 0x00, 0x00, 0x00 };

            VerifyFileContents(expectedBytes, 24);
        }

        [Test]
        public void GivenSpatialRowWithEnumeratedMeterWhenWriteThenDlvIsWritten()
        {
            _enumeratedMeter.Id.UniqueIds.Add(GenerateUniqueId(0));
            _meters.AddRange(new List<Meter> { _enumeratedMeter });

            _spatialRecord.SetMeterValue(_enumeratedMeter, new EnumeratedValue());
            _enumeratedMeterMapperMock.Setup(x => x.Map(_enumeratedMeter, _meters, _spatialRecord)).Returns(1);

            Write();

            var expectedBytes = new List<byte> { 0x01, 0x00, 0x01, 0x00, 0x00, 0x00 };
            VerifyFileContents(expectedBytes, 24);
        }

        [Test]
        public void GivenSpatialRowWithNumericAndEnumeratedMeterWhenWriteWhenDlvsAreWritten()
        {
            _enumeratedMeter.Id.UniqueIds.Add(GenerateUniqueId(0));
            _numericMeter.Id.UniqueIds.Add(GenerateUniqueId(1));
            _meters.AddRange(new List<Meter> { _enumeratedMeter, _numericMeter });

            _spatialRecord.SetMeterValue(_enumeratedMeter, new EnumeratedValue());
            _spatialRecord.SetMeterValue(_numericMeter, new NumericRepresentationValue());

            _enumeratedMeterMapperMock.Setup(x => x.Map(_enumeratedMeter, _meters, _spatialRecord)).Returns(1);
            _numericValueMapperMock.Setup(x => x.Map(_numericMeter, _spatialRecord)).Returns(20);

            Write();

            var expectedBytes = new List<byte>
            {
                0x02,                           // number of DLVs to follow
                0x00, 0x01, 0x00, 0x00, 0x00,   // DLV0 and value
                0x01, 0x14, 0x00, 0x00, 0x00    // DLV1 and value
            };

            VerifyFileContents(expectedBytes, 24);
        }

        [Test]
        public void GivenMultipleSpatialRowsWhenWriteThenAreWritten()
        {
            _enumeratedMeter.Id.UniqueIds.Add(GenerateUniqueId(0));
            _numericMeter.Id.UniqueIds.Add(GenerateUniqueId(1));
            _meters.AddRange(new List<Meter> { _enumeratedMeter, _numericMeter });

            var spatialRecord1 = new SpatialRecord
            {
                Timestamp = new DateTime(2000, 1, 1, 8, 0, 0),
                Geometry = new Point { X = 93.6208, Y = 41.5908 }
            };

            spatialRecord1.SetMeterValue(_enumeratedMeter,
                new EnumeratedValue
                {
                    Value = new AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember { Code = 1 }
                });
            spatialRecord1.SetMeterValue(_numericMeter, new NumericRepresentationValue());
            _numericValueMapperMock.Setup(x => x.Map(_numericMeter, spatialRecord1)).Returns(20);

            var spatialRecord2 = new SpatialRecord
            {
                Timestamp = new DateTime(2000, 1, 1, 8, 0, 0),
                Geometry = new Point { X = 93.6208, Y = 41.5908 }
            };

            spatialRecord2.SetMeterValue(_enumeratedMeter,
                new EnumeratedValue
                {
                    Value = new AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember { Code = 2 }
                });
            spatialRecord2.SetMeterValue(_numericMeter, new NumericRepresentationValue());
            _numericValueMapperMock.Setup(x => x.Map(_numericMeter, spatialRecord2)).Returns(30);

            _spatialRecords = new List<SpatialRecord> { spatialRecord1, spatialRecord2 };
            _enumeratedMeterMapperMock.Setup(x => x.Map(_enumeratedMeter, _meters, spatialRecord1)).Returns(1);
            _enumeratedMeterMapperMock.Setup(x => x.Map(_enumeratedMeter, _meters, spatialRecord2)).Returns(2);

            Write();

            var expectedBytes = new List<byte>
            {
                0x00, 0x74, 0xB7, 0x01, 0x89, 0x1C,                 // SpatialRecord1 TimeStart
                0xA0, 0x40, 0xCA, 0x18,                             // SpatialRecord1 Position North
                0x80, 0x66, 0xCD, 0x37,                             // SpatialRecord1 Position East
                0x00, 0x00, 0x00, 0x00,                             // SpatialRecord1 Position Up
                0x00, 0x0B, 0x01, 0x03, 0x89, 0x1C,                 // SpatialRecord1 GpsUtcTime and Date
                0x02,                                               // number of DLVs to follow
                0x00, 0x01, 0x00, 0x00, 0x00,                       // DLV0 and value
                0x01, 0x14, 0x00, 0x00, 0x00,                       // DLV1 and value
                0x00, 0x74, 0xB7, 0x01, 0x89, 0x1C,                 // SpatialRecord2 TimeStart
                0xA0, 0x40, 0xCA, 0x18,                             // SpatialRecord1 Position North
                0x80, 0x66, 0xCD, 0x37,                             // SpatialRecord1 Position East
                0x00, 0x00, 0x00, 0x00,                             // SpatialRecord1 Position Up
                0x00, 0x0B, 0x01, 0x03, 0x89, 0x1C,                 // SpatialRecord2 GpsUtcTime and Date
                0x02,                                               // number of DLVs to follow
                0x00, 0x02, 0x00, 0x00, 0x00,                       // DLV0 and value
                0x01, 0x1E, 0x00, 0x00, 0x00                        // DLV1 and value
            };

            VerifyFileContents(expectedBytes, 0);
        }

        [Test]
        public void GivenSpatialRecordWithEnumeratedMetersWithSameIdsWhenWriteThenEnumeratedMeterIsCombined()
        {
            var isoEnumeratedMeter = new ISOEnumeratedMeter();
            _meters.AddRange(new List<Meter> { isoEnumeratedMeter });

            var spatialRecord = new SpatialRecord
            {
                Timestamp = new DateTime(2000, 1, 1, 8, 0, 0),
                Geometry = new Point { X = 93.6208, Y = 41.5908 }
            };
            spatialRecord.SetMeterValue(isoEnumeratedMeter, new EnumeratedValue());
            _spatialRecords = new List<SpatialRecord> { spatialRecord };

            const uint value = 0x12345678;
            _enumeratedMeterMapperMock.Setup(x => x.Map(isoEnumeratedMeter, _meters, spatialRecord)).Returns(value);

            Write();

            var expectedBytes = new List<byte>
            {
                0x00, 0x74, 0xB7, 0x01, 0x89, 0x1C,                 // SpatialRecord1 TimeStart
                0xA0, 0x40, 0xCA, 0x18,                             // SpatialRecord1 Position North
                0x80, 0x66, 0xCD, 0x37,                             // SpatialRecord1 Position East
                0x00, 0x00, 0x00, 0x00,                             // SpatialRecord1 Position Up
                0x00, 0x0B, 0x01, 0x03, 0x89, 0x1C,                 // SpatialRecord1 GpsUtcTime and Date
                0x01,                                               // number of DLVs to follow
                0x00, 0x78, 0x56, 0x34, 0x12,                       // DLV0 and value
            };

            VerifyFileContents(expectedBytes, 0);
        }

        private void VerifyFileContents(IEnumerable<byte> expectedBytes, int startIndex)
        {
            var filename = Path.Combine(_dataPath, _fileName);

            using (var binaryReader = new BinaryReader(File.OpenRead(filename)))
            {
                binaryReader.ReadBytes(startIndex);

                foreach (byte t in expectedBytes)
                {
                    var byteIn = binaryReader.ReadByte();
                    Assert.AreEqual(t, byteIn);
                }
            }
        }

        private UniqueId GenerateUniqueId(int order)
        {
            return new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = "DLV" + order,
                Source = UniqueIdMapper.IsoSource
            };
        }

        private void Write()
        {
            _dataPath = Path.GetTempPath();
            _fileName = "test.bin";
            var filename = Path.Combine(_dataPath, _fileName);

            _binaryWriter.Write(filename, _meters, _spatialRecords);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(Path.Combine(_dataPath, _fileName)))
                File.Delete(Path.Combine(_dataPath, _fileName));
        }

    }
}
