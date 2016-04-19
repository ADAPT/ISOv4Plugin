using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IBinaryWriter
    {
        IEnumerable<ISOSpatialRow> Write(string fileName, IEnumerable<Section> sections, IEnumerable<Meter> meters, IEnumerable<SpatialRecord> spatialRecords);
    }

    public class BinaryWriter : IBinaryWriter
    {
        private readonly IEnumeratedValueMapper _enumeratedValueMapper;
        private readonly DateTime _januaryFirst1980 = new DateTime(1980,1,1);
        private readonly IRepresentationMapper _representationMapper;

        public BinaryWriter() : this (new EnumeratedValueMapper(), new RepresentationMapper())
        {
            
        }

        public BinaryWriter(IEnumeratedValueMapper enumeratedValueMapper, IRepresentationMapper representationMapper)
        {
            _enumeratedValueMapper = enumeratedValueMapper;
            _representationMapper = representationMapper;
        }

        public IEnumerable<ISOSpatialRow> Write(string fileName, IEnumerable<Section> sections, IEnumerable<Meter> meters, IEnumerable<SpatialRecord> spatialRecords)
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

        private void WriteSpatialRecord(SpatialRecord spatialRecord, IEnumerable<Meter> meters, System.IO.BinaryWriter binaryWriter)
        {
            WriteTimeStart(spatialRecord.Timestamp, binaryWriter);
            WritePosition(spatialRecord.Geometry, binaryWriter);
            // TODO:  PDOP, HDOP, Number Of Satellites- all have to do with precision of location from satellites
            WriteGpcUtcTime(spatialRecord.Timestamp.ToUniversalTime(), binaryWriter);
            WriteMeterValues(spatialRecord, meters.ToList(), binaryWriter);
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
            if (geometry != null)
            {
                const double coordinateMultiplier = 0.0000001;
                var location = geometry as Point;

                if (location == null) return;

                var north = (Int32) (location.Y/coordinateMultiplier);
                var east = (Int32) (location.X/coordinateMultiplier);
                var up = (Int32) (location.Z.GetValueOrDefault());

                binaryWriter.Write(north);
                binaryWriter.Write(east);
                binaryWriter.Write(up);
            }
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

            var ddis = DdiLoader.Ddis;

            foreach (var meter in uniqueMeters)
            {
                var order = (byte)meter.Id.FindIntIsoId();
                binaryWriter.Write(order);
            
                if (meter.GetType() == typeof (NumericMeter))
                {
                    var value = (NumericRepresentationValue)spatialRecord.GetMeterValue(meter);
                    if(value == null)
                        binaryWriter.Write((UInt32)0);
                    else
                    {
                        var resolution = GetResolution(value, ddis);
                        var convertedValue = (UInt32)(value.Value.Value / resolution);

                        binaryWriter.Write(convertedValue);
                    }
                }

                var isoEnumerateMeter = meter as ISOEnumeratedMeter;
                if (isoEnumerateMeter != null)
                {
                    var value = _enumeratedValueMapper.Map(isoEnumerateMeter, meters, spatialRecord);
                    binaryWriter.Write(value);
                }
            }
        }

        private double GetResolution(NumericRepresentationValue value, Dictionary<int, DdiDefinition> ddis)
        {
            var ddi = _representationMapper.Map(value.Representation);
            var resolution = 1d;
            if (ddis.ContainsKey(ddi.GetValueOrDefault()))
            {
                resolution = ddis[ddi.GetValueOrDefault()].Resolution;
            }
            return resolution;
        }
    }
}
