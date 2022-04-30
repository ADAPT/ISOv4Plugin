/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers.Factories
{
    /// <summary>
    /// Factory class to helping process ISOTimeLog data
    /// </summary>
    public class TimeLogMapperFactory
    {
        private readonly TimeLogMapper _timeLogMapper;
        private readonly MultiFileTimeLogMapper _multiFileTimeLogMapper;
        private readonly TaskDataMapper _taskDataMapper;

        // Helper class to analyze ISOTimeLog data
        private class TimeLogHelper
        {
            public ISOTimeLog ISOTimeLog { get; set; }
            public ISOTime ISOTime { get; set; }
        }

        public TimeLogMapperFactory(TaskDataMapper taskDataMapper)
        {
            _taskDataMapper = taskDataMapper;
            _timeLogMapper = new TimeLogMapper(taskDataMapper);
            _multiFileTimeLogMapper = new MultiFileTimeLogMapper(taskDataMapper);
        }

        public IEnumerable<OperationData> ImportTimeLogs(ISOTask loggedTask, int? prescriptionID)
        {
            bool enableMultiFileTimeLogs = false;
            if (_taskDataMapper.Properties != null)
            {
                bool.TryParse(_taskDataMapper.Properties.GetProperty(TaskDataMapper.EnableMultiFileTimeLogs), out enableMultiFileTimeLogs);
            }

            var timeLogGroups = GetTimeLogGroups(loggedTask);

            var opearationDats = new List<OperationData>();
            foreach (var timeLogGroup in timeLogGroups)
            {
                opearationDats.AddRange(enableMultiFileTimeLogs && timeLogGroup.Count > 1
                    ? _multiFileTimeLogMapper.ImportTimeLogs(loggedTask, timeLogGroup, prescriptionID)
                    : _timeLogMapper.ImportTimeLogs(loggedTask, timeLogGroup, prescriptionID));
            }
            return opearationDats;
        }

        public IEnumerable<ISOTimeLog> ExportTimeLogs(IEnumerable<OperationData> operationDatas, string dataPath)
        {
            return _timeLogMapper.ExportTimeLogs(operationDatas, dataPath);
        }


        private List<List<ISOTimeLog>> GetTimeLogGroups(ISOTask loggedTask)
        {
            var dataPath = _timeLogMapper.TaskDataPath;

            var timeLogGroups = GroupByDataLogValueCount(loggedTask, dataPath);
            timeLogGroups = HandleDuplicateDataLogValues(timeLogGroups);

            return timeLogGroups.Select(x => x.Select(y => y.ISOTimeLog).ToList()).ToList();
        }

        private List<List<TimeLogHelper>> HandleDuplicateDataLogValues(List<List<TimeLogHelper>> timeLogGroups)
        {
            var result = new List<List<TimeLogHelper>>();
            // Check if all DataLogValues are referencing unique combination of DET/DDI.
            // Exclude PGN-based DLVs as they are not unique.
            foreach (var timeLogGroup in timeLogGroups)
            {
                var hasDuplicatesByElementAndDDI = timeLogGroup.SelectMany(x => x.ISOTime.DataLogValues)
                    .Where(x => x.DataLogPGN == null)
                    .GroupBy(x => new { x.DeviceElementIdRef, x.ProcessDataDDI })
                    .Where(x => x.Count() > 1)
                    .Any();
                if (hasDuplicatesByElementAndDDI)
                {
                    // Some DataLogValues are referencing same combination of DET/DDI.
                    // Split them into separate groups
                    result.AddRange(timeLogGroup.Select(x => new List<TimeLogHelper> { x }));
                }
                else
                {
                    result.Add(timeLogGroup);
                }
            }
            return result;
        }

        private List<List<TimeLogHelper>> GroupByDataLogValueCount(ISOTask loggedTask, string dataPath)
        {
            // All consequent time logs with 255 DLVs are kept together as a group.
            var timeLogGroups = new List<List<TimeLogHelper>>();
            var logGroup = new List<TimeLogHelper>();
            foreach (var timeLog in loggedTask.TimeLogs)
            {
                ISOTime templateTime = timeLog.GetTimeElement(dataPath);
                logGroup.Add(new TimeLogHelper { ISOTime = templateTime, ISOTimeLog = timeLog });
                // A time log with less than 255 DLVs found. Add it to current group
                // and start a new one.
                if (templateTime.DataLogValues.Count < 255)
                {
                    timeLogGroups.Add(logGroup);
                    logGroup = new List<TimeLogHelper>();
                }
            }
            if (logGroup.Count > 1)
            {
                timeLogGroups.Add(logGroup);
            }
            return timeLogGroups;
        }
    }
}
