using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IBinaryReader
    {
        IEnumerable<ISOSpatialRow> Read(string dataPath, string fileName, TIM tim);
    }

    public class BinaryReader : IBinaryReader
    {
        private DateTime _firstDayOf1980 = new DateTime(1980, 01, 01);

        public IEnumerable<ISOSpatialRow> Read(string dataPath, string fileName, TIM tim)
        {
            if (tim == null)
                yield break;

            if (!File.Exists(Path.Combine(dataPath, fileName)))
                yield break;

            using (var binaryReader = new System.IO.BinaryReader(File.Open(Path.Combine(dataPath, fileName), FileMode.Open)))
            {
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    var ptn = tim.Items.FirstOrDefault(x => x.GetType() == typeof (PTN)) as PTN;
                    
                    var record = new ISOSpatialRow { TimeStart = GetStartTime(tim, binaryReader) };

                    if (ptn != null)
                    {
                        record.NorthPosition = ReadInt32(ptn.A, ptn.ASpecified, binaryReader).GetValueOrDefault(0);
                        record.EastPosition = ReadInt32(ptn.B, ptn.BSpecified, binaryReader).GetValueOrDefault(0);
                        record.Elevation = ReadInt32(ptn.C, ptn.CSpecified, binaryReader);
                        record.PositionStatus = ReadByte(ptn.D, ptn.DSpecified, binaryReader);
                        record.PDOP = ReadShort(ptn.E, ptn.ESpecified, binaryReader);
                        record.HDOP = ReadShort(ptn.F, ptn.FSpecified, binaryReader);
                        record.NumberOfSatellites = ReadByte(ptn.G, ptn.GSpecified, binaryReader);

                        SetGpsUtcDateTime(ptn, record, binaryReader);
                    }
                    
                    SetSpatialValues(tim, record, binaryReader);
                    yield return record;
                }
            }
        }

        private static void SetSpatialValues(TIM tim, ISOSpatialRow record, System.IO.BinaryReader binaryReader)
        {
            var numberOfDLVs = binaryReader.ReadByte();
            record.SpatialValues = new List<SpatialValue>();

            for (int i = 0; i < numberOfDLVs; i++)
            {
                var order = binaryReader.ReadByte();
                var value = binaryReader.ReadInt32();

                record.SpatialValues.Add(CreateSpatialValue(tim, order, value));
            }
        }

        private void SetGpsUtcDateTime(PTN ptn, ISOSpatialRow record, System.IO.BinaryReader binaryReader)
        {
            if (ptn.HSpecified)
            {
                if (ptn.H.HasValue)
                    record.GpsUtcTime = Convert.ToInt32(ptn.H.Value);
                else
                    record.GpsUtcTime = binaryReader.ReadInt32();
            }

            if (ptn.ISpecified)
            {
                if (ptn.I.HasValue)
                    record.GpsUtcDate = (short)ptn.I.Value;
                else
                    record.GpsUtcDate = binaryReader.ReadInt16();
            }

            if (record.GpsUtcDate != null && record.GpsUtcTime != null)
                record.GpsUtcDateTime = _firstDayOf1980.AddDays((double) record.GpsUtcDate).AddMilliseconds((double) record.GpsUtcTime);
        }

      

        private static short? ReadShort(double? value, bool specified, System.IO.BinaryReader binaryReader)
        {
            if (specified)
            {
                if (value.HasValue)
                    return (short)value.Value;
                return binaryReader.ReadInt16();
            }
            return null;
        }

     

        private static byte? ReadByte(byte? byteValue, bool specified, System.IO.BinaryReader binaryReader)
        {
            if (specified)
            {
                if (byteValue.HasValue)
                    return byteValue;
                return binaryReader.ReadByte();
            }
            return null;
        }


        private static int? ReadInt32(double? d, bool specified, System.IO.BinaryReader binaryReader)
        {
            if (specified)
            {
                if (d.HasValue)
                    return (int)d.Value;

                return binaryReader.ReadInt32();
            }
            return null;
        }

        private DateTime GetStartTime(TIM tim, System.IO.BinaryReader binaryReader)
        {
            if (tim.ASpecified && tim.A == null)
            {
                var milliseconds = (double) binaryReader.ReadInt32();
                var daysFrom1980 = binaryReader.ReadInt16();
                return _firstDayOf1980.AddDays(daysFrom1980).AddMilliseconds(milliseconds);
            }
            else if(tim.ASpecified)
                return (DateTime) tim.A.Value;

            return _firstDayOf1980;
        }

        private static SpatialValue CreateSpatialValue(TIM tim, byte order, int value)
        {
            var dlvs = tim.Items.Where(x => x.GetType() == typeof (DLV)); // Todo: change to DLV
            var matchingDlv = dlvs.ElementAtOrDefault(order) as DLV;

            if (matchingDlv == null)
                return null;

            var ddis = DdiLoader.Ddis;

            var resolution = 1d;
            if (matchingDlv.A != null && ddis.ContainsKey(Convert.ToInt32(matchingDlv.A, 16)))
                resolution = ddis[Convert.ToInt32(matchingDlv.A)].Resolution; 

            var spatialValue = new SpatialValue
            {
                Id = order,
                Dlv = matchingDlv,
                Value = value * resolution,
            };

            return spatialValue;
        }
    }
}
