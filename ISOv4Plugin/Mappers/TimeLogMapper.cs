/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ITimeLogMapper
    {
        IEnumerable<ISOTimeLog> ExportTimeLogs(IEnumerable<OperationData> operationDatas, string dataPath);
        IEnumerable<OperationData> ImportTimeLogs(ISOTask loggedTask, IEnumerable<ISOTimeLog> timeLogs, int? prescriptionID);
    }

    internal class TimeLogMapper : BaseMapper, ITimeLogMapper
    {
        internal TimeLogMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "TLG")
        {
        }

        private static readonly DateTime _firstDayOf1980 = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Local);

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

                var daysSinceJanOne1980 = (UInt16)(spatialRecord.Timestamp - _firstDayOf1980).TotalDays;
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

        public virtual IEnumerable<OperationData> ImportTimeLogs(ISOTask loggedTask, IEnumerable<ISOTimeLog> timeLogs, int? prescriptionID)
        {
            List<OperationData> operations = new List<OperationData>();
            foreach (ISOTimeLog isoTimeLog in timeLogs)
            {
                IEnumerable<OperationData> operationData = ImportTimeLog(loggedTask, isoTimeLog, prescriptionID);
                if (operationData != null)
                {
                    operations.AddRange(operationData);
                }
            }

            return operations;
        }

        protected IEnumerable<OperationData> ImportTimeLog(ISOTask loggedTask, ISOTimeLog isoTimeLog, int? prescriptionID)
        {
            WorkingDataMapper workingDataMapper = new WorkingDataMapper(new EnumeratedMeterFactory(), TaskDataMapper);
            SectionMapper sectionMapper = new SectionMapper(workingDataMapper, TaskDataMapper);
            bool suppressDataInterpolation;
            if (TaskDataMapper.Properties == null || !bool.TryParse(TaskDataMapper.Properties.GetProperty(TaskDataMapper.SuppressTimeLogDataInterpolation), out suppressDataInterpolation))
            {
                //We interpolate sparse data by default.  One may wish to override this setting to examine the raw state/frequency of the data.
                suppressDataInterpolation = false;
            }
            SpatialRecordMapper spatialMapper = new SpatialRecordMapper(new RepresentationValueInterpolator(suppressDataInterpolation), sectionMapper, workingDataMapper, TaskDataMapper);
            IEnumerable<ISOSpatialRow> isoRecords = ReadTimeLog(isoTimeLog, this.TaskDataPath);
            bool useDeferredExecution = true;
            if (isoRecords != null)
            {
                try
                {
                    if (TaskDataMapper.Properties == null || !bool.TryParse(TaskDataMapper.Properties.GetProperty(TaskDataMapper.SpatialRecordDeferredExecution), out useDeferredExecution))
                    {
                        //Set this property to override the default behavior of deferring execution on the spatial data
                        //We historically pre-iterated this data, giving certain benefits but having negative memory impacts
                        //Going forward the default is to defer execution
                        useDeferredExecution = true;
                    }

                    if (!useDeferredExecution)
                    {
                        isoRecords = isoRecords.ToList(); //Avoids multiple reads
                    }

                    //Set a UTC "delta" from the first record where possible.  We set only one per data import.
                    if (!TaskDataMapper.GPSToLocalDelta.HasValue)
                    {
                        //Find the first record with a valid GPS time and date. A GpsUtcDate of 0x0000 or 0xFFFF indicates an invalid date.
                        var firstRecord = isoRecords.FirstOrDefault(r => r.GpsUtcDateTime.HasValue && r.GpsUtcDate != ushort.MaxValue && r.GpsUtcDate != 0);
                        if (firstRecord != null)
                        {
                            //Local - UTC = Delta.  This value will be rough based on the accuracy of the clock settings
                            // but will expose the ability to derive the UTC times from the exported local times.
                            TimeSpan offset = firstRecord.TimeStart - firstRecord.GpsUtcDateTime.Value;
                            // Round offset to nearest minute for use in timezone offset
                            TaskDataMapper.TimezoneOffset = TimeSpan.FromMinutes(Math.Round(offset.TotalMinutes));
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDataMapper.AddError($"Timelog file {isoTimeLog.Filename} is invalid.  Skipping.", ex.Message, null, ex.StackTrace);
                    return null;
                }
                ISOTime time = GetTimeElementFromTimeLog(isoTimeLog);

                //Identify unique devices represented in this TimeLog data
                List<string> deviceElementIDs = time.DataLogValues.Where(d => d.ProcessDataIntDDI != 0xDFFF && d.ProcessDataIntDDI != 0xDFFE)
                    .Select(d => d.DeviceElementIdRef).Distinct().ToList();

                Dictionary<ISODevice, HashSet<string>> loggedDeviceElementsByDevice = new Dictionary<ISODevice, HashSet<string>>();
                foreach (string deviceElementID in deviceElementIDs)
                {
                    ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies?.GetISODeviceElementFromID(deviceElementID);
                    if (isoDeviceElement != null)
                    {
                        ISODevice device = isoDeviceElement.Device;
                        if (!loggedDeviceElementsByDevice.ContainsKey(device))
                        {
                            loggedDeviceElementsByDevice.Add(device, new HashSet<string>());
                        }
                        loggedDeviceElementsByDevice[device].Add(deviceElementID);

                        //Supplement the list with any parent device elements which although don't log data in the TLG
                        //May require a vrProductIndex working data based on product allocations
                        while (isoDeviceElement.Parent != null &&
                                isoDeviceElement.Parent is ISODeviceElement parentDet)
                        {
                            loggedDeviceElementsByDevice[device].Add(parentDet.DeviceElementId);
                            isoDeviceElement = parentDet;
                        }
                    }
                }

                //Split all devices in the same TimeLog into separate OperationData objects to handle multi-implement scenarios
                //This will ensure implement geometries/DeviceElementUse Depths & Orders do not get confused between implements
                List<OperationData> operationDatas = new List<OperationData>();
                foreach (ISODevice dvc in loggedDeviceElementsByDevice.Keys)
                {
                    //Determine products
                    Dictionary<string, List<ISOProductAllocation>> deviceProductAllocations = GetProductAllocationsByDeviceElement(loggedTask, dvc);

                    //Create a separate operation for each combination of specific product properties.
                    List<List<string>> deviceElementGroups = SplitElementsByProductProperties(deviceProductAllocations, loggedDeviceElementsByDevice[dvc], dvc);

                    foreach (var deviceElementGroup in deviceElementGroups)
                    {
                        ISOOperationData operationData = new ISOOperationData();

                        //Get ids of all device elements in a group including parent element ids
                        //since product allocations can be at parent elements which are not logging any data.
                        var elementHierarchyIds = GetISOElementHierarchyIds(deviceElementGroup);
                        Dictionary<string, List<ISOProductAllocation>> productAllocations = deviceProductAllocations
                            .Where(x => elementHierarchyIds.Contains(x.Key))
                            .ToDictionary(x => x.Key, x => x.Value);
                        List<int> productIDs = GetDistinctProductIDs(TaskDataMapper, productAllocations);

                        //This line will necessarily invoke a spatial read in order to find 
                        //1)The correct number of CondensedWorkState working datas to create 
                        //2)Any Widths and Offsets stored in the spatial data
                        IEnumerable<DeviceElementUse> sections = sectionMapper.Map(time,
                                                                                   isoRecords,
                                                                                   operationData.Id.ReferenceId,
                                                                                   deviceElementGroup,
                                                                                   productAllocations);

                        var workingDatas = sections != null ? sections.SelectMany(x => x.GetWorkingDatas()).ToList() : new List<WorkingData>();

                        operationData.GetSpatialRecords = () => spatialMapper.Map(isoRecords, workingDatas, productAllocations);
                        operationData.MaxDepth = sections.Count() > 0 ? sections.Select(s => s.Depth).Max() : 0;
                        operationData.DeviceElementUses = sectionMapper.ConvertToBaseTypes(sections.ToList());
                        operationData.GetDeviceElementUses = x => operationData.DeviceElementUses.Where(s => s.Depth == x).ToList();
                        operationData.PrescriptionId = prescriptionID;
                        operationData.OperationType = GetOperationType(productIDs, time, workingDatas);
                        operationData.ProductIds = productIDs;
                        if (!useDeferredExecution)
                        {
                            operationData.SpatialRecordCount = isoRecords.Count(); //We will leave this at 0 unless a consumer has overridden deferred execution of spatial data iteration
                        }
                        operationDatas.Add(operationData);
                    }
                }

                //Set the CoincidentOperationDataIds property identifying Operation Datas from the same TimeLog.
                operationDatas.ForEach(o => o.CoincidentOperationDataIds = operationDatas.Where(o2 => o2.Id.ReferenceId != o.Id.ReferenceId).Select(o3 => o3.Id.ReferenceId).ToList());

                return operationDatas;
            }
            return null;
        }

        private List<List<string>> SplitElementsByProductProperties(Dictionary<string, List<ISOProductAllocation>> productAllocations, HashSet<string> loggedDeviceElementIds, ISODevice dvc)
        {
            //This function splits device elements logged by single TimeLog into groups based
            //on product form/type referenced by these elements. This is done using following logic:
            // - determine used products forms and list of device element ids for each form
            // - for each product form determine device elements from all other forms
            // - remove these device elements and their children from a copy of device hierarchy elements
            // - this gives a list of device elements to keep for a product form
            var deviceElementIdsByProductForm = productAllocations
                .SelectMany(x => x.Value.Select(y => new { Product = GetProductByProductAllocation(y), Id = x.Key }))
                .Where(x => x.Product != null)
                .GroupBy(x => new { x.Product.Form, x.Product.ProductType }, x => x.Id)
                .Select(x => x.Distinct().ToList())
                .ToList();

            List<List<string>> deviceElementGroups = new List<List<string>>();
            if (deviceElementIdsByProductForm.Count > 1)
            {
                var deviceHierarchyElement = TaskDataMapper.DeviceElementHierarchies.Items[dvc.DeviceId];

                var idsWithProduct = deviceElementIdsByProductForm.SelectMany(x => x).ToList();
                foreach (var deviceElementIds in deviceElementIdsByProductForm)
                {
                    var idsToRemove = idsWithProduct.Except(deviceElementIds).ToList();
                    var idsToKeep = FilterDeviceElementIds(deviceHierarchyElement, idsToRemove);

                    deviceElementGroups.Add(loggedDeviceElementIds.Intersect(idsToKeep).ToList());
                }
            }
            else
            {
                deviceElementGroups.Add(loggedDeviceElementIds.ToList());
            }

            return deviceElementGroups;
        }

        private Product GetProductByProductAllocation(ISOProductAllocation pan)
        {
            var adaptProductId = TaskDataMapper.InstanceIDMap.GetADAPTID(pan.ProductIdRef);
            var adaptProduct = TaskDataMapper.AdaptDataModel.Catalog.Products.FirstOrDefault(x => x.Id.ReferenceId == adaptProductId);

            // Add an error if ProductAllocation is referencing non-existent product
            if (adaptProduct == null)
            {
                TaskDataMapper.AddError($"ProductAllocation referencing Product={pan.ProductIdRef} skipped since no matching product found");
            }
            return adaptProduct;
        }

        private List<string> FilterDeviceElementIds(DeviceHierarchyElement deviceHierarchyElement, List<string> idsToRemove)
        {
            var elementIdsToKeep = new List<string>();
            if (!idsToRemove.Contains(deviceHierarchyElement.DeviceElement.DeviceElementId))
            {
                //By default we need to keep this element - covers scenario of no children elements
                bool addThisElement = true;
                if (deviceHierarchyElement.Children != null && deviceHierarchyElement.Children.Count > 0)
                {
                    foreach (var c in deviceHierarchyElement.Children)
                    {
                        elementIdsToKeep.AddRange(FilterDeviceElementIds(c, idsToRemove));
                    }
                    //Keep this element if at least one child element is kept
                    addThisElement = elementIdsToKeep.Count > 0;
                }

                if (addThisElement)
                {
                    elementIdsToKeep.Add(deviceHierarchyElement.DeviceElement.DeviceElementId);
                }
            }
            return elementIdsToKeep;
        }

        private List<string> GetISOElementHierarchyIds(List<string> deviceElementIds)
        {
            return deviceElementIds.Aggregate(new { ids = new HashSet<string>(), TaskDataMapper.DeviceElementHierarchies }, (acc, x) =>
            {
                var isoDevElement = acc.DeviceElementHierarchies.GetISODeviceElementFromID(x);
                while (isoDevElement != null)
                {
                    acc.ids.Add(isoDevElement.DeviceElementId);
                    isoDevElement = isoDevElement.Parent as ISODeviceElement;
                }
                return acc;
            }).ids.ToList();
        }

        protected virtual ISOTime GetTimeElementFromTimeLog(ISOTimeLog isoTimeLog)
        {
            return isoTimeLog.GetTimeElement(this.TaskDataPath);
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
            Dictionary<string, Dictionary<string, ISOProductAllocation>> reportedPANs = new Dictionary<string, Dictionary<string, ISOProductAllocation>>();
            int panIndex = 0; // This supports multiple direct PANs for the same DET
            foreach (ISOProductAllocation pan in loggedTask.ProductAllocations.Where(p => !string.IsNullOrEmpty(p.DeviceElementIdRef)))
            {
                ISODeviceElement deviceElement = dvc.DeviceElements.FirstOrDefault(d => d.DeviceElementId == pan.DeviceElementIdRef);
                if (deviceElement != null) //Filter PANs by this DVC
                {
                    // If device element was merged with another one, use it instead
                    var mergedElement = TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(deviceElement.DeviceElementId, true);
                    if (mergedElement != null)
                    {
                        deviceElement = mergedElement.DeviceElement;
                    }
                    AddProductAllocationsForDeviceElement(reportedPANs, pan, deviceElement, $"{GetHierarchyPosition(deviceElement)}_{panIndex}");
                }
                panIndex++;
            }
            // Sort product allocations for each DeviceElement using it's position among ancestors.
            // This arranges PANs on each DET in reverse order: ones from lowest DET in hierarchy having precedence over ones from top.
            Dictionary<string, List<ISOProductAllocation>> output = reportedPANs.ToDictionary(x => x.Key, x =>
            {
                var allocations = x.Value.OrderByDescending(y => y.Key).Select(y => y.Value).ToList();
                // Check if there are any indirect allocations: ones that came from parent device element
                var indirectAllocations = allocations.Where(y => y.DeviceElementIdRef != x.Key).ToList();
                if (indirectAllocations.Count > 0 && indirectAllocations.Count != allocations.Count)
                {
                    // Only keep direct allocations
                    allocations = allocations.Except(indirectAllocations).ToList();
                }
                return allocations;
            });

            // Determine the lowest depth at which product allocations are reported to eliminate any duplicate PANs
            // at multiple levels within the hierarchy
            DeviceHierarchyElement det = reportedPANs.Keys
                .Select(x => TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(x))
                .Where(x => x != null)
                .FirstOrDefault();

            var rootElement = det?.GetRootDeviceElementHierarchy();
            int lowestLevel = GetLowestProductAllocationLevel(rootElement, output);
            var elementAtLowestDepth = rootElement?.GetElementsAtDepth(lowestLevel).FirstOrDefault();

            // Keep allocations for lowest level or for elements of the same type and without children.
            // This handles scenario where device hierarchy for different products have different lengths:
            // - one with 4 levels and Unit device element at the lowest level
            // - one with 3 levels and Unit device element at the lowest level
            return output
                .Where(x =>
                {
                    var matchingElement = TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(x.Key);
                    if (matchingElement == null)
                    {
                        return false;
                    }
                    if (matchingElement.Depth == lowestLevel)
                    {
                        return true;
                    }
                    return matchingElement.Type == elementAtLowestDepth?.Type &&
                           (matchingElement.Children == null || matchingElement.Children.Count == 0);
                })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private int GetLowestProductAllocationLevel(DeviceHierarchyElement isoDeviceElementHierarchy, Dictionary<string, List<ISOProductAllocation>> isoProductAllocations)
        {
            int level = -1;
            // If device element or any merged device elements have direct product allocations, use its Depth.
            if (isoDeviceElementHierarchy != null &&
                isoProductAllocations.TryGetValue(isoDeviceElementHierarchy.DeviceElement.DeviceElementId, out List<ISOProductAllocation> productAllocations))
            {
                var deviceElementIds = new List<string> { isoDeviceElementHierarchy.DeviceElement.DeviceElementId };
                deviceElementIds.AddRange(isoDeviceElementHierarchy.MergedElements.Select(x => x.DeviceElementId));
                if (productAllocations.Any(x => deviceElementIds.Contains(x.DeviceElementIdRef)))
                {
                    level = isoDeviceElementHierarchy.Depth;
                }
            }

            // Get max level from children elements
            int? maxChildLevel = isoDeviceElementHierarchy?.Children?.Max(x => GetLowestProductAllocationLevel(x, isoProductAllocations));

            return Math.Max(level, maxChildLevel.GetValueOrDefault(-1));
        }

        private int GetHierarchyPosition(ISODeviceElement deviceElement)
        {
            int position = 0;

            while (deviceElement != null)
            {
                deviceElement = deviceElement.Parent as ISODeviceElement;
                position++;
            }
            return position;
        }

        private void AddProductAllocationsForDeviceElement(Dictionary<string, Dictionary<string, ISOProductAllocation>> productAllocations, ISOProductAllocation pan, ISODeviceElement deviceElement, string hierarchyPoistion)
        {
            if (!productAllocations.ContainsKey(deviceElement.DeviceElementId))
            {
                productAllocations.Add(deviceElement.DeviceElementId, new Dictionary<string, ISOProductAllocation>());
            }

            productAllocations[deviceElement.DeviceElementId][hierarchyPoistion] = pan;

            foreach (ISODeviceElement child in deviceElement.ChildDeviceElements)
            {
                AddProductAllocationsForDeviceElement(productAllocations, pan, child, hierarchyPoistion);
            }
        }

        private OperationTypeEnum GetOperationType(List<int> productIds, ISOTime time, List<WorkingData> workingDatas)
        {
            var productCategories = productIds
                .Select(x => TaskDataMapper.AdaptDataModel.Catalog.Products.FirstOrDefault(y => y.Id.ReferenceId == x))
                .Where(x => x != null && x.Category != CategoryEnum.Unknown)
                .Select(x => x.Category)
                .ToList();

            var deviceOperationType = GetOperationTypeFromLoggingDevices(time);

            // Prefer product category to determine operation type where possible
            switch (productCategories.FirstOrDefault())
            {
                case CategoryEnum.Variety:
                    // It's technically an error to log Harvesting as a variety product,
                    // but this was observed by Ag Leader in a CNH Pro1200 file in August 2025
                    // (see https://github.com/ADAPT/ISOv4Plugin/pull/256)
                    if (deviceOperationType == OperationTypeEnum.Harvesting)
                    {
                        return deviceOperationType;
                    }

                    return OperationTypeEnum.SowingAndPlanting;

                case CategoryEnum.Fertilizer:
                case CategoryEnum.NitrogenStabilizer:
                case CategoryEnum.Manure:
                    return OperationTypeEnum.Fertilizing;

                case CategoryEnum.Fungicide:
                case CategoryEnum.Herbicide:
                case CategoryEnum.Insecticide:
                case CategoryEnum.Pesticide:
                    return OperationTypeEnum.CropProtection;

                default:
                    //Harvest/ForageHarvest omitted intentionally to be determined from machine type vs. working data
                    if (workingDatas.Any(w => w.Representation.ContainsCode("Seed")))
                    {
                        return OperationTypeEnum.SowingAndPlanting;
                    }
                    if (workingDatas.Any(w => w.Representation.ContainsCode("Tillage")))
                    {
                        return OperationTypeEnum.Tillage;
                    }
                    if (workingDatas.Any(w => w.Representation.ContainsCode("AppRate")))
                    {
                        if (deviceOperationType != OperationTypeEnum.Fertilizing && deviceOperationType != OperationTypeEnum.CropProtection)
                        {
                            return OperationTypeEnum.Unknown; // We can't differentiate CropProtection from Fertilizing, but prefer unknown to letting implement type set to SowingAndPlanting
                        }
                    }
                    return deviceOperationType;
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

            DeviceOperationType deviceType = representedTypes.FirstOrDefault(t => t.ClientNAMEMachineType >= 2 && t.ClientNAMEMachineType <= 11 &&
                t.OperationType != OperationTypeEnum.Unknown);

            if (deviceType != null)
            {
                //2-11 represent known types of operations
                //These will map to implement devices and will govern the actual operation type.
                //Return the first such device type
                return deviceType.OperationType;
            }
            return OperationTypeEnum.Unknown;
        }

        protected virtual IEnumerable<ISOSpatialRow> ReadTimeLog(ISOTimeLog timeLog, string dataPath)
        {
            ISOTime templateTime = timeLog.GetTimeElement(dataPath);
            string binName = string.Concat(timeLog.Filename, ".bin");
            string filePath = dataPath.GetDirectoryFiles(binName, SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (templateTime != null && filePath != null)
            {
                return BinaryReader.Read(filePath, templateTime, TaskDataMapper.DeviceElementHierarchies, TaskDataMapper.Version);
            }
            return null;
        }

        internal static Dictionary<byte, int> ReadImplementGeometryValues(IEnumerable<byte> dlvsToRead, ISOTime templateTime, string filePath, int version, IList<IError> errors)
        {
            return BinaryReader.ReadImplementGeometryValues(filePath, templateTime, dlvsToRead, version, errors);
        }

        protected class BinaryReader
        {
            public static Dictionary<byte, int> ReadImplementGeometryValues(string filePath, ISOTime templateTime, IEnumerable<byte> desiredDLVIndices, int version, IList<IError> errors)
            {
                Dictionary<byte, int> output = new Dictionary<byte, int>();
                List<byte> desiredIndexes = desiredDLVIndices.ToList();

                //Determine the number of header bytes in each position
                short headerCount = 0;
                bool overrideTimelogAttributeChecks = DetermineTimelogAttributeValidity(filePath, version);
                SkipBytes(overrideTimelogAttributeChecks || (templateTime.HasStart && templateTime.Start == null), 6, ref headerCount);
                ISOPosition templatePosition = templateTime.Positions.FirstOrDefault();
                
                if (templatePosition != null)
                {
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasPositionNorth && templatePosition.PositionNorth == null), 4, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasPositionEast && templatePosition.PositionEast == null), 4, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasPositionUp && templatePosition.PositionUp == null), 4, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasPositionStatus && templatePosition.PositionStatus == null), 1, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasPDOP && templatePosition.PDOP == null), 2, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasHDOP && templatePosition.HDOP == null), 2, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasNumberOfSatellites && templatePosition.NumberOfSatellites == null), 1, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasGpsUtcTime && templatePosition.GpsUtcTime == null), 4, ref headerCount);
                    SkipBytes(overrideTimelogAttributeChecks || (templatePosition.HasGpsUtcDate && templatePosition.GpsUtcDate == null), 2, ref headerCount);
                }

                using (var binaryReader = new System.IO.BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    while (ContinueReading(binaryReader))
                    {
                        binaryReader.BaseStream.Position += headerCount; //Skip over the header
                        if (ContinueReading(binaryReader))
                        {
                            var numberOfDLVs = ReadByte(null, true, false, binaryReader).GetValueOrDefault(0);
                            if (ContinueReading(binaryReader))
                            {
                                numberOfDLVs = ConfirmNumberOfDLVs(binaryReader, numberOfDLVs); //Validate we are not at the end of a truncated file
                                for (byte i = 0; i < numberOfDLVs; i++)
                                {
                                    byte dlvIndex = ReadByte(null, true, false, binaryReader).GetValueOrDefault(); //This is the current DLV reported
                                    if (desiredIndexes.Contains(dlvIndex))
                                    {
                                        //A desired DLV is reported here
                                        int value = ReadInt32(null, true, false, binaryReader).GetValueOrDefault();
                                        try
                                        {
                                            if (!output.ContainsKey(dlvIndex))
                                            {
                                                output.Add(dlvIndex, value);
                                            }
                                            else if (Math.Abs(value) > Math.Abs(output[dlvIndex]))
                                            {
                                                //Values should be all the same, but prefer the furthest from 0
                                                output[dlvIndex] = value;
                                            }
                                        }
                                        catch (OverflowException ex)
                                        {
                                            // If value == int.MinValue, Math.Abs(value) will throw System.OverflowException: Negating the minimum value of a twos complement number is invalid.
                                            errors.Add(new Error() { Description = ex.Message, Id = ex.GetType().ToString(), Source = ex.Source, StackTrace = ex.StackTrace });
                                        }
                                    }
                                    else
                                    {
                                        binaryReader.BaseStream.Position += 4;
                                    }
                                }
                            }
                        }

                    }

                }
                return output;
            }

            private static void SkipBytes(bool hasValue, short byteRange, ref short skipCount)
            {
                if (hasValue)
                {
                    skipCount += byteRange;
                }
            }

            private static bool ContinueReading(System.IO.BinaryReader binaryReader)
            {
                return binaryReader.BaseStream.Position < binaryReader.BaseStream.Length;
            }

            private static byte ConfirmNumberOfDLVs(System.IO.BinaryReader binaryReader, byte numberOfDLVs)
            {
                if (numberOfDLVs > 0)
                {
                    var endPosition = binaryReader.BaseStream.Position + 5 * numberOfDLVs;
                    if (endPosition > binaryReader.BaseStream.Length)
                    {
                        numberOfDLVs = (byte)Math.Floor((binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) / 5d);
                    }
                }
                return numberOfDLVs;
            }

            private static bool DetermineTimelogAttributeValidity(string fileName, int version)
            {
                bool overrideTimelogAttributeChecks = false;
                if (version < 3)
                {
                    //Some early datasets have a misinterpretation of the "template" behavior of the TIM & PTN elements in the TLG.XML files, 
                    //and all GPS data elements are reported (often as 0s) regardless of the TLG.XML.
                    //Run a quick check to see if this is such a dataset (or if all GPS header attributes are legitimately populated)
                    //to override the attribute-prescence logic in reading the binary.
                    using (var binaryReader = new System.IO.BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        if (binaryReader.BaseStream.Length > 31) //Guard against small files failing on this logic
                        {
                            //First record
                            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
                            var firstDaysSince1980 = binaryReader.ReadUInt16();
                            binaryReader.BaseStream.Seek(24, SeekOrigin.Current);
                            var firstDLVCount = binaryReader.ReadByte();

                            var firstRecordDataByteCount = firstDLVCount * 5; //1 byte of id + 4 bytes of value for each dlv
                            if (binaryReader.BaseStream.Length > 31 + firstRecordDataByteCount + 6) //Guard against small files failing on this logic
                            {
                                binaryReader.BaseStream.Seek(firstRecordDataByteCount, SeekOrigin.Current);

                                //Second record
                                binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
                                var secondDaysSince1980 = binaryReader.ReadUInt16();
                                if (firstDaysSince1980 == secondDaysSince1980)
                                {
                                    //The byte offsets suggest all header data is present
                                    overrideTimelogAttributeChecks = true;
                                }
                            }
                        }
                    }
                }
                return overrideTimelogAttributeChecks;
            }

            public static IEnumerable<ISOSpatialRow> Read(string fileName, ISOTime templateTime, DeviceElementHierarchies deviceHierarchies, int version)
            {
                if (templateTime == null)
                    yield break;

                if (!File.Exists(fileName))
                    yield break;

                bool overrideTimelogAttributeChecks = DetermineTimelogAttributeValidity(fileName, version);
                using (var binaryReader = new System.IO.BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        ISOPosition templatePosition = templateTime.Positions.FirstOrDefault();

                        var record = new ISOSpatialRow { TimeStart = GetStartTime(templateTime, binaryReader, overrideTimelogAttributeChecks).GetValueOrDefault() };

                        if (templatePosition != null)
                        {
                            //North and East are required binary data
                            record.NorthPosition = ReadInt32((double?)templatePosition.PositionNorth, templatePosition.HasPositionNorth, overrideTimelogAttributeChecks, binaryReader).GetValueOrDefault(0);
                            record.EastPosition = ReadInt32((double?)templatePosition.PositionEast, templatePosition.HasPositionEast, overrideTimelogAttributeChecks, binaryReader).GetValueOrDefault(0);

                            //Optional position attributes will be included in the binary only if a corresponding attribute is present in the PTN element
                            record.Elevation = ReadInt32(templatePosition.PositionUp, templatePosition.HasPositionUp, overrideTimelogAttributeChecks, binaryReader);

                            //Position status is required
                            record.PositionStatus = ReadByte((byte?)templatePosition.PositionStatus, templatePosition.HasPositionStatus, overrideTimelogAttributeChecks, binaryReader);

                            record.PDOP = ReadUShort((double?)templatePosition.PDOP, templatePosition.HasPDOP, overrideTimelogAttributeChecks, binaryReader);

                            record.HDOP = ReadUShort((double?)templatePosition.HDOP, templatePosition.HasHDOP, overrideTimelogAttributeChecks, binaryReader);

                            record.NumberOfSatellites = ReadByte(templatePosition.NumberOfSatellites, templatePosition.HasNumberOfSatellites, overrideTimelogAttributeChecks, binaryReader);

                            record.GpsUtcTime = ReadUInt32(templatePosition.GpsUtcTime, templatePosition.HasGpsUtcTime, overrideTimelogAttributeChecks, binaryReader).GetValueOrDefault();

                            record.GpsUtcDate = ReadUShort(templatePosition.GpsUtcDate, templatePosition.HasGpsUtcDate, overrideTimelogAttributeChecks, binaryReader);

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

                        var numberOfDLVs = ReadByte(null, true, false, binaryReader).GetValueOrDefault(0);
                        // There should be some values but no more data exists in file, stop processing
                        if (numberOfDLVs > 0 && binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
                        {
                            break;
                        }

                        //If the reported number of values does not fit into the stream, correct the numberOfDLVs
                        numberOfDLVs = ConfirmNumberOfDLVs(binaryReader, numberOfDLVs);

                        record.SpatialValues = new List<SpatialValue>(numberOfDLVs);

                        bool unexpectedEndOfStream = false;
                        //Read DLVs out of the TLG.bin
                        for (int i = 0; i < numberOfDLVs; i++)
                        {
                            var order = ReadByte(null, true, false, binaryReader).GetValueOrDefault();
                            var value = ReadInt32(null, true, false, binaryReader).GetValueOrDefault();
                            // Can't read either order or value or both, stop processing
                            if (i < numberOfDLVs - 1 && binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
                            {
                                unexpectedEndOfStream = true;
                                break;
                            }

                            SpatialValue spatialValue = CreateSpatialValue(templateTime, order, value, deviceHierarchies);
                            if (spatialValue != null)
                            {
                                record.SpatialValues.Add(spatialValue);
                            }
                        }
                        // Unable to read some of the expected DLVs, stop processing
                        if (unexpectedEndOfStream)
                        {
                            break;
                        }

                        //Add any fixed values from the TLG.xml
                        foreach (ISODataLogValue fixedValue in templateTime.DataLogValues.Where(dlv => dlv.ProcessDataValue.HasValue && !EnumeratedMeterFactory.IsCondensedMeter(dlv.ProcessDataIntDDI)))
                        {
                            byte order = (byte)templateTime.DataLogValues.IndexOf(fixedValue);
                            SpatialValue matchingValue = record.SpatialValues.FirstOrDefault(s => s.Id == order);
                            if (matchingValue != null) //Check to ensure the binary data didn't already write this value
                            {
                                //Per the spec, any fixed value in the XML applies to all rows; as such, replace what was read from the binary
                                matchingValue.DataLogValue = fixedValue;
                            }
                        }

                        yield return record;
                    }
                }
            }

            private static ushort? ReadUShort(double? value, bool specified, bool overrideTemplate, System.IO.BinaryReader binaryReader)
            {
                if (specified || overrideTemplate)
                {
                    if (value.HasValue && !overrideTemplate)
                        return (ushort)value.Value;

                    var buffer = new byte[2];
                    var actualSize = binaryReader.Read(buffer, 0, buffer.Length);
                    return actualSize != buffer.Length ? null : (ushort?)BitConverter.ToUInt16(buffer, 0);
                }
                return null;          
            }

            private static byte? ReadByte(byte? byteValue, bool specified, bool overrideTemplate, System.IO.BinaryReader binaryReader)
            {  
                if (specified || overrideTemplate)
                {
                    if (byteValue.HasValue && !overrideTemplate)
                        return byteValue;

                    var buffer = new byte[1];
                    var actualSize = binaryReader.Read(buffer, 0, buffer.Length);
                    return actualSize != buffer.Length ? null : (byte?)buffer[0];
                }
                return null;
            }

            private static int? ReadInt32(double? d, bool specified, bool overrideTemplate, System.IO.BinaryReader binaryReader)
            {
                if (specified || overrideTemplate)
                {
                    if (d.HasValue && !overrideTemplate)
                        return (int)d.Value;

                    var buffer = new byte[4];
                    var actualSize = binaryReader.Read(buffer, 0, buffer.Length);
                    return actualSize != buffer.Length ? null : (int?)BitConverter.ToInt32(buffer, 0);
                }
                return null;
            }

            private static uint? ReadUInt32(double? d, bool specified, bool overrideTemplate, System.IO.BinaryReader binaryReader)
            {
                if (specified || overrideTemplate)
                {
                    if (d.HasValue && !overrideTemplate)
                        return (uint)d.Value;

                    var buffer = new byte[4];
                    var actualSize = binaryReader.Read(buffer, 0, buffer.Length);
                    return actualSize != buffer.Length ? null : (uint?)BitConverter.ToUInt32(buffer, 0);
                }
                return null;
            }

            private static DateTime? GetStartTime(ISOTime templateTime, System.IO.BinaryReader binaryReader, bool overrideTemplate)
            {
                if (overrideTemplate || (templateTime.HasStart && templateTime.Start == null))
                {
                    var milliseconds = ReadInt32(null, true, overrideTemplate, binaryReader);
                    var daysFrom1980 = ReadUShort(null, true, overrideTemplate, binaryReader);
                    return !milliseconds.HasValue || !daysFrom1980.HasValue ? null : (DateTime?)_firstDayOf1980.AddDays(daysFrom1980.Value).AddMilliseconds(milliseconds.Value);
                }
                else if (templateTime.HasStart)
                    return templateTime.Start;

                return _firstDayOf1980;
            }

            private static SpatialValue CreateSpatialValue(ISOTime templateTime, byte order, int value, DeviceElementHierarchies deviceHierarchies)
            {
                var dataLogValues = templateTime.DataLogValues;
                var matchingDlv = dataLogValues.ElementAtOrDefault(order);

                if (matchingDlv == null)
                    return null;

                ISODeviceElement det = deviceHierarchies?.GetISODeviceElementFromID(matchingDlv.DeviceElementIdRef);
                ISODevice dvc = det?.Device;
                ISODeviceProcessData dpd = dvc?.FirstOrDefaultDeviceProcessData(matchingDlv.ProcessDataIntDDI);

                var ddis = DdiLoader.Ddis;

                var resolution = 1d;
                if (matchingDlv.ProcessDataDDI != null && ddis.ContainsKey(matchingDlv.ProcessDataIntDDI))
                {
                    resolution = ddis[matchingDlv.ProcessDataIntDDI].Resolution;
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
