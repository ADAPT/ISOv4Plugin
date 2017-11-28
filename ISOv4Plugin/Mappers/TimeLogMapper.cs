/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers 
{
    public interface ITimeLogMapper
    {
        IEnumerable<ISOTimeLog> ExportTimeLogs(IEnumerable<OperationData> operationDatas, string dataPath);
        IEnumerable<OperationData> ImportTimeLogs(IEnumerable<ISOTimeLog> isoTimeLogs, int? prescriptionID);
    }

    public class TimeLogMapper : BaseMapper, ITimeLogMapper
    {
        public TimeLogMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "TLG")
        {
        }

        #region Export
        public IEnumerable<ISOTimeLog> ExportTimeLogs(IEnumerable<OperationData> operationDatas, string dataPath)
        {
            List<ISOTimeLog> timeLogs = new List<ISOTimeLog>();
            foreach (OperationData operation in operationDatas)
            {
                IEnumerable<SpatialRecord> spatialRecords = operation.GetSpatialRecords != null ? operation.GetSpatialRecords() : null;
                if (spatialRecords != null && spatialRecords.Any()) //No need to export a timelog if no data
                {
                    ISOTimeLog timeLog = ExportTimeLog(operation, spatialRecords, dataPath);
                    timeLogs.Add(timeLog);
                }
            }
            return timeLogs;
        }

        private ISOTimeLog ExportTimeLog(OperationData operation, IEnumerable<SpatialRecord> spatialRecords, string dataPath)
        {
            ISOTimeLog isoTimeLog = new ISOTimeLog();

            //ID
            string id = operation.Id.FindIsoId() ?? GenerateId(5);
            isoTimeLog.Filename = id;
            ExportUniqueIDs(operation.Id, id);
            TaskDataMapper.ISOIdMap.Add(operation.Id.ReferenceId, id);

            List<DeviceElementUse> deviceElementUses = operation.GetAllSections();
            IEnumerable<WorkingData> workingDatas = deviceElementUses.SelectMany(x => x.GetWorkingDatas()).ToList();

            ISOTime isoTime = new ISOTime();
            isoTime.HasStart = true;
            isoTime.Type = ISOTimeType.Effective;
            isoTime.DataLogValues = ExportDataLogValues(workingDatas, deviceElementUses).ToList();

            //Set the timelog data definition for PTN
            ISOPosition position = new ISOPosition(); 
            position.HasPositionNorth = true;
            position.HasPositionEast = true;
            position.HasPositionUp = true;
            position.HasPositionStatus = true;
            position.HasPDOP = false;
            position.HasHDOP = false;
            position.HasNumberOfSatellites = false;
            position.HasGpsUtcTime = false;
            position.HasGpsUtcTime = false;
            isoTime.Positions.Add(position);

            //Write XML
            TaskDocumentWriter xmlWriter = new TaskDocumentWriter();
            xmlWriter.WriteTimeLog(dataPath, isoTimeLog, isoTime);

            //Write BIN
            var binFilePath = Path.Combine(dataPath, isoTimeLog.Filename + ".bin");
            BinaryWriter writer = new BinaryWriter();
            writer.Write(binFilePath, workingDatas.ToList(), spatialRecords);

            return isoTimeLog;            
        }

        public IEnumerable<ISODataLogValue> ExportDataLogValues(IEnumerable<WorkingData> workingDatas, List<DeviceElementUse> deviceElementUses)
        {
            if (workingDatas == null)
            {
                return null;
            }
            //This separate subroutine is necessary due to the iterator therein
            return ExportNonNullDataLogValues(workingDatas, deviceElementUses);
        }

        private IEnumerable<ISODataLogValue> ExportNonNullDataLogValues(IEnumerable<WorkingData> workingDatas, List<DeviceElementUse> deviceElementUses)
        {
            var dlvOrders = workingDatas.Select(x => x.Id.FindIntIsoId()).Distinct().OrderBy(y => y);

            if (dlvOrders.Contains(-1))
            {
                var sortedWorkingDatas = workingDatas.OrderBy(x => x.Id.FindIntIsoId());

                foreach (var workingData in sortedWorkingDatas)
                {
                    yield return ExportDataLogValue(workingData, deviceElementUses);
                }
            }
            else
            {
                foreach (var order in dlvOrders)
                {
                    var dlvMeter = workingDatas.First(x => x.Id.FindIntIsoId() == order);
                    yield return ExportDataLogValue(dlvMeter, deviceElementUses);
                }
            }
        }

        public ISODataLogValue ExportDataLogValue(WorkingData workingData, List<DeviceElementUse> deviceElementUses)
        {
            //DDI
            int? mappedDDI = RepresentationMapper.Map(workingData.Representation);
            var dlv = new ISODataLogValue();
            if (mappedDDI != null)
            {
                if (workingData.Representation != null && workingData.Representation.Code == "dtRecordingStatus" && workingData.DeviceElementUseId != 0)
                {
                    dlv.ProcessDataDDI = 141.AsHexDDI(); //No support for exporting CondensedWorkState at this time
                }
                else
                {
                    dlv.ProcessDataDDI = mappedDDI.Value.AsHexDDI();
                }
            }
            else if (workingData.Representation.CodeSource == ApplicationDataModel.Representations.RepresentationCodeSourceEnum.ISO11783_DDI)
            {
                dlv.ProcessDataDDI = workingData.Representation.Code;
            }

            //DeviceElementIdRef
            DeviceElementUse use = deviceElementUses.FirstOrDefault(d => d.Id.ReferenceId == workingData.DeviceElementUseId);
            if (use != null)
            {
                DeviceElementConfiguration deviceElementConfiguration = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(d => d.Id.ReferenceId == use.DeviceConfigurationId);
                if (deviceElementConfiguration != null)
                {
                    if (TaskDataMapper.ISOIdMap.ContainsKey(deviceElementConfiguration.DeviceElementId))
                    {
                        //This requires the Devices will have been mapped prior to the LoggedData
                        dlv.DeviceElementIdRef = TaskDataMapper.ISOIdMap[deviceElementConfiguration.DeviceElementId];
                    }
                }
            }

            return dlv;
        }

        private class BinaryWriter
        {
            private const double CoordinateMultiplier = 0.0000001;
            private readonly DateTime _januaryFirst1980 = new DateTime(1980, 1, 1);

            private readonly IEnumeratedValueMapper _enumeratedValueMapper;
            private readonly INumericValueMapper _numericValueMapper;

            public BinaryWriter() : this(new EnumeratedValueMapper(), new NumericValueMapper())
            {

            }

            public BinaryWriter(IEnumeratedValueMapper enumeratedValueMapper, INumericValueMapper numericValueMapper)
            {
                _enumeratedValueMapper = enumeratedValueMapper;
                _numericValueMapper = numericValueMapper;
            }

            public IEnumerable<ISOSpatialRow> Write(string fileName, List<WorkingData> meters, IEnumerable<SpatialRecord> spatialRecords)
            {
                if (spatialRecords == null)
                    return null;

                var metersByIsoIds = GetMeterToIsoIdCache(meters);

                using (var memoryStream = new MemoryStream())
                {
                    foreach (var spatialRecord in spatialRecords)
                    {
                        WriteSpatialRecord(spatialRecord, meters, memoryStream, metersByIsoIds);
                    }
                    var binaryWriter = new System.IO.BinaryWriter(File.Create(fileName));
                    binaryWriter.Write(memoryStream.ToArray());
                    binaryWriter.Flush();
                    binaryWriter.Close();
                }

                return null;
            }

            private static Dictionary<WorkingData, int> GetMeterToIsoIdCache(List<WorkingData> meters)
            {
                return meters.ToDictionary(meter => meter, meter => meter.Id.FindIntIsoId());
            }

            private void WriteSpatialRecord(SpatialRecord spatialRecord, List<WorkingData> meters, MemoryStream memoryStream, Dictionary<WorkingData, int> metersByIsoIds)
            {
                //Start Time
                var millisecondsSinceMidnight = (UInt32)new TimeSpan(0, spatialRecord.Timestamp.Hour, spatialRecord.Timestamp.Minute, spatialRecord.Timestamp.Second, spatialRecord.Timestamp.Millisecond).TotalMilliseconds;
                memoryStream.Write(BitConverter.GetBytes(millisecondsSinceMidnight), 0, 4);

                var daysSinceJanOne1980 = (UInt16)(spatialRecord.Timestamp - (_januaryFirst1980)).TotalDays;
                memoryStream.Write(BitConverter.GetBytes(daysSinceJanOne1980), 0, 2);

                //Position
                var north = Int32.MaxValue; //"Not available" value for the Timelog
                var east = Int32.MaxValue;
                var up = Int32.MaxValue;
                if (spatialRecord.Geometry != null)
                {
                    Point location = spatialRecord.Geometry as Point;
                    if (location != null)
                    {
                        north = (Int32)(location.Y / CoordinateMultiplier);
                        east = (Int32)(location.X / CoordinateMultiplier);
                        up = (Int32)(location.Z.GetValueOrDefault());
                    }
                }

                memoryStream.Write(BitConverter.GetBytes(north), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(east), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(up), 0, 4);
                memoryStream.WriteByte((byte)ISOPositionStatus.NotAvailable);

                //Values
                var dlvOrders = metersByIsoIds.Values.Distinct();
                Dictionary<int, uint> dlvsToWrite;

                if (dlvOrders.Contains(-1))
                    dlvsToWrite = GetMeterValuesAndAssignDlvNumbers(spatialRecord, meters);
                else
                    dlvsToWrite = GetMeterValues(spatialRecord, meters, metersByIsoIds);

                byte numberOfMeters = (byte)dlvsToWrite.Count;
                memoryStream.WriteByte(numberOfMeters);

                foreach (var key in dlvsToWrite.Keys)
                {
                    byte order = (byte)key;
                    uint value = dlvsToWrite[key];

                    memoryStream.WriteByte(order);
                    memoryStream.Write(BitConverter.GetBytes(value), 0, 4);
                }
            }

            private readonly Dictionary<int, uint> _previousDlvs = new Dictionary<int, uint>();
            private Dictionary<int, uint> GetMeterValues(SpatialRecord spatialRecord, List<WorkingData> meters, Dictionary<WorkingData, int> metersByIsoIds)
            {
                var dlvsToWrite = new Dictionary<int, uint>();
                var metersWithValues = meters.Where(x => spatialRecord.GetMeterValue(x) != null);
                var dlvOrders = metersWithValues.Select(m => metersByIsoIds[m]).Distinct();

                foreach (var order in dlvOrders)
                {
                    var dlvMeters = meters.Where(m => metersByIsoIds[m] == order).ToList();
                    var numericMeter = dlvMeters[0] as NumericWorkingData;
                    UInt32? value = null;
                    if (numericMeter != null && spatialRecord.GetMeterValue(numericMeter) != null)
                    {
                        value = _numericValueMapper.Map(numericMeter, spatialRecord);
                    }

                    var enumeratedMeter = dlvMeters[0] as EnumeratedWorkingData;
                    if (enumeratedMeter != null && spatialRecord.GetMeterValue(enumeratedMeter) != null)
                    {
                        value = _enumeratedValueMapper.Map(enumeratedMeter, dlvMeters, spatialRecord);
                    }

                    if (value == null)
                        continue;

                    if (_previousDlvs.ContainsKey(order) && _previousDlvs[order] != value)
                    {
                        _previousDlvs[order] = value.Value;
                        dlvsToWrite.Add(order, value.Value);
                    }
                    else if (!_previousDlvs.ContainsKey(order))
                    {
                        _previousDlvs.Add(order, value.Value);
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

                    var enumeratedMeter = meter as EnumeratedWorkingData;
                    if (enumeratedMeter != null && spatialRecord.GetMeterValue(enumeratedMeter) != null)
                    {
                        value = _enumeratedValueMapper.Map(enumeratedMeter, new List<WorkingData> { meter }, spatialRecord);
                    }

                    if (value == null)
                        continue;

                    if (_previousDlvs.ContainsKey(meterIndex) && _previousDlvs[meterIndex] != value)
                    {
                        _previousDlvs[meterIndex] = value.Value;
                        dlvValues.Add(meterIndex, value.Value);
                    }
                    else if (!_previousDlvs.ContainsKey(meterIndex))
                    {
                        _previousDlvs.Add(meterIndex, value.Value);
                        dlvValues.Add(meterIndex, value.Value);
                    }
                }

                return dlvValues;
            }
        }

        #endregion Export 

        #region Import

        public IEnumerable<OperationData> ImportTimeLogs(IEnumerable<ISOTimeLog> isoTimeLogs, int? prescriptionID)
        {
            List<OperationData> operations = new List<OperationData>();
            foreach (ISOTimeLog isoTimeLog in isoTimeLogs)
            {
                IEnumerable<OperationData> operationData = ImportTimeLog(isoTimeLog, prescriptionID);
                if (operationData != null)
                {
                    operations.AddRange(operationData);
                }
            }

            return operations;
        }

        private IEnumerable<OperationData> ImportTimeLog(ISOTimeLog isoTimeLog, int? prescriptionID)
        {
            WorkingDataMapper workingDataMapper = new WorkingDataMapper(new EnumeratedMeterFactory(), TaskDataMapper);
            SectionMapper sectionMapper = new SectionMapper(workingDataMapper, TaskDataMapper);
            SpatialRecordMapper spatialMapper = new SpatialRecordMapper(new RepresentationValueInterpolator(), sectionMapper, workingDataMapper);
            IEnumerable<ISOSpatialRow> isoRecords = ReadTimeLog(isoTimeLog, this.TaskDataPath);
            if (isoRecords != null)
            {
                isoRecords = isoRecords.ToList(); //Avoids multiple reads
                ISOTime time = isoTimeLog.GetTimeElement(this.TaskDataPath);

                //Identify unique devices represented in this TimeLog data
                IEnumerable<string> deviceElementIDs = time.DataLogValues.Where(d => d.ProcessDataDDI != "DFFF" && d.ProcessDataDDI != "DFFE").Select(d => d.DeviceElementIdRef);
                Dictionary<ISODevice, HashSet<string>> loggedDeviceElementsByDevice = new Dictionary<ISODevice, HashSet<string>>();
                foreach (string deviceElementID in deviceElementIDs)
                {
                    ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies.GetISODeviceElementFromID(deviceElementID);
                    if (isoDeviceElement != null)
                    {
                        ISODevice device = isoDeviceElement.Device;
                        if (!loggedDeviceElementsByDevice.ContainsKey(device))
                        {
                            loggedDeviceElementsByDevice.Add(device, new HashSet<string>());
                        }
                        loggedDeviceElementsByDevice[device].Add(deviceElementID);
                    }
                }

                //Split all devices in the same TimeLog into separate OperationData objects to handle multi-implement scenarios
                //This will ensure implement geometries/DeviceElementUse Depths & Orders do not get confused between implements
                List<OperationData> operationDatas = new List<OperationData>();
                foreach (ISODevice dvc in loggedDeviceElementsByDevice.Keys)
                {
                    OperationData operationData = new OperationData();

                    //This line will necessarily invoke a spatial read in order to find 
                    //1)The correct number of CondensedWorkState working datas to create 
                    //2)Any Widths and Offsets stored in the spatial data
                    IEnumerable<DeviceElementUse> sections = sectionMapper.Map(time, isoRecords, operationData.Id.ReferenceId, loggedDeviceElementsByDevice[dvc]);

                    var workingDatas = sections != null ? sections.SelectMany(x => x.GetWorkingDatas()).ToList() : new List<WorkingData>();
                    var sectionsSimple = sectionMapper.ConvertToBaseTypes(sections.ToList());

                    operationData.GetSpatialRecords = () => spatialMapper.Map(isoRecords, workingDatas);
                    operationData.MaxDepth = sections.Count() > 0 ? sections.Select(s => s.Depth).Max() : 0;
                    operationData.GetDeviceElementUses = x => x == 0 ? sectionsSimple : new List<DeviceElementUse>();
                    operationData.PrescriptionId = prescriptionID;
                    operationData.OperationType = GetOperationTypeFromLoggingDevices(time);
                    operationDatas.Add(operationData);
                }

                return operationDatas;
            }
            return null;
        }

        private OperationTypeEnum GetOperationTypeFromLoggingDevices(ISOTime time)
        {
            HashSet<DeviceOperationType> representedTypes = new HashSet<DeviceOperationType>();
            IEnumerable<string> distinctDeviceElementIDs = time.DataLogValues.Select(d => d.DeviceElementIdRef).Distinct();
            foreach (string isoDeviceElementID in distinctDeviceElementIDs)
            {
                int? deviceElementID = TaskDataMapper.ADAPTIdMap.FindByISOId(isoDeviceElementID);
                if (deviceElementID.HasValue)
                {
                    DeviceElement deviceElement = DataModel.Catalog.DeviceElements.FirstOrDefault(d => d.Id.ReferenceId == deviceElementID.Value);
                    if (deviceElement != null && deviceElement.DeviceClassification != null)
                    {
                        DeviceOperationType deviceOperationType = DeviceOperationTypes.FirstOrDefault(d => d.MachineEnumerationMember.ToModelEnumMember().Value == deviceElement.DeviceClassification.Value.Value);
                        if (deviceOperationType != null)
                        {
                            representedTypes.Add(deviceOperationType);
                        }
                    }
                }
            }

            DeviceOperationType deviceType = representedTypes.FirstOrDefault(t => t.ClientNAMEMachineType >= 2 && t.ClientNAMEMachineType <= 11);
            if (deviceType != null)
            {
                //2-11 represent known types of operations
                //These will map to implement devices and will govern the actual operation type.
                //Return the first such device type
                return deviceType.OperationType;
            }
            return OperationTypeEnum.Unknown;
        }

        private IEnumerable<ISOSpatialRow> ReadTimeLog(ISOTimeLog timeLog, string dataPath)
        {
            ISOTime templateTime = timeLog.GetTimeElement(dataPath);
            string filePath = Path.Combine(dataPath, string.Concat(timeLog.Filename, ".bin"));

            if (templateTime != null && File.Exists(filePath))
            {
                BinaryReader reader = new BinaryReader();
                return reader.Read(dataPath.WithTaskDataPath(), filePath, templateTime);
            }
            return null;
        }

        private class BinaryReader
        {
            private DateTime _firstDayOf1980 = new DateTime(1980, 01, 01);

            public IEnumerable<ISOSpatialRow> Read(string dataPath, string fileName, ISOTime templateTime)
            {
                if (templateTime == null)
                    yield break;

                if (!File.Exists(Path.Combine(dataPath, fileName)))
                    yield break;

                using (var binaryReader = new System.IO.BinaryReader(File.Open(Path.Combine(dataPath, fileName), FileMode.Open)))
                {
                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        ISOPosition templatePosition = templateTime.Positions.FirstOrDefault();

                        var record = new ISOSpatialRow { TimeStart = GetStartTime(templateTime, binaryReader) };

                        if (templatePosition != null)
                        {
                            //North and East are required binary data
                            record.NorthPosition = ReadInt32((double?)templatePosition.PositionNorth, templatePosition.HasPositionNorth, binaryReader).GetValueOrDefault(0);
                            record.EastPosition = ReadInt32((double?)templatePosition.PositionEast, templatePosition.HasPositionEast, binaryReader).GetValueOrDefault(0);

                            if (templatePosition.HasPositionUp) //Optional position attributes will be included in the binary only if a corresponding attribute is present in the PTN element
                            {
                                record.Elevation = ReadInt32(templatePosition.PositionUp, templatePosition.HasPositionUp, binaryReader);
                            }

                            //Position status is required
                            record.PositionStatus = ReadByte((byte?)templatePosition.PositionStatus, templatePosition.HasPositionStatus, binaryReader);

                            if (templatePosition.HasPDOP)
                            {
                                record.PDOP = ReadShort((double?)templatePosition.PDOP, templatePosition.HasPDOP, binaryReader);
                            }

                            if (templatePosition.HasHDOP)
                            {
                                record.HDOP = ReadShort((double?)templatePosition.HDOP, templatePosition.HasHDOP, binaryReader);
                            }

                            if (templatePosition.HasNumberOfSatellites)
                            {
                                record.NumberOfSatellites = ReadByte(templatePosition.NumberOfSatellites, templatePosition.HasNumberOfSatellites, binaryReader);
                            }

                            if (templatePosition.HasGpsUtcDate)
                            {
                                if (templatePosition.GpsUtcDate.HasValue)
                                {
                                    record.GpsUtcDate = (short)templatePosition.GpsUtcDate.Value;
                                }
                                else
                                {
                                    record.GpsUtcDate = binaryReader.ReadInt16();
                                }
                                
                            }

                            if (templatePosition.HasGpsUtcTime)
                            {
                                if (templatePosition.GpsUtcTime.HasValue)
                                {
                                    record.GpsUtcTime = Convert.ToInt32(templatePosition.GpsUtcTime.Value);
                                }
                                else
                                {
                                    record.GpsUtcTime = binaryReader.ReadInt32();
                                }
                            }

                            if (record.GpsUtcDate != null && record.GpsUtcTime != null)
                            {
                                record.GpsUtcDateTime = _firstDayOf1980.AddDays((double)record.GpsUtcDate).AddMilliseconds((double)record.GpsUtcTime);
                            }
                        }

                        var numberOfDLVs = binaryReader.ReadByte();
                        record.SpatialValues = new List<SpatialValue>();

                        for (int i = 0; i < numberOfDLVs; i++)
                        {
                            var order = binaryReader.ReadByte();
                            var value = binaryReader.ReadInt32();

                            record.SpatialValues.Add(CreateSpatialValue(templateTime, order, value));
                        }

                        yield return record;
                    }
                }
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

            private DateTime GetStartTime(ISOTime templateTime, System.IO.BinaryReader binaryReader)
            {
                if (templateTime.HasStart && templateTime.Start == null)
                {
                    var milliseconds = (double)binaryReader.ReadInt32();
                    var daysFrom1980 = binaryReader.ReadInt16();
                    return _firstDayOf1980.AddDays(daysFrom1980).AddMilliseconds(milliseconds);
                }
                else if (templateTime.HasStart)
                    return (DateTime)templateTime.Start.Value;

                return _firstDayOf1980;
            }

            private static SpatialValue CreateSpatialValue(ISOTime templateTime, byte order, int value)
            {
                var dataLogValues = templateTime.DataLogValues;
                var matchingDlv = dataLogValues.ElementAtOrDefault(order);

                if (matchingDlv == null)
                    return null;

                var ddis = DdiLoader.Ddis;

                var resolution = 1d;
                if (matchingDlv.ProcessDataDDI != null && ddis.ContainsKey(matchingDlv.ProcessDataDDI.AsInt32DDI()))
                {
                    resolution = ddis[matchingDlv.ProcessDataDDI.AsInt32DDI()].Resolution;
                }

                var spatialValue = new SpatialValue
                {
                    Id = order,
                    DataLogValue = matchingDlv,
                    Value = value * resolution,
                };

                return spatialValue;
            }
        }
        #endregion Import
    }
}
