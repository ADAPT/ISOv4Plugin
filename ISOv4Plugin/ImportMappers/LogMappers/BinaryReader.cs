using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IBinaryReader
    {
        IEnumerable<ISOSpatialRow> Read(string dataPath, string fileName, TIMHeader timHeader);
    }

    public class BinaryReader : IBinaryReader
    {
        private DateTime _firstDayOf1980 = new DateTime(1980, 01, 01);

        public IEnumerable<ISOSpatialRow> Read(string dataPath, string fileName, TIMHeader timHeader)
        {
            if (timHeader == null)
                yield break;

            if (!File.Exists(Path.Combine(dataPath, fileName)))
                yield break;

            using (var binaryReader = new System.IO.BinaryReader(File.Open(Path.Combine(dataPath, fileName), FileMode.Open)))
            {
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    var ptnHeader = timHeader.PtnHeader;
                    var record = new ISOSpatialRow
                    {
                        TimeStart = GetStartTime(timHeader, binaryReader),
                        NorthPosition = ReadInt32(ptnHeader.PositionNorth, binaryReader).GetValueOrDefault(0),
                        EastPosition = ReadInt32(ptnHeader.PositionEast, binaryReader).GetValueOrDefault(0),
                        Elevation = ReadInt32(ptnHeader.PositionUp, binaryReader),
                        PositionStatus = ReadByte(ptnHeader.PositionStatus, binaryReader),
                        PDOP = ReadShort(ptnHeader.PDOP, binaryReader),
                        HDOP = ReadShort(ptnHeader.HDOP, binaryReader),
                        NumberOfSatellites = ReadByte(ptnHeader.NumberOfSatellites, binaryReader)
                    };
                    SetGpsUtcDateTime(ptnHeader, record, binaryReader);
                    SetSpatialValues(timHeader, record, binaryReader);

                    yield return record;
                }
            }
        }

        private static void SetSpatialValues(TIMHeader timHeader, ISOSpatialRow record, System.IO.BinaryReader binaryReader)
        {
            var numberOfDLVs = binaryReader.ReadByte();
            record.SpatialValues = new List<SpatialValue>();

            for (int i = 0; i < numberOfDLVs; i++)
            {
                var order = binaryReader.ReadByte();
                var value = binaryReader.ReadInt32();

                record.SpatialValues.Add(CreateSpatialValue(timHeader, order, value));
            }
        }

        private void SetGpsUtcDateTime(PTNHeader ptnHeader, ISOSpatialRow record, System.IO.BinaryReader binaryReader)
        {
            if(ptnHeader.GpsUtcTime.State != HeaderPropertyState.IsNull)
                record.GpsUtcTime = ReadInt32(ptnHeader.GpsUtcTime, binaryReader);
            if (ptnHeader.GpsUtcDate.State != HeaderPropertyState.IsNull)
                record.GpsUtcDate = ReadShort(ptnHeader.GpsUtcDate, binaryReader);

            if (record.GpsUtcDate != null && record.GpsUtcTime != null)
                record.GpsUtcDateTime = _firstDayOf1980.AddDays((double) record.GpsUtcDate).AddMilliseconds((double) record.GpsUtcTime);
        }

        private static short? ReadShort(HeaderProperty headerMetadata, System.IO.BinaryReader binaryReader)
        {
            if (headerMetadata.State == HeaderPropertyState.IsEmpty)
                return binaryReader.ReadInt16();
            if (headerMetadata.State == HeaderPropertyState.HasValue)
                return (short)headerMetadata.Value;
            return null;
        }

        private static byte? ReadByte(HeaderProperty headerMetadata, System.IO.BinaryReader binaryReader)
        {
            if (headerMetadata.State == HeaderPropertyState.IsEmpty)
                return binaryReader.ReadByte();
            if (headerMetadata.State == HeaderPropertyState.HasValue)
                return Convert.ToByte(headerMetadata.Value);
            return null;
        }

        private static int? ReadInt32(HeaderProperty headerMetadata, System.IO.BinaryReader binaryReader)
        {
            if (headerMetadata.State == HeaderPropertyState.IsEmpty)
                return binaryReader.ReadInt32();
            if (headerMetadata.State == HeaderPropertyState.HasValue)
                return (int)headerMetadata.Value;
            return null;
        }

        private DateTime GetStartTime(TIMHeader timHeader, System.IO.BinaryReader binaryReader)
        {
            if (timHeader.Start.State == HeaderPropertyState.IsEmpty)
            {
                var milliseconds = (double) binaryReader.ReadInt32();
                var daysFrom1980 = binaryReader.ReadInt16();
                return _firstDayOf1980.AddDays(daysFrom1980).AddMilliseconds(milliseconds);
            }
            return (DateTime) timHeader.Start.Value;
        }

        private static SpatialValue CreateSpatialValue(TIMHeader timHeader, byte order, int value)
        {
            var matchingDlvHeader = timHeader.DLVs.ElementAtOrDefault(order);

            if (matchingDlvHeader == null)
                return null;

            var ddis = DdiLoader.Ddis;

            var resolution = 1d;
            if (matchingDlvHeader.ProcessDataDDI.Value != null && ddis.ContainsKey((int)matchingDlvHeader.ProcessDataDDI.Value))
                resolution = ddis[(int)matchingDlvHeader.ProcessDataDDI.Value].Resolution; 

            var spatialValue = new SpatialValue
            {
                Id = order,
                DlvHeader = matchingDlvHeader,
                Value = value * resolution,
            };

            return spatialValue;
        }
    }
}
