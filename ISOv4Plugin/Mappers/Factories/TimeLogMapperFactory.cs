/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Mappers.Manufacturers;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers.Factories
{
    /// <summary>
    /// Factory class to help process ISOTimeLog data
    /// </summary>
    public class TimeLogMapperFactory
    {
        private readonly TimeLogMapper _timeLogMapper;
        private readonly TaskDataMapper _taskDataMapper;
        private readonly IManufacturer _manufacturer;

        // A wrapper class to hold together ISOTimeLog and included ISODataLogValues.
        // This avoids multiple calls to ISOTimeLog.GetTimeElement() which performs xml parsing on each call.
        private class TimeLogWrapper
        {
            public ISOTimeLog ISOTimeLog { get; set; }
            public List<ISODataLogValue> DataLogValues { get; set; }
        }

        // Class representing a group of TimeLogWrapper objects that should be processed together
        private class TimeLogWrapperGroup : List<TimeLogWrapper>
        {
            public TimeLogWrapperGroup()
            {
                KeepAsGroup = false;
            }

            public bool KeepAsGroup { get; set; }
        }

        // Class representing a group of ISOTimeLog objects that will be processed as single entity
        private class TimeLogGroup : List<ISOTimeLog>
        {
            public TimeLogGroup(IEnumerable<ISOTimeLog> collection): base(collection) { }
        }

        public TimeLogMapperFactory(TaskDataMapper taskDataMapper)
        {
            _taskDataMapper = taskDataMapper;
            _timeLogMapper = new TimeLogMapper(taskDataMapper);

            _manufacturer = ManufacturerFactory.GetManufacturer(taskDataMapper);
        }

        public IEnumerable<OperationData> ImportTimeLogs(ISOTask loggedTask, int? prescriptionID)
        {
            var timeLogGroups = GetTimeLogGroups(loggedTask);

            var operationDatas = new List<OperationData>();
            foreach (var timeLogGroup in timeLogGroups)
            {
                operationDatas.AddRange(timeLogGroup.Count > 1
                    ? new MultiFileTimeLogMapper(_taskDataMapper).ImportTimeLogs(loggedTask, timeLogGroup, prescriptionID)
                    : _timeLogMapper.ImportTimeLogs(loggedTask, timeLogGroup, prescriptionID));
            }

            return _manufacturer?.PostProcessOperationData(_taskDataMapper, loggedTask, operationDatas) ?? operationDatas;
        }

        public IEnumerable<ISOTimeLog> ExportTimeLogs(IEnumerable<OperationData> operationDatas, string dataPath)
        {
            return _timeLogMapper.ExportTimeLogs(operationDatas, dataPath);
        }


        private List<TimeLogGroup> GetTimeLogGroups(ISOTask loggedTask)
        {
            var dataPath = _timeLogMapper.TaskDataPath;

            var timeLogGroups = GroupByDataLogValueCount(loggedTask, dataPath);
            timeLogGroups = HandleRollOverScenario(timeLogGroups);
            timeLogGroups = HandleDuplicateDataLogValues(timeLogGroups);

            return timeLogGroups.Select(x => new TimeLogGroup(x.Select(y => y.ISOTimeLog).ToList())).ToList();
        }

        private List<TimeLogWrapperGroup> HandleDuplicateDataLogValues(List<TimeLogWrapperGroup> timeLogGroups)
        {
            var result = new List<TimeLogWrapperGroup> ();
            // Check if all DataLogValues are referencing unique combination of DET/DDI.
            // Exclude PGN-based DLVs as they are not unique.
            foreach (var timeLogGroup in timeLogGroups)
            {
                var duplicatesByElementAndDDI = timeLogGroup.SelectMany(x => x.DataLogValues)
                    .Where(x => x.DataLogPGN == null && x.ProcessDataIntDDI != 0x0000)
                    .GroupBy(x => new { x.DeviceElementIdRef, x.ProcessDataDDI })
                    .Where(x => x.Count() > 1);
                if (!timeLogGroup.KeepAsGroup && duplicatesByElementAndDDI.Any())
                {
                    // Some DataLogValues are referencing same combination of DET/DDI.
                    // Split them into separate groups
                    result.AddRange(timeLogGroup.Select(x => new TimeLogWrapperGroup { x }));
                }
                else
                {
                    result.Add(timeLogGroup);
                }
            }
            return result;
        }

        private List<TimeLogWrapperGroup> HandleRollOverScenario(List<TimeLogWrapperGroup> timeLogGroups)
        {
            // Initial TLG files with less than 255 DLVs could be changed
            // by appending more DLVs to then in the process. The other file where these DLVS
            // are coming from is no longer written in.
            foreach (var timeLogGroup in timeLogGroups)
            {
                // Unique device element ids from all duplicate DLVs
                var duplicateDeviceElementIds = timeLogGroup.SelectMany(y => y.DataLogValues)
                    .Where(x => x.DataLogPGN == null)
                    .GroupBy(x => new { x.DeviceElementIdRef, x.ProcessDataDDI })
                    .Where(x => x.Count() > 1)
                    .Select(x => x.Key.DeviceElementIdRef)
                    .Distinct()
                    .ToList();
                if (duplicateDeviceElementIds.Count <= 0)
                {
                    continue;
                }

                // Count how many TLGs have 255 DLVs
                var timeLogs = timeLogGroup.Where(x => x.DataLogValues.Any(y => duplicateDeviceElementIds.Contains(y.DeviceElementIdRef)))
                    .Where(x => x.DataLogValues.Count == 255)
                    .ToList();

                // Multiple TLGs with 255 DLVs in them. Most likely a roll-over scenario
                if (timeLogs.Count > 1)
                {
                    // Keep them as a single group
                    timeLogGroup.KeepAsGroup = true;
                }
            }

            return timeLogGroups;
        }

        private List<TimeLogWrapperGroup> GroupByDataLogValueCount(ISOTask loggedTask, string dataPath)
        {
            // All consequent time logs with 255 DLVs are kept together as a group.
            var timeLogGroups = new List<TimeLogWrapperGroup>();
            var logGroup = new TimeLogWrapperGroup();
            foreach (var timeLog in loggedTask.TimeLogs)
            {
                ISOTime templateTime = timeLog.GetTimeElement(dataPath);
                if (templateTime != null)
                {
                    logGroup.Add(new TimeLogWrapper { DataLogValues = templateTime.DataLogValues, ISOTimeLog = timeLog });
                    // A time log with less than 255 DLVs found. Add it to current group
                    // and start a new one.
                    if (templateTime.DataLogValues.Count < 255)
                    {
                        timeLogGroups.Add(logGroup);
                        logGroup = new TimeLogWrapperGroup();
                    }
                }
            }
            // Add remaning log group
            if (logGroup.Count > 0)
            {
                timeLogGroups.Add(logGroup);
            }
            return timeLogGroups;
        }
    }
}
