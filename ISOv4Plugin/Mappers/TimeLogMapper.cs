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
        IEnumerable<OperationData> ImportTimeLogs(ISOTask loggedTask, int? prescriptionID);
    }

    public class TimeLogMapper : BaseMapper, ITimeLogMapper
    {
        public TimeLogMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "TLG")
        {
        }

        #region Export
        private Dictionary<int, int> _dataLogValueOrdersByWorkingDataID;
        public IEnumerable<ISOTimeLog> ExportTimeLogs(IEnumerable<OperationData> operationDatas, string dataPath)
        {
            _dataLogValueOrdersByWorkingDataID = new Dictionary<int, int>();
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
            isoTimeLog.TimeLogType = 1; // TimeLogType TLG.C is a required attribute. Currently only the value "1" is defined.
            ExportIDs(operation.Id, id);

            List<DeviceElementUse> deviceElementUses = operation.GetAllSections();
            List<WorkingData> workingDatas = deviceElementUses.SelectMany(x => x.GetWorkingDatas()).ToList();

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
            BinaryWriter writer = new BinaryWriter(_dataLogValueOrdersByWorkingDataID);
            writer.Write(binFilePath, workingDatas.ToList(), spatialRecords);

            return isoTimeLog;            
        }

        public IEnumerable<ISODataLogValue> ExportDataLogValues(List<WorkingData> workingDatas, List<DeviceElementUse> deviceElementUses)
        {
            if (workingDatas == null)
            {
                return null;
            }

            List<ISODataLogValue> dlvs = new List<ISODataLogValue>();
            int i = 0;
            foreach (WorkingData workingData in workingDatas)
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
                        //This requires the Devices will have been mapped prior to the LoggedData
                        dlv.DeviceElementIdRef = TaskDataMapper.InstanceIDMap.GetISOID(deviceElementConfiguration.DeviceElementId);
                    }
                }

                if (dlv.ProcessDataDDI != null && dlv.DeviceElementIdRef != null)
                {
                    dlvs.Add(dlv);
                    _dataLogValueOrdersByWorkingDataID.Add(workingData.Id.ReferenceId, i++);
                }
            }
            return dlvs;
        }

        private class BinaryWriter
        {   // ATTENTION: CoordinateMultiplier and ZMultiplier also exist in Import\SpatialRecordMapper.cs!
            private const double CoordinateMultiplier = 0.0000001;
            private const double ZMultiplier = 0.001;   // In ISO the PositionUp value is specified in mm.
            private readonly DateTime _januaryFirst1980 = new DateTime(1980, 1, 1);

            private readonly IEnumeratedValueMapper _enumeratedValueMapper;
            private readonly INumericValueMapper _numericValueMapper;
            private Dictionary<int, int> _dlvOrdersByWorkingDataID;

            public BinaryWriter(Dictionary<int, int> dlvOrdersByWorkingDataID) : this(new EnumeratedValueMapper(), new NumericValueMapper(), dlvOrdersByWorkingDataID)
            {
            }

            public BinaryWriter(IEnumeratedValueMapper enumeratedValueMapper, INumericValueMapper numericValueMapper, Dictionary<int, int> dlvOrdersByWorkingDataID)
            {
                _enumeratedValueMapper = enumeratedValueMapper;
                _numericValueMapper = numericValueMapper;
                _dlvOrdersByWorkingDataID = dlvOrdersByWorkingDataID;
            }

            public IEnumerable<ISOSpatialRow> Write(string fileName, List<WorkingData> meters, IEnumerable<SpatialRecord> spatialRecords)
            {
                if (spatialRecords == null)
                    return null;

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
                        up = (Int32)(location.Z.GetValueOrDefault() / ZMultiplier);
                    }
                }

                memoryStream.Write(BitConverter.GetBytes(north), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(east), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(up), 0, 4);
                memoryStream.WriteByte((byte)ISOPositionStatus.NotAvailable);

                //Values
                Dictionary<int, uint> dlvsToWrite = GetMeterValues(spatialRecord, meters);

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

            private Dictionary<int, uint> GetMeterValues(SpatialRecord spatialRecord, List<WorkingData> workingDatas)
            {
                var dlvsToWrite = new Dictionary<int, uint>();
                var workingDatasWithValues = workingDatas.Where(x => spatialRecord.GetMeterValue(x) != null);

                foreach (WorkingData workingData in workingDatasWithValues.Where(d => _dlvOrdersByWorkingDataID.ContainsKey(d.Id.ReferenceId)))
                {
                    int order = _dlvOrdersByWorkingDataID[workingData.Id.ReferenceId];

                    UInt32? value = null;
                    if (workingData is NumericWorkingData)
                    {
                        NumericWorkingData numericMeter = workingData as NumericWorkingData;
                        if (numericMeter != null && spatialRecord.GetMeterValue(numericMeter) != null)
                        {
                            value = _numericValueMapper.Map(numericMeter, spatialRecord);
                        }
                    }
                    else if (workingData is EnumeratedWorkingData)
                    {
                        EnumeratedWorkingData enumeratedMeter = workingData as EnumeratedWorkingData;
                        if (enumeratedMeter != null && spatialRecord.GetMeterValue(enumeratedMeter) != null)
                        {
                            value = _enumeratedValueMapper.Map(enumeratedMeter, new List<WorkingData>() { workingData }, spatialRecord);
                        }
                    }

                    if (value == null)
                    {
                        continue;
                    }
                    else
                    {
                        dlvsToWrite.Add(order, value.Value);
                    }
                }

                return dlvsToWrite;
            }
        }

        #endregion Export 

        #region Import

        public IEnumerable<OperationData> ImportTimeLogs(ISOTask loggedTask, int? prescriptionID)
        {
            List<OperationData> operations = new List<OperationData>();
            foreach (ISOTimeLog isoTimeLog in loggedTask.TimeLogs)
            {
                IEnumerable<OperationData> operationData = ImportTimeLog(loggedTask, isoTimeLog, prescriptionID);
                if (operationData != null)
                {
                    operations.AddRange(operationData);
                }
            }

            return operations;
        }

        private IEnumerable<OperationData> ImportTimeLog(ISOTask loggedTask, ISOTimeLog isoTimeLog, int? prescriptionID)
        {
            WorkingDataMapper workingDataMapper = new WorkingDataMapper(new EnumeratedMeterFactory(), TaskDataMapper);
            SectionMapper sectionMapper = new SectionMapper(workingDataMapper, TaskDataMapper);
            SpatialRecordMapper spatialMapper = new SpatialRecordMapper(new RepresentationValueInterpolator(), sectionMapper, workingDataMapper, TaskDataMapper);
            IEnumerable<ISOSpatialRow> isoRecords = ReadTimeLog(isoTimeLog, this.TaskDataPath);
            bool useDeferredExecution = false;
            if (isoRecords != null)
            {
                try
                {
                    if (TaskDataMapper.Properties != null)
                    {
                        //Set this property to override the default behavior of pre-iterating the data
                        bool.TryParse(TaskDataMapper.Properties.GetProperty("SpatialRecordDeferredExecution"), out useDeferredExecution);
                    }

                    if (!useDeferredExecution)
                    {
                        isoRecords = isoRecords.ToList(); //Avoids multiple reads
                    }

                    //Set a UTC "delta" from the first record where possible.  We set only one per data import.
                    if (!TaskDataMapper.GPSToLocalDelta.HasValue)
                    {
                        var firstRecord = isoRecords.FirstOrDefault();
                        if (firstRecord != null && firstRecord.GpsUtcDateTime.HasValue)
                        {
                            //Local - UTC = Delta.  This value will be rough based on the accuracy of the clock settings but will expose the ability to derive the UTC times from the exported local times.
                            TaskDataMapper.GPSToLocalDelta = (firstRecord.TimeStart - firstRecord.GpsUtcDateTime.Value).TotalHours;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDataMapper.AddError($"Timelog file {isoTimeLog.Filename} is invalid.  Skipping.", ex.Message, null, ex.StackTrace);
                    return null;
                }
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

                    //Determine products
                    Dictionary<string, List<ISOProductAllocation>> productAllocations = GetProductAllocationsByDeviceElement(loggedTask, dvc);
                    List<int> productIDs = GetDistinctProductIDs(TaskDataMapper, productAllocations);

                    //This line will necessarily invoke a spatial read in order to find 
                    //1)The correct number of CondensedWorkState working datas to create 
                    //2)Any Widths and Offsets stored in the spatial data
                    IEnumerable<DeviceElementUse> sections = sectionMapper.Map(time,
                                                                               isoRecords,
                                                                               operationData.Id.ReferenceId,
                                                                               loggedDeviceElementsByDevice[dvc],
                                                                               productAllocations);

                    var workingDatas = sections != null ? sections.SelectMany(x => x.GetWorkingDatas()).ToList() : new List<WorkingData>();

                    operationData.GetSpatialRecords = () => spatialMapper.Map(isoRecords, workingDatas, productAllocations);
                    operationData.MaxDepth = sections.Count() > 0 ? sections.Select(s => s.Depth).Max() : 0;
                    operationData.GetDeviceElementUses = x => sectionMapper.ConvertToBaseTypes(sections.Where(s => s.Depth == x).ToList());
                    operationData.PrescriptionId = prescriptionID;
                    operationData.OperationType = GetOperationTypeFromLoggingDevices(time);
                    operationData.ProductIds = productIDs; 
                    operationData.SpatialRecordCount = isoRecords.Count();
                    operationDatas.Add(operationData);
                }

                //Set the CoincidentOperationDataIds property identifying Operation Datas from the same TimeLog.
                operationDatas.ForEach(o => o.CoincidentOperationDataIds = operationDatas.Where(o2 => o2.Id.ReferenceId != o.Id.ReferenceId).Select(o3 => o3.Id.ReferenceId).ToList());

                return operationDatas;
            }
            return null;
        }

        internal static List<int> GetDistinctProductIDs(TaskDataMapper taskDataMapper, Dictionary<string, List<ISOProductAllocation>> productAllocations)
        {
            HashSet<int> productIDs = new HashSet<int>();
            foreach (string detID in productAllocations.Keys)
            {
                foreach (ISOProductAllocation pan in productAllocations[detID])
                {
                    int? id = taskDataMapper.InstanceIDMap.GetADAPTID(pan.ProductIdRef);
                    if (id.HasValue)
                    {
                        productIDs.Add(id.Value);
                    }
                }
            }
            return productIDs.ToList();
        }

        private Dictionary<string, List<ISOProductAllocation>> GetProductAllocationsByDeviceElement(ISOTask loggedTask, ISODevice dvc)
        {
            Dictionary<string, List<ISOProductAllocation>> output = new Dictionary<string, List<ISOProductAllocation>>();
            foreach (ISOProductAllocation pan in loggedTask.ProductAllocations.Where(p => !string.IsNullOrEmpty(p.DeviceElementIdRef)))
            {
                if (dvc.DeviceElements.Select(d => d.DeviceElementId).Contains(pan.DeviceElementIdRef)) //Filter PANs by this DVC
                {
                    ISODeviceElement deviceElement = dvc.DeviceElements.First(d => d.DeviceElementId == pan.DeviceElementIdRef);
                    AddProductAllocationsForDeviceElement(output, pan, deviceElement);
                }
            }
            return output;
        }

        private void AddProductAllocationsForDeviceElement(Dictionary<string, List<ISOProductAllocation>> productAllocations, ISOProductAllocation pan, ISODeviceElement deviceElement)
        {
            if (!productAllocations.ContainsKey(deviceElement.DeviceElementId))
            {
                productAllocations.Add(deviceElement.DeviceElementId, new List<ISOProductAllocation>());
            }
            productAllocations[deviceElement.DeviceElementId].Add(pan);

            foreach (ISODeviceElement child in deviceElement.ChildDeviceElements)
            {
                AddProductAllocationsForDeviceElement(productAllocations, pan, child);
            }
        }

        private OperationTypeEnum GetOperationTypeFromLoggingDevices(ISOTime time)
        {
            HashSet<DeviceOperationType> representedTypes = new HashSet<DeviceOperationType>();
            IEnumerable<string> distinctDeviceElementIDs = time.DataLogValues.Select(d => d.DeviceElementIdRef).Distinct();
            foreach (string isoDeviceElementID in distinctDeviceElementIDs)
            {
                int? deviceElementID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoDeviceElementID);
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
            string binName = string.Concat(timeLog.Filename, ".bin");
            string filePath = dataPath.GetDirectoryFiles(binName, SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (templateTime != null && filePath != null)
            {
                BinaryReader reader = new BinaryReader();
                return reader.Read(filePath, templateTime, TaskDataMapper.DeviceElementHierarchies);
            }
            return null;
        }

        private class BinaryReader
        {
            private DateTime _firstDayOf1980 = new DateTime(1980, 01, 01);

            public IEnumerable<ISOSpatialRow> Read(string fileName, ISOTime templateTime, DeviceElementHierarchies deviceHierarchies)
            {
                if (templateTime == null)
                    yield break;

                if (!File.Exists(fileName))
                    yield break;

                using (var binaryReader = new System.IO.BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
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
                                record.PDOP = ReadUShort((double?)templatePosition.PDOP, templatePosition.HasPDOP, binaryReader);
                            }

                            if (templatePosition.HasHDOP)
                            {
                                record.HDOP = ReadUShort((double?)templatePosition.HDOP, templatePosition.HasHDOP, binaryReader);
                            }

                            if (templatePosition.HasNumberOfSatellites)
                            {
                                record.NumberOfSatellites = ReadByte(templatePosition.NumberOfSatellites, templatePosition.HasNumberOfSatellites, binaryReader);
                            }

                            if (templatePosition.HasGpsUtcTime)
                            {
                                if (templatePosition.GpsUtcTime.HasValue)
                                {
                                    record.GpsUtcTime = Convert.ToUInt32(templatePosition.GpsUtcTime.Value);
                                }
                                else
                                {
                                    record.GpsUtcTime = binaryReader.ReadUInt32();
                                }
                            }

                            if (templatePosition.HasGpsUtcDate)
                            {
                                if (templatePosition.GpsUtcDate.HasValue)
                                {
                                    record.GpsUtcDate = (ushort)templatePosition.GpsUtcDate.Value;
                                }
                                else
                                {
                                    record.GpsUtcDate = binaryReader.ReadUInt16();
                                }
                                
                            }

                            if (record.GpsUtcDate != null && record.GpsUtcTime != null)
                            {
                                record.GpsUtcDateTime = _firstDayOf1980.AddDays((double)record.GpsUtcDate).AddMilliseconds((double)record.GpsUtcTime);
                            }
                        }

                        //Some datasets end here
                        if (binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
                        {
                            break;
                        }

                        var numberOfDLVs = binaryReader.ReadByte();
                        //Some datasets end here
                        if (binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
                        {
                            break;
                        }

                        //If the reported number of values does not fit into the stream, correct the numberOfDLVs
                        if (numberOfDLVs > 0)
                        {
                            var endPosition = binaryReader.BaseStream.Position + 5 * numberOfDLVs;
                            if (endPosition > binaryReader.BaseStream.Length)
                            {
                                numberOfDLVs = (byte)Math.Floor((endPosition - binaryReader.BaseStream.Length) / 5d);
                            }
                        }

                        record.SpatialValues = new List<SpatialValue>();

                        //Read DLVs out of the TLG.bin
                        for (int i = 0; i < numberOfDLVs; i++)
                        {
                            var order = binaryReader.ReadByte();
                            var value = binaryReader.ReadInt32();

                            SpatialValue spatialValue = CreateSpatialValue(templateTime, order, value, deviceHierarchies);
                            if(spatialValue != null)
                                record.SpatialValues.Add(spatialValue);
                        }

                        //Add any fixed values from the TLG.xml
                        foreach (ISODataLogValue fixedValue in templateTime.DataLogValues.Where(dlv => dlv.ProcessDataValue.HasValue && !EnumeratedMeterFactory.IsCondensedMeter(dlv.ProcessDataDDI.AsInt32DDI())))
                        {
                            byte order = (byte)templateTime.DataLogValues.IndexOf(fixedValue);
                            if (record.SpatialValues.Any(s => s.Id == order)) //Check to ensure the binary data didn't already write this value
                            {
                                //Per the spec, any fixed value in the XML applies to all rows; as such, replace what was read from the binary
                                SpatialValue matchingValue = record.SpatialValues.Single(s => s.Id == order);
                                matchingValue.DataLogValue = fixedValue;
                            }
                        }

                        yield return record;
                    }
                }
            }

            private static ushort? ReadUShort(double? value, bool specified, System.IO.BinaryReader binaryReader)
            {
                if (specified)
                {
                    if (value.HasValue)
                        return (ushort)value.Value;
                    return binaryReader.ReadUInt16();
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

            private static SpatialValue CreateSpatialValue(ISOTime templateTime, byte order, int value, DeviceElementHierarchies deviceHierarchies)
            {
                var dataLogValues = templateTime.DataLogValues;
                var matchingDlv = dataLogValues.ElementAtOrDefault(order);

                if (matchingDlv == null)
                    return null;

                ISODeviceElement det = deviceHierarchies.GetISODeviceElementFromID(matchingDlv.DeviceElementIdRef);
                ISODevice dvc = det?.Device;
                ISODeviceProcessData dpd = dvc?.DeviceProcessDatas?.FirstOrDefault(d => d.DDI == matchingDlv.ProcessDataDDI);

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
                    DeviceProcessData = dpd
                };

                return spatialValue;
            }
        }
        #endregion Import
    }
}
