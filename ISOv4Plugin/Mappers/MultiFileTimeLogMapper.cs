/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    /// <summary>
    /// A TimeLogMapper class with support for data split between two or more ISO TLG files.
    /// </summary>
    internal class MultiFileTimeLogMapper : TimeLogMapper, ITimeLogMapper
    {
        private ISOTime _combinedTime;
        private IEnumerable<ISOTimeLog> _timeLogs;

        // Helper class to keep track of individual TLG files and current record from each
        private class BinaryReaderHelper
        {
            public IEnumerator<ISOSpatialRow> Enumerator { get; set; }
            public ISOSpatialRow CurrentRecord { get; set; }
        }

        internal MultiFileTimeLogMapper(TaskDataMapper taskDataMapper)
            : base(taskDataMapper)
        {
        }

        #region Import

        public override IEnumerable<OperationData> ImportTimeLogs(ISOTask loggedTask, IEnumerable<ISOTimeLog> timeLogs, int? prescriptionID)
        {
            _timeLogs = timeLogs;
            // Combine ISOTime elements from each TimeLog into one.
            _combinedTime = CreateCombinedTime();
            // Read data from all timelogs as if it was a single file.
            // Pass first available TimeLog to avoid breaking base class.
            return ImportTimeLog(loggedTask, timeLogs.First(), prescriptionID);
        }

        protected override IEnumerable<ISOSpatialRow> ReadTimeLog(ISOTimeLog _timeLog, string _dataPath)
        {
            List<BinaryReaderHelper> readers = new List<BinaryReaderHelper>();
            try
            {
                // Obtain binary readers for each time log
                foreach (var timeLog in _timeLogs)
                {
                    var reader = base.ReadTimeLog(timeLog, TaskDataPath);
                    if (reader != null)
                    {
                        readers.Add(new BinaryReaderHelper
                        {
                            Enumerator = reader.GetEnumerator()
                        });
                    }
                }

                return ReadFromBinaryReaders(readers);
            }
            finally
            {
                // Clean up readers
                foreach (var reader in readers)
                {
                    reader.Enumerator?.Dispose();
                }
            }
        }

        private IEnumerable<ISOSpatialRow> ReadFromBinaryReaders(List<BinaryReaderHelper> readers)
        {
            // Below alogrithm is using queues for each binary file and matching records on TimeStart/Position.
            // At start of each iteration a single record is read from binary file into queue.
            // Records with earliest TimeStart are merged together and removed from each file queue.
            while (true)
            {
                // Read next record from each time log
                foreach (var reader in readers)
                {
                    if (reader.CurrentRecord == null)
                    {
                        reader.CurrentRecord = reader.Enumerator.MoveNext() ? reader.Enumerator.Current : null;
                    }
                }

                // Only get readers which still have records;
                var readersWithData = readers.Where(x => x.CurrentRecord != null).ToList();
                if (readersWithData.Count == 0)
                {
                    // No more records in each file. Stop processing.
                    break;
                }

                // Group records by TimeStart and East/North position, and then grab ones with earliest TimeStart.
                // This leads to processing earliest records from any file first and keeping other records untouched.
                // They will be processed in the next loop iteration along with any records read from already processed files.
                var candidates = readersWithData.GroupBy(x => new { x.CurrentRecord.TimeStart, x.CurrentRecord.EastPosition, x.CurrentRecord.NorthPosition })
                    .OrderBy(x => x.Key.TimeStart)
                    .First().ToList();

                // Merge data from all candidates into first record
                ISOSpatialRow result = null;
                foreach (var candidate in candidates)
                {
                    result = result == null ? candidate.CurrentRecord : result.Merge(candidate.CurrentRecord);
                    // Clear current record to force reading next one
                    candidate.CurrentRecord = null;
                }

                yield return result;
            }
        }

        protected override ISOTime GetTimeElementFromTimeLog(ISOTimeLog isoTimeLog)
        {
            // Always return a combined ISOTime record.
            return _combinedTime;
        }

        private ISOTime CreateCombinedTime()
        {
            ISOTime result = null;
            foreach (var timeLog in _timeLogs)
            {
                var time = timeLog.GetTimeElement(TaskDataPath);
                result = ISOTime.Merge(result, time);
            }

            var duplicateDataLogValues = result.DataLogValues
                .Where(x => x.DataLogPGN == null)
                .GroupBy(x => new { x.DeviceElementIdRef, x.ProcessDataDDI })
                .Where(x => x.Count() > 1)
                .SelectMany(x => x.Skip(1))
                .ToList();
            duplicateDataLogValues.ForEach(x => result.DataLogValues.Remove(x));

            return result;
        }

        #endregion Import
    }
}
