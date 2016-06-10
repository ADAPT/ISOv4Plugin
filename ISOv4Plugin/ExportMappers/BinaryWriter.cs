using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IBinaryWriter
    {
        IEnumerable<ISOSpatialRow> Write(string fileName, List<WorkingData> meters, IEnumerable<SpatialRecord> spatialRecords);
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

        public IEnumerable<ISOSpatialRow> Write(string fileName, List<WorkingData> meters, IEnumerable<SpatialRecord> spatialRecords)
        {
            Debug.WriteLine("Writing file " + fileName);

            using (var memoryStream = new MemoryStream())
            {
                foreach (var spatialRecord in spatialRecords)
                { 
                    WriteSpatialRecord(spatialRecord, meters, memoryStream);
                }
                var binaryWriter = new System.IO.BinaryWriter(File.Create(fileName));
                binaryWriter.Write(memoryStream.ToArray());
                binaryWriter.Flush();
                binaryWriter.Close();
            }

            return null;
        }

        private void WriteSpatialRecord(SpatialRecord spatialRecord, List<WorkingData> meters, MemoryStream memoryStream)
        {
            WriteTimeStart(spatialRecord.Timestamp, memoryStream);
            WritePosition(spatialRecord.Geometry, memoryStream);
            WriteMeterValues(spatialRecord, meters, memoryStream);
        }
        
        private void WriteTimeStart(DateTime timestamp, MemoryStream memoryStream)
        {
            var millisecondsSinceMidnight = (UInt32)new TimeSpan(0, timestamp.Hour, timestamp.Minute,
                timestamp.Second, timestamp.Millisecond).TotalMilliseconds;

            var daysSinceJanOne1980 = (UInt16)(timestamp - (_januaryFirst1980)).TotalDays;

            var millisecondsMemStream = new MemoryStream(BitConverter.GetBytes(millisecondsSinceMidnight));
            millisecondsMemStream.WriteTo(memoryStream);

            var daysMemStream = new MemoryStream(BitConverter.GetBytes(daysSinceJanOne1980));
            daysMemStream.WriteTo(memoryStream);
        }

        private void WritePosition(Shape geometry, MemoryStream memoryStream)
        {
            if (geometry == null) 
                return;

            var location = geometry as Point;

            if (location == null) return;

            var north = (Int32) (location.Y/CoordinateMultiplier);
            var east = (Int32) (location.X/CoordinateMultiplier);
            var up = (Int32) (location.Z.GetValueOrDefault());

            var northStream = new MemoryStream(BitConverter.GetBytes(north));
            northStream.WriteTo(memoryStream);

            var eastStream = new MemoryStream(BitConverter.GetBytes(east));
            eastStream.WriteTo(memoryStream);

            var upStream = new MemoryStream(BitConverter.GetBytes(up));
            upStream.WriteTo(memoryStream);
        }

        private readonly Dictionary<int, uint> previousDlvs = new Dictionary<int, uint>();

        private void WriteMeterValues(SpatialRecord spatialRecord, List<WorkingData> meters, MemoryStream memoryStream)
        {
            var dlvOrders = meters.Select(x => x.Id.FindIntIsoId()).Distinct();
            var dlvsToWrite = new Dictionary<int, uint>();

            if (dlvOrders.Contains(-1))
                dlvsToWrite = GetMeterValuesAndAssignDlvNumbers(spatialRecord, meters);
            else
                dlvsToWrite = GetMeterValues(spatialRecord, meters);

            var numberOfMeters = (byte)dlvsToWrite.Count;
            var numberOfMetersStream = new MemoryStream();
            numberOfMetersStream.WriteByte(numberOfMeters);
            numberOfMetersStream.WriteTo(memoryStream);

            foreach (var key in dlvsToWrite.Keys)
            {
                var order = (byte) key;
                var value = dlvsToWrite[key];

                memoryStream.WriteByte(order);

                var valueStream = new MemoryStream(BitConverter.GetBytes(value));
                valueStream.WriteTo(memoryStream);
            }
        }

        private Dictionary<int, uint> GetMeterValues(SpatialRecord spatialRecord, List<WorkingData> meters)
        {
            var dlvsToWrite = new Dictionary<int, uint>();
            var metersWithValues = meters.Where(x => spatialRecord.GetMeterValue(x) != null);
            var dlvOrders = metersWithValues.Select(x => x.Id.FindIntIsoId()).Distinct();

            foreach (var order in dlvOrders)
            {
                var dlvMeters = meters.Where(x => x.Id.FindIntIsoId() == order);
                var numericMeter = dlvMeters.First() as NumericWorkingData;
                UInt32? value = null;
                if (numericMeter != null && spatialRecord.GetMeterValue(numericMeter) != null)
                {
                    value = _numericValueMapper.Map(numericMeter, spatialRecord);
                }

                var isoEnumerateMeter = dlvMeters.First() as ISOEnumeratedMeter;
                if (isoEnumerateMeter != null && spatialRecord.GetMeterValue(isoEnumerateMeter) != null)
                {
                    value = _enumeratedValueMapper.Map(isoEnumerateMeter, dlvMeters.ToList(), spatialRecord);
                }

                if (value == null)
                    continue;

                if (previousDlvs.ContainsKey(order) && previousDlvs[order] != value)
                {
                    previousDlvs[order] = value.Value;
                    dlvsToWrite.Add(order, value.Value);
                }
                else if (!previousDlvs.ContainsKey(order))
                {
                    previousDlvs.Add(order, value.Value);
                    dlvsToWrite.Add(order, value.Value);
                }
            }

            return dlvsToWrite;
        }

        private Dictionary<int, uint> GetMeterValuesAndAssignDlvNumbers(SpatialRecord spatialRecord, List<WorkingData> meters)
        {
            var dlvValues = new Dictionary<int, uint>();

            for (int meterIndex = 0; meterIndex < meters.Count; meterIndex++)
            {
                var meter = meters[meterIndex];
                var numericMeter = meter as NumericWorkingData;
                UInt32? value = null;
                if (numericMeter != null && spatialRecord.GetMeterValue(numericMeter) != null)
                {
                    value = _numericValueMapper.Map(numericMeter, spatialRecord);
                }

                var isoEnumerateMeter = meter as ISOEnumeratedMeter;
                if (isoEnumerateMeter != null && spatialRecord.GetMeterValue(isoEnumerateMeter) != null)
                {
                    value = _enumeratedValueMapper.Map(isoEnumerateMeter, new List<WorkingData> {meter}, spatialRecord);
                }

                if (value == null)
                    continue;

                if (previousDlvs.ContainsKey(meterIndex) && previousDlvs[meterIndex] != value)
                {
                    previousDlvs[meterIndex] = value.Value;
                    dlvValues.Add(meterIndex, value.Value);
                }
                else if (!previousDlvs.ContainsKey(meterIndex))
                {
                    previousDlvs.Add(meterIndex, value.Value);
                    dlvValues.Add(meterIndex, value.Value);
                }
            }

            return dlvValues;
        }
    }
}
