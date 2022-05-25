/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Mappers.Factories;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ITaskMapper
    {
        IEnumerable<ISOTask> Export(IEnumerable<WorkItem> adaptWorkItems, int isoGridType);
        IEnumerable<ISOTask> Export(IEnumerable<LoggedData> adaptLoggedDatas);
        IEnumerable<WorkItem> ImportWorkItems(IEnumerable<ISOTask> isoFarms);
        IEnumerable<LoggedData> ImportLoggedDatas(IEnumerable<ISOTask> isoFarms);
    }

    public class TaskMapper : BaseMapper, ITaskMapper
    {
        private Dictionary<string, int> _rxIDsByTask;
        private Dictionary<int, string> _taskIDsByPrescription;
        public TaskMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "TSK")
        {
            _rxIDsByTask = new Dictionary<string, int>();
            _taskIDsByPrescription = new Dictionary<int, string>();
        }

        PrescriptionMapper _prescriptionMapper;
        public PrescriptionMapper PrescriptionMapper
        {
            get
            {
                if (_prescriptionMapper == null)
                {
                    _prescriptionMapper = new PrescriptionMapper(TaskDataMapper, ConnectionMapper);
                }
                return _prescriptionMapper;
            }
        }

        TimeLogMapperFactory _timeLogFactoryMapper;
        public TimeLogMapperFactory TimeLogMapperFactory
        {
            get
            {
                if (_timeLogFactoryMapper == null)
                {
                    _timeLogFactoryMapper = new TimeLogMapperFactory(TaskDataMapper);
                }
                return _timeLogFactoryMapper;
            }
        }

        ConnectionMapper _connectionMapper;
        public ConnectionMapper ConnectionMapper
        {
            get
            {
                if (_connectionMapper == null)
                {
                    _connectionMapper = new ConnectionMapper(TaskDataMapper);
                }
                return _connectionMapper;
            }
        }

        #region Export

        #region Export Work Items
        public IEnumerable<ISOTask> Export(IEnumerable<WorkItem> adaptWorkItems, int isoGridType)
        {
            List<ISOTask> tasks = new List<ISOTask>();
            foreach (WorkItem workItem in adaptWorkItems)
            {
                IEnumerable<ISOTask> newTasks = Export(workItem, isoGridType);
                tasks.AddRange(newTasks);
            }
            return tasks;
        }

        private IEnumerable<ISOTask> Export(WorkItem workItem, int isoGridType)
        {
            List<ISOTask> tasks = new List<ISOTask>();

            if (workItem.WorkItemOperationIds.Any())
            {
                foreach (int operationID in workItem.WorkItemOperationIds)
                {
                    WorkItemOperation operation = DataModel.Documents.WorkItemOperations.FirstOrDefault(o => o.Id.ReferenceId == operationID);
                    if (operation != null && operation.PrescriptionId.HasValue)
                    {
                        Prescription prescription = DataModel.Catalog.Prescriptions.FirstOrDefault(p => p.Id.ReferenceId == operation.PrescriptionId.Value);
                        if (prescription != null)
                        {
                            ISOTask task = PrescriptionMapper.ExportPrescription(workItem, isoGridType, prescription);
                            tasks.Add(task);
                            _taskIDsByPrescription.Add(prescription.Id.ReferenceId, task.TaskID);
                        }
                    }
                }
            }
            return tasks;
        }

        #endregion Export Work Items

        #region Export Logged Data

        public IEnumerable<ISOTask> Export(IEnumerable<LoggedData> adaptLoggedDatas)
        {
            List<ISOTask> tasks = new List<ISOTask>();
            foreach (LoggedData loggedData in adaptLoggedDatas)
            {
                ISOTask task = Export(loggedData);
                tasks.Add(task);
            }
            return tasks;
        }

        private ISOTask Export(LoggedData loggedData)
        {
            ISOTask task = null;

            //Try to map to a pre-existing Work Item task where appropriate
            if (loggedData.OperationData.All(o => o.PrescriptionId.HasValue) &&
                loggedData.OperationData.Select(o => o.PrescriptionId.Value).Distinct().Count() == 1)
            {
                int rxID = loggedData.OperationData.First().PrescriptionId.Value;
                if (_taskIDsByPrescription.ContainsKey(rxID))
                {
                    task = ISOTaskData.ChildElements.OfType<ISOTask>().FirstOrDefault(t => t.TaskID == _taskIDsByPrescription[rxID]);
                }
            }

            if (task == null)
            {
                task = new ISOTask(TaskDataMapper.Version);

                //Task ID
                string taskID = loggedData.Id.FindIsoId() ?? GenerateId();
                task.TaskID = taskID;
            }


            if (!ExportIDs(loggedData.Id, task.TaskID))
            {
                //Update the mapping to represent the completed task
                TaskDataMapper.InstanceIDMap.ReplaceADAPTID(task.TaskID, loggedData.Id.ReferenceId);
            }

            //Task Designator
            task.TaskDesignator = loggedData.Description;

            //Customer Ref
            if (loggedData.GrowerId.HasValue)
            {
                task.CustomerIdRef = TaskDataMapper.InstanceIDMap.GetISOID(loggedData.GrowerId.Value);
            }

            //Farm Ref
            if (loggedData.FarmId.HasValue)
            {
                task.FarmIdRef = TaskDataMapper.InstanceIDMap.GetISOID(loggedData.FarmId.Value);
            }

            //Partfield Ref
            if (loggedData.CropZoneId.HasValue)
            {
                task.PartFieldIdRef = TaskDataMapper.InstanceIDMap.GetISOID(loggedData.CropZoneId.Value);
            }
            else if (loggedData.FieldId.HasValue)
            {
                task.PartFieldIdRef = TaskDataMapper.InstanceIDMap.GetISOID(loggedData.FieldId.Value);
            }

            //Status
            task.TaskStatus = ISOEnumerations.ISOTaskStatus.Completed;

            if (loggedData.OperationData.Any())
            {
                //Time Logs
                task.TimeLogs = TimeLogMapperFactory.ExportTimeLogs(loggedData.OperationData, TaskDataPath).ToList();

                //Connections
                IEnumerable<int> taskEquipmentConfigIDs = loggedData.OperationData.SelectMany(o => o.EquipmentConfigurationIds);
                if (taskEquipmentConfigIDs.Any())
                {
                    IEnumerable<EquipmentConfiguration> taskEquipmentConfigs = DataModel.Catalog.EquipmentConfigurations.Where(d => taskEquipmentConfigIDs.Contains(d.Id.ReferenceId));
                    task.Connections = ConnectionMapper.ExportConnections(task, taskEquipmentConfigs).ToList();
                }
            }

            //Summaries
            if (loggedData.SummaryId.HasValue)
            {
                Summary summary = DataModel.Documents.Summaries.FirstOrDefault(s => s.Id.ReferenceId == loggedData.SummaryId.Value);
                if (summary != null)
                {
                    task.Times.AddRange(ExportSummary(summary));
                }

                List<ISOProductAllocation> productAllocations = GetProductAllocationsForSummary(summary);
                if (productAllocations != null)
                {
                    task.ProductAllocations.AddRange(productAllocations);
                }
            }

            //Comments
            if (loggedData.Notes.Any())
            {
                CommentAllocationMapper canMapper = new CommentAllocationMapper(TaskDataMapper);
                task.CommentAllocations = canMapper.ExportCommentAllocations(loggedData.Notes).ToList();
            }

            //Worker Allocations
            if (loggedData.PersonRoleIds.Any())
            {
                WorkerAllocationMapper workerAllocationMapper = new WorkerAllocationMapper(TaskDataMapper);
                List<PersonRole> personRoles = new List<PersonRole>();
                foreach (int id in loggedData.PersonRoleIds)
                {
                    PersonRole personRole = DataModel.Catalog.PersonRoles.FirstOrDefault(p => p.Id.ReferenceId == id);
                    if (personRole != null)
                    {
                        personRoles.Add(personRole);
                    }
                }
                task.WorkerAllocations = workerAllocationMapper.ExportWorkerAllocations(personRoles).ToList();
            }

            //Guidance Allocations
            if (loggedData.GuidanceAllocationIds.Any())
            {
                GuidanceAllocationMapper guidanceAllocationMapper = new GuidanceAllocationMapper(TaskDataMapper);
                List<GuidanceAllocation> allocations = new List<GuidanceAllocation>();
                foreach (int id in loggedData.GuidanceAllocationIds)
                {
                    GuidanceAllocation allocation = DataModel.Documents.GuidanceAllocations.FirstOrDefault(p => p.Id.ReferenceId == id);
                    if (allocation != null)
                    {
                        allocations.Add(allocation);
                    }
                }
                task.GuidanceAllocations = guidanceAllocationMapper.ExportGuidanceAllocations(allocations).ToList();
            }

            return task;
        }

        private List<ISOTime> ExportSummary(Summary summary)
        {
            List<ISOTime> times = new List<ISOTime>();
            foreach (StampedMeteredValues values in summary.SummaryData)
            {
                ISOTime time = new ISOTime();
                time.Start = values.Stamp.TimeStamp1;
                time.Stop = values.Stamp.TimeStamp2;
                if (values.Stamp.Duration.HasValue)
                {
                    time.Duration = (uint)values.Stamp.Duration.Value.TotalSeconds;
                }
                time.Type = values.Stamp.DateContext == DateContextEnum.ProposedStart ? ISOEnumerations.ISOTimeType.Planned : ISOEnumerations.ISOTimeType.Effective;

                foreach (MeteredValue value in values.Values)
                {
                    if (value.Value != null && value.Value is NumericRepresentationValue)
                    {
                        NumericRepresentationValue numericValue = value.Value as NumericRepresentationValue;
                        ISODataLogValue dlv = new ISODataLogValue();
                        int? ddi = RepresentationMapper.Map(numericValue.Representation);
                        if (ddi.HasValue)
                        {
                            dlv.ProcessDataDDI = ddi.Value.AsHexDDI();
                            DdiDefinition DdiDefinition = DDIs[ddi.Value];
                            dlv.ProcessDataValue = (int)(numericValue.Value.Value / (DdiDefinition.Resolution != 0 ? DdiDefinition.Resolution : 1d));
                        }
                        else
                        {
                            if (numericValue.Representation.CodeSource == RepresentationCodeSourceEnum.ISO11783_DDI)
                            {
                                dlv.ProcessDataDDI = numericValue.Representation.Code;
                                dlv.ProcessDataValue = (int)numericValue.Value.Value;
                            }
                        }
                        if (value.DeviceConfigurationId.HasValue)
                        {
                            DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(c => c.Id.ReferenceId == value.DeviceConfigurationId.Value);
                            if (config != null)
                            {
                                dlv.DeviceElementIdRef = TaskDataMapper.InstanceIDMap.GetISOID(config.DeviceElementId);
                            }
                        }
                        time.DataLogValues.Add(dlv);
                    }
                }
                times.Add(time);
            }
            return times;
        }

        private List<ISOProductAllocation> GetProductAllocationsForSummary(Summary summary)
        {
            List<ISOProductAllocation> productAllocations = null;
            if (summary.OperationSummaries != null && summary.OperationSummaries.Any())
            {
                productAllocations = new List<ISOProductAllocation>();
                foreach (OperationSummary operationSummary in summary.OperationSummaries)
                {
                    foreach (StampedMeteredValues values in operationSummary.Data)
                    {
                        ISOProductAllocation pan = new ISOProductAllocation();
                        pan.AllocationStamp = AllocationStampMapper.ExportAllocationStamp(values.Stamp);
                        pan.ProductIdRef = TaskDataMapper.InstanceIDMap.GetISOID(operationSummary.ProductId);
                        productAllocations.Add(pan);
                    }
                }
            }
            return productAllocations;
        }

        #endregion Export LoggedData

        #endregion Export

        #region Import

        #region Import WorkItems

        public IEnumerable<WorkItem> ImportWorkItems(IEnumerable<ISOTask> isoPrescribedTasks)
        {
            List<WorkItem> adaptWorkItems = new List<WorkItem>();
            foreach (ISOTask isoPrescribedTask in isoPrescribedTasks)
            {
                WorkItem workItem = ImportWorkItem(isoPrescribedTask);
                adaptWorkItems.Add(workItem);
            }
            return adaptWorkItems;
        }

        private WorkItem ImportWorkItem(ISOTask isoPrescribedTask)
        {
            WorkItem workItem = new WorkItem();

            //Task ID
            ImportIDs(workItem.Id, isoPrescribedTask.TaskID);

            //Grower ID
            workItem.GrowerId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoPrescribedTask.CustomerIdRef);

            //Farm ID
            workItem.FarmId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoPrescribedTask.FarmIdRef);

            //Field/CropZone
            int? pfdID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoPrescribedTask.PartFieldIdRef);
            if (pfdID.HasValue)
            {
                if (DataModel.Catalog.CropZones.Any(c => c.Id.ReferenceId == pfdID.Value))
                {
                    workItem.CropZoneId = pfdID.Value;
                }
                else
                {
                    workItem.FieldId = pfdID.Value;
                    if (DataModel.Catalog.CropZones.Count(c => c.FieldId == pfdID) == 1)
                    {
                        //There is a single cropZone for the field.  
                        workItem.CropZoneId = DataModel.Catalog.CropZones.Single(c => c.FieldId == pfdID).Id.ReferenceId;
                    }
                }
            }

            //Status
            workItem.StatusUpdates = new List<StatusUpdate>() { new StatusUpdate() { Status = ImportStatus(isoPrescribedTask.TaskStatus) } };

            //Responsible Worker
            if (!string.IsNullOrEmpty(isoPrescribedTask.ResponsibleWorkerIdRef))
            {
                ISOWorker worker = ISOTaskData.ChildElements.OfType<ISOWorker>().FirstOrDefault(w => w.WorkerId == isoPrescribedTask.ResponsibleWorkerIdRef);
                int? personID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoPrescribedTask.ResponsibleWorkerIdRef);
                if (personID.HasValue)
                {
                    //Create a Role
                    PersonRole role = new PersonRole() { PersonId = personID.Value };

                    //Add to Catalog
                    DataModel.Catalog.PersonRoles.Add(role);
                    if (workItem.PeopleRoleIds == null)
                    {
                        workItem.PeopleRoleIds = new List<int>();
                    }

                    //Add to Task
                    workItem.PeopleRoleIds.Add(role.Id.ReferenceId);
                }
            }

            //Worker Allocation
            if (isoPrescribedTask.WorkerAllocations.Any())
            {               
                WorkerAllocationMapper wanMapper = new WorkerAllocationMapper(TaskDataMapper);
                List<PersonRole> personRoles = wanMapper.ImportWorkerAllocations(isoPrescribedTask.WorkerAllocations).ToList();

                //Add to Catalog
                DataModel.Catalog.PersonRoles.AddRange(personRoles);

                //Add to Task
                if (workItem.PeopleRoleIds == null)
                {
                    workItem.PeopleRoleIds = new List<int>();
                }
                workItem.PeopleRoleIds.AddRange(personRoles.Select(p => p.Id.ReferenceId));
            }

            if (isoPrescribedTask.GuidanceAllocations.Any())
            {
                GuidanceAllocationMapper ganMapper = new GuidanceAllocationMapper(TaskDataMapper);
                List<GuidanceAllocation> allocations = ganMapper.ImportGuidanceAllocations(isoPrescribedTask.GuidanceAllocations).ToList();

                //Add to Catalog
                List<GuidanceAllocation> guidanceAllocations = DataModel.Documents.GuidanceAllocations as List<GuidanceAllocation>;
                if (guidanceAllocations != null)
                {
                    guidanceAllocations.AddRange(allocations);
                }

                //Add to Task
                if (workItem.GuidanceAllocationIds == null)
                {
                    workItem.GuidanceAllocationIds = new List<int>();
                }
                workItem.GuidanceAllocationIds.AddRange(allocations.Select(p => p.Id.ReferenceId));
            }

            //Comments
            if (isoPrescribedTask.CommentAllocations.Any())
            {
                CommentAllocationMapper canMapper = new CommentAllocationMapper(TaskDataMapper);
                workItem.Notes = canMapper.ImportCommentAllocations(isoPrescribedTask.CommentAllocations).ToList();
            }

            //Prescription
            if (isoPrescribedTask.HasPrescription)
            {
                Prescription rx = PrescriptionMapper.ImportPrescription(isoPrescribedTask, workItem);

                if (rx == null) return workItem;
                //Add to the Prescription the Catalog
                List<Prescription> prescriptions = DataModel.Catalog.Prescriptions as List<Prescription>;
                prescriptions?.Add(rx);

                //Add A WorkItemOperation
                WorkItemOperation operation = new WorkItemOperation();
                operation.PrescriptionId = rx.Id.ReferenceId;

                //Add the operation to the Documents and reference on the WorkItem
                List<WorkItemOperation> operations =
                    DataModel.Documents.WorkItemOperations as List<WorkItemOperation>;
                operations?.Add(operation);

                workItem.WorkItemOperationIds.Add(operation.Id.ReferenceId);

                //Track any prescription IDs to map to any completed TimeLog data 
                _rxIDsByTask.Add(isoPrescribedTask.TaskID, rx.Id.ReferenceId);
            }

            //DataLogTriggers
            if (isoPrescribedTask.DataLogTriggers.Any())
            {
                DataLogTriggerMapper dltMapper = new DataLogTriggerMapper(TaskDataMapper);
                workItem.DataLogTriggers = dltMapper.ImportDataLogTriggers(isoPrescribedTask.DataLogTriggers).ToList();
            }

            return workItem;
        }

        private WorkStatusEnum ImportStatus(ISOEnumerations.ISOTaskStatus isoStatus)
        {
            switch (isoStatus)
            {
                case ISOEnumerations.ISOTaskStatus.Canceled:
                    return WorkStatusEnum.Cancelled;
                case ISOEnumerations.ISOTaskStatus.Completed:
                    return WorkStatusEnum.Completed;
                case ISOEnumerations.ISOTaskStatus.Running:
                    return WorkStatusEnum.InProgress;
                case ISOEnumerations.ISOTaskStatus.Paused:
                    return WorkStatusEnum.Paused;
                case ISOEnumerations.ISOTaskStatus.Planned:
                case ISOEnumerations.ISOTaskStatus.Template:
                default:
                    return WorkStatusEnum.Scheduled;
            }
        }

        #endregion Import WorkItems

        #region Import LoggedData

        public IEnumerable<LoggedData> ImportLoggedDatas(IEnumerable<ISOTask> isoLoggedTasks)
        {
            List<LoggedData> adaptLoggedDatas = new List<LoggedData>();
            foreach (ISOTask isoLoggedTask in isoLoggedTasks)
            {
                LoggedData loggedData = ImportLoggedData(isoLoggedTask);
                adaptLoggedDatas.Add(loggedData);
            }
            return adaptLoggedDatas;
        }

        private LoggedData ImportLoggedData(ISOTask isoLoggedTask)
        {
            LoggedData loggedData = new LoggedData();
            loggedData.OperationData = new List<OperationData>();

            //Task ID
            if (!ImportIDs(loggedData.Id, isoLoggedTask.TaskID))
            {
                //In the case where a TSK contains both TZN and TLG data, we'll store the LoggedData as the mapped Task.
                //The Prescription ID will be assigned to the OperationData objects by means of the dictionary in this class.
                TaskDataMapper.InstanceIDMap.ReplaceADAPTID(isoLoggedTask.TaskID, loggedData.Id.ReferenceId);
            }

            //Task Name
            loggedData.Description = isoLoggedTask.TaskDesignator;

            //Grower ID
            loggedData.GrowerId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoLoggedTask.CustomerIdRef);

            //Farm ID
            loggedData.FarmId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoLoggedTask.FarmIdRef);

            //Field ID
            int? pfdID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoLoggedTask.PartFieldIdRef);
            if (pfdID.HasValue)
            {
                if (DataModel.Catalog.CropZones.Any(c => c.Id.ReferenceId == pfdID.Value))
                {
                    loggedData.CropZoneId = pfdID.Value;
                }
                else
                {
                    loggedData.FieldId = pfdID.Value;
                    if (DataModel.Catalog.CropZones.Count(c => c.FieldId == pfdID) == 1)
                    {
                        //There is a single cropZone for the field.  
                        loggedData.CropZoneId = DataModel.Catalog.CropZones.Single(c => c.FieldId == pfdID).Id.ReferenceId;
                    }
                }
            }

            //Responsible Worker
            if (!string.IsNullOrEmpty(isoLoggedTask.ResponsibleWorkerIdRef))
            {
                ISOWorker worker = ISOTaskData.ChildElements.OfType<ISOWorker>().FirstOrDefault(w => w.WorkerId == isoLoggedTask.ResponsibleWorkerIdRef);
                int? personID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoLoggedTask.ResponsibleWorkerIdRef);
                if (personID.HasValue)
                {
                    //Create a Role
                    PersonRole role = new PersonRole() { PersonId = personID.Value };

                    //Add to Catalog
                    DataModel.Catalog.PersonRoles.Add(role);
                    if (loggedData.PersonRoleIds == null)
                    {
                        loggedData.PersonRoleIds = new List<int>();
                    }

                    //Add to Task
                    loggedData.PersonRoleIds.Add(role.Id.ReferenceId);
                }
            }

            //Worker Allocations
            if (isoLoggedTask.WorkerAllocations.Any())
            {
                WorkerAllocationMapper wanMapper = new WorkerAllocationMapper(TaskDataMapper);
                List<PersonRole> personRoles = wanMapper.ImportWorkerAllocations(isoLoggedTask.WorkerAllocations).ToList();

                //Add to Catalog
                DataModel.Catalog.PersonRoles.AddRange(personRoles);
                if (loggedData.PersonRoleIds == null)
                {
                    loggedData.PersonRoleIds = new List<int>();
                }

                //Add to Task
                loggedData.PersonRoleIds.AddRange(personRoles.Select(p => p.Id.ReferenceId));
            }

            //Guidance Allocations
            if (isoLoggedTask.GuidanceAllocations.Any())
            {
                GuidanceAllocationMapper ganMapper = new GuidanceAllocationMapper(TaskDataMapper);
                List<GuidanceAllocation> allocations = ganMapper.ImportGuidanceAllocations(isoLoggedTask.GuidanceAllocations).ToList();

                //Add to Catalog
                List<GuidanceAllocation> guidanceAllocations = DataModel.Documents.GuidanceAllocations as List<GuidanceAllocation>;
                if (guidanceAllocations != null)
                {
                    guidanceAllocations.AddRange(allocations);
                }

                //Add to Task
                if (loggedData.GuidanceAllocationIds == null)
                {
                    loggedData.GuidanceAllocationIds = new List<int>();
                }
                loggedData.GuidanceAllocationIds.AddRange(allocations.Select(p => p.Id.ReferenceId));
            }

            //Comments
            if (isoLoggedTask.CommentAllocations.Any())
            {
                CommentAllocationMapper canMapper = new CommentAllocationMapper(TaskDataMapper);
                loggedData.Notes = canMapper.ImportCommentAllocations(isoLoggedTask.CommentAllocations).ToList();
            }

            //Operation Data
            if (isoLoggedTask.TimeLogs.Any())
            {
                //Find ID for any Prescription that may also be tied to this task
                int? rxID = null;
                if (_rxIDsByTask.ContainsKey(isoLoggedTask.TaskID))
                {
                    rxID = _rxIDsByTask[isoLoggedTask.TaskID];
                }

                loggedData.OperationData = TimeLogMapperFactory.ImportTimeLogs(isoLoggedTask, rxID);
            }

            //Connections
            if (isoLoggedTask.Connections.Any())
            {
                IEnumerable<EquipmentConfiguration> equipConfigs = ConnectionMapper.ImportConnections(isoLoggedTask);

                loggedData.EquipmentConfigurationGroup = new EquipmentConfigurationGroup();
                loggedData.EquipmentConfigurationGroup.EquipmentConfigurations = equipConfigs.ToList();

                //Make a reference to the IDs on the OperationData
                foreach (OperationData operationData in loggedData.OperationData)
                {
                    operationData.EquipmentConfigurationIds.AddRange(equipConfigs.Select(e => e.Id.ReferenceId));
                }

                DataModel.Catalog.EquipmentConfigurations.AddRange(equipConfigs);
            }

            //Summaries
            if (isoLoggedTask.Times.Any(t => t.HasStart && t.HasType)) //Nothing added without a Start & Type attribute
            {
                //An ADAPT LoggedData has exactly one summary.   This is what necessitates that ISO Task maps to LoggedData and ISO TimeLog maps to one or more Operation Data objects
                Summary summary = ImportSummary(isoLoggedTask, loggedData);
                if (DataModel.Documents.Summaries == null)
                {
                    DataModel.Documents.Summaries = new List<Summary>();
                }
                (DataModel.Documents.Summaries as List<Summary>).Add(summary);
                loggedData.SummaryId = summary.Id.ReferenceId;
            }

            loggedData.ReleaseSpatialData = () => { };

            return loggedData;
        }

        #region Import Summary Data
        private Summary ImportSummary(ISOTask isoLoggedTask, LoggedData loggedData)
        {
            //Per ISO11783-10:2015(E) 6.8.3, the last Time element contains the comprehensive task totals.
            //Earlier Time elements contain the task totals leading up to points where the Task was paused.
            //As such, the Summary.TimeScopes will include detail on various intermittent Timescopes,
            //and the Summary.SummaryData will contain totals from only the last time element.
            //Summary.SummaryData.Stamp will be set to a comprehensive time stamp from the beginning of the first time to the end of the last.
            Summary summary = null;
            IEnumerable<ISOTime> timeElements = isoLoggedTask.Times.Where(t => t.HasStart && t.HasType);
            if (timeElements.Any())
            {
                summary = new Summary();

                //TimeScopes
                summary.TimeScopes = new List<TimeScope>();
                foreach (ISOTime time in isoLoggedTask.Times.Where(t => t.HasStart && t.HasType)) //Nothing added without a Start and Type attribute
                {
                    TimeScope timeScope = new TimeScope();
                    timeScope.TimeStamp1 = time.Start;
                    if (time.Stop != null)
                    {
                        timeScope.TimeStamp2 = time.Stop;
                    }

                    if (time.Stop == null && time.Duration != null)
                    {
                        //Calculate the Stop time if missing and duration present
                        timeScope.TimeStamp2 = timeScope.TimeStamp1.Value.AddSeconds(time.Duration.Value);
                    }
                    timeScope.DateContext = time.Type == ISOEnumerations.ISOTimeType.Planned ? DateContextEnum.ProposedStart : DateContextEnum.ActualStart;
                    timeScope.Duration = timeScope.TimeStamp2.GetValueOrDefault() - timeScope.TimeStamp1.GetValueOrDefault();
                    summary.TimeScopes.Add(timeScope);
                }

                //Summary Data - does not have a product reference
                summary.SummaryData = ImportSummaryData(timeElements);               

                //Operation Summaries - includes a product reference
                summary.OperationSummaries = ImportOperationSummaries(isoLoggedTask);

                //Copy properties from LoggedData
                summary.GrowerId = loggedData.GrowerId;
                summary.FarmId = loggedData.FarmId;
                summary.FieldId = loggedData.FieldId;
                summary.CropZoneId = loggedData.CropZoneId;
                summary.PersonRoleIds = loggedData.PersonRoleIds;
                summary.WorkItemIds = loggedData.WorkItemIds;
                summary.GuidanceAllocationIds = loggedData.GuidanceAllocationIds;
                summary.EquipmentConfigurationGroup = loggedData.EquipmentConfigurationGroup;
            }
            return summary;
        }

        private List<StampedMeteredValues> ImportSummaryData(IEnumerable<ISOTime> timeElements)
        {
            List<StampedMeteredValues> summaryData = new List<StampedMeteredValues>();
            foreach (ISOTimeType timeType in timeElements.Select(t => t.Type).Distinct())
            {
                //Times of each type get distinct StampedMeteredValues
                summaryData.Add(GetStampedMeteredValuesForTimes(timeElements.Where(t => t.Type == timeType)));
            }
            return summaryData;
        }

        private List<OperationSummary> ImportOperationSummaries(ISOTask isoLoggedTask)
        {
            List<OperationSummary> operationSummaries = null;
            if (isoLoggedTask.ProductAllocations.Any())
            {
                IEnumerable<ISOTime> timeElements = isoLoggedTask.Times.Where(t => t.HasStart && t.HasType);
                operationSummaries = new List<OperationSummary>();
                if (isoLoggedTask.ProductAllocations.Count == 1) 
                {
                    //There is a single product allocation on the task
                    string isoProductRef = isoLoggedTask.ProductAllocations.Single().ProductIdRef;
                    OperationSummary summary = GetOperationSummary(timeElements, isoProductRef);
                    if (summary != null)
                    {
                        operationSummaries.Add(summary);
                    }
                }
                else if (isoLoggedTask.ProductAllocations.Select(p => p.ProductIdRef).Distinct().Count() == 1)
                {
                    //There is a single product on multiple allocations
                    string isoProductRef = isoLoggedTask.ProductAllocations.Select(p => p.ProductIdRef).First();
                    OperationSummary summary = GetOperationSummary(timeElements, isoProductRef);
                    if (summary != null)
                    {
                        operationSummaries.Add(summary);
                    }
                }
                else
                {
                    //There are multiple products.  Use any DeviceElements on the PANs to reconcile summaries. 
                    foreach (string isoProductRef in isoLoggedTask.ProductAllocations.Select(p => p.ProductIdRef).Distinct())
                    {
                        IEnumerable<string> deviceElementIDs = isoLoggedTask.ProductAllocations.Where(p => p.ProductIdRef == isoProductRef && p.DeviceElementIdRef != null)
                                                                                               .Select(p => p.DeviceElementIdRef);
                        OperationSummary summary = GetOperationSummary(timeElements, isoProductRef, deviceElementIDs);
                        if (summary != null)
                        {
                            operationSummaries.Add(summary);
                        }
                    }
                }
            }

            return operationSummaries;
        }

        private OperationSummary GetOperationSummary(IEnumerable<ISOTime> timeElements, string productIDRef, IEnumerable<string> deviceElementIDRefs = null)
        {
            OperationSummary operationSummary = null;
            int? productID = TaskDataMapper.InstanceIDMap.GetADAPTID(productIDRef);
            if (productID.HasValue)
            {
                operationSummary = new OperationSummary();
                operationSummary.ProductId = productID.Value;
                operationSummary.Data = new List<StampedMeteredValues>();
                operationSummary.Data.Add(GetStampedMeteredValuesForTimes(timeElements, deviceElementIDRefs));
            }
            return operationSummary;
        }

        private StampedMeteredValues GetStampedMeteredValuesForTimes(IEnumerable<ISOTime> taskTimes, IEnumerable<string> deviceElementFilter = null)
        {
            StampedMeteredValues stampedValues = new StampedMeteredValues();

            IEnumerable<ISOTime> orderedTimes = taskTimes.OrderBy(t => t.Start.Value); //This will address any out-of-order data as written in the file.

            //TimeScope
            stampedValues.Stamp = new TimeScope();
            stampedValues.Stamp.TimeStamp1 = orderedTimes.First().Start;
            if (orderedTimes.Last().Stop != null)
            {
                stampedValues.Stamp.TimeStamp2 = orderedTimes.Last().Stop;
            }
            else if (orderedTimes.Last().Duration != null)
            {
                //Calculate the Stop time if missing and duration present
                stampedValues.Stamp.TimeStamp2 = stampedValues.Stamp.TimeStamp1.Value.AddSeconds(orderedTimes.Last().Duration.Value);
            }

            //All types should be the same
            stampedValues.Stamp.DateContext = orderedTimes.First().Type == ISOEnumerations.ISOTimeType.Planned ? DateContextEnum.ProposedStart : DateContextEnum.ActualStart;
            //Duration will define the time from the first to the last time.   Gaps will be identifiable by examining Summary.Timescopes as defined above.
            stampedValues.Stamp.Duration = stampedValues.Stamp.TimeStamp2.GetValueOrDefault() - stampedValues.Stamp.TimeStamp1.GetValueOrDefault();

            //Values
            foreach (ISODataLogValue dlv in orderedTimes.Last().DataLogValues) //The last Time contains the comprehensive Task totals
            {
                MeteredValue value = GetSummaryMeteredValue(dlv);
                if (value != null)
                {
                    if (deviceElementFilter == null || deviceElementFilter.Contains(dlv.DeviceElementIdRef))
                    {
                        stampedValues.Values.Add(value);
                    }
                }
            }
            return stampedValues;
        }

        private MeteredValue GetSummaryMeteredValue(ISODataLogValue dlv)
        {
            if (dlv.ProcessDataDDI == null)
            {
                return null;
            }
            int ddi = dlv.ProcessDataDDI.AsInt32DDI();

            int dataValue = 0;
            if (dlv.ProcessDataValue.HasValue)
            {
                dataValue = dlv.ProcessDataValue.Value;
            }

            var unitOfMeasure = RepresentationMapper.GetUnitForDdi(ddi);
            if (!DDIs.ContainsKey(ddi) || unitOfMeasure == null)
            {
                return null;
            }

            DdiDefinition ddiDefintion = DDIs[ddi];

            int? deviceConfigurationID = null;
            int? deviceElementID = TaskDataMapper.InstanceIDMap.GetADAPTID(dlv.DeviceElementIdRef);
            if (deviceElementID.HasValue)
            {
                //Since Device creation is on-demand, we need to call GetDeviceElementConfiguration here to ensure the relevant device is created if it hasn't been yet.
                var hierarchyElement = TaskDataMapper?.DeviceElementHierarchies?.GetMatchingElement(dlv.DeviceElementIdRef);
                var adaptDeviceElement = DataModel?.Catalog?.DeviceElements?.FirstOrDefault(d => d?.Id?.ReferenceId == deviceElementID.Value);
                if (hierarchyElement != null && adaptDeviceElement != null)
                {
                    DeviceElementConfiguration config = DeviceElementMapper.GetDeviceElementConfiguration(adaptDeviceElement, hierarchyElement, DataModel.Catalog);
                    if (config != null)
                    {
                        deviceConfigurationID = config.Id.ReferenceId;
                    }
                }
            }

            return new MeteredValue
            {
                Value = new NumericRepresentationValue(RepresentationMapper.Map(ddi) as NumericRepresentation,
                                                       unitOfMeasure,
                                                       new NumericValue(unitOfMeasure, dataValue * ddiDefintion.Resolution)),
                DeviceConfigurationId = deviceConfigurationID
            };
        }
        #endregion Import Summary Data

        #endregion Import Logged Data

        #endregion Import

    }
}
