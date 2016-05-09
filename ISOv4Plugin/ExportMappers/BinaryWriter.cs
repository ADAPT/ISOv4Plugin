using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IBinaryWriter
    {
        IEnumerable<ISOSpatialRow> Write(string fileName, List<Meter> meters, IEnumerable<SpatialRecord> spatialRecords);
    }

    public class BinaryWriter : IBinaryWriter
    {
        private const double CoordinateMultiplier = 0.0000001;
        private readonly DateTime _januaryFirst1980 = new DateTime(1980,1,1);

        private readonly IEnumeratedValueMapper _enumeratedValueMapper;
        private readonly INumericValueMapper _numericValueMapper;

        public BinaryWriter() : this (new EnumeratedValueMapper(), new NumericValueMapper())
        {
            
        }

        public BinaryWriter(IEnumeratedValueMapper enumeratedValueMapper, INumericValueMapper numericValueMapper)
        {
            _enumeratedValueMapper = enumeratedValueMapper;
            _numericValueMapper = numericValueMapper;
        }

        public IEnumerable<ISOSpatialRow> Write(string fileName, List<Meter> meters, IEnumerable<SpatialRecord> spatialRecords)
        {
            using (var binaryWriter = new System.IO.BinaryWriter(File.Create(fileName)))
            {
                foreach (var spatialRecord in spatialRecords)
                {
                    WriteSpatialRecord(spatialRecord, meters, binaryWriter);
                }
            }

            return null;
        }

        private void WriteSpatialRecord(SpatialRecord spatialRecord, List<Meter> meters, System.IO.BinaryWriter binaryWriter)
        {
            WriteTimeStart(spatialRecord.Timestamp, binaryWriter);
            WritePosition(spatialRecord.Geometry, binaryWriter);
            //WriteGpcUtcTime(spatialRecord.Timestamp.ToUniversalTime(), binaryWriter);
            WriteMeterValues(spatialRecord, meters, binaryWriter);
        }

        private void WriteTimeStart(DateTime timestamp, System.IO.BinaryWriter binaryWriter)
        {
            var millisecondsSinceMidnight = (UInt32)new TimeSpan(0, timestamp.Hour, timestamp.Minute,
                timestamp.Second, timestamp.Millisecond).TotalMilliseconds;

            var daysSinceJanOne1980 = (UInt16)(timestamp - (_januaryFirst1980)).TotalDays;

            binaryWriter.Write(BitConverter.GetBytes(millisecondsSinceMidnight));
            binaryWriter.Write(BitConverter.GetBytes(daysSinceJanOne1980));
        }

        private void WritePosition(Shape geometry, System.IO.BinaryWriter binaryWriter)
        {
            if (geometry == null) 
                return;

            var location = geometry as Point;

            if (location == null) return;

            var north = (Int32) (location.Y/CoordinateMultiplier);
            var east = (Int32) (location.X/CoordinateMultiplier);
            var up = (Int32) (location.Z.GetValueOrDefault());

            binaryWriter.Write(north);
            binaryWriter.Write(east);
            binaryWriter.Write(up);
        }

        private void WriteGpcUtcTime(DateTime timestamp, System.IO.BinaryWriter binaryWriter)
        {
            WriteTimeStart(timestamp, binaryWriter);
        }

        private void WriteMeterValues(SpatialRecord spatialRecord, List<Meter> meters, System.IO.BinaryWriter binaryWriter)
        {
            var uniqueMeters = meters.GroupBy(x => x.Id.FindIntIsoId())
                .Select(x => x.First())
                .Where(x => spatialRecord.GetMeterValue(x) != null)
                .OrderBy(x => x.Id.FindIntIsoId());

            var numberOfMeters = (byte)uniqueMeters.Count();
            binaryWriter.Write(numberOfMeters);

            foreach (var meter in uniqueMeters)
            {
                var order = (byte)meter.Id.FindIntIsoId();
                binaryWriter.Write(order);

                var numericMeter = meter as NumericMeter;
                if (numericMeter != null)
                {
                    var value = _numericValueMapper.Map(numericMeter, spatialRecord);
                    binaryWriter.Write(value);
                }

                var isoEnumerateMeter = meter as ISOEnumeratedMeter;
                if (isoEnumerateMeter != null)
                {
                    var value = _enumeratedValueMapper.Map(isoEnumerateMeter, meters, spatialRecord);
                    binaryWriter.Write(value);
                }
            }
        }
    }
}
