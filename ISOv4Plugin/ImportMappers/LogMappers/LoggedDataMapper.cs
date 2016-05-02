using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface ILoggedDataMapper
    {
        IEnumerable<LoggedData> Map(List<TSK> tsk, string dataPath, Catalog catalog);
        LoggedData Map(TSK tsk, string dataPath, Catalog catalog);
    }

    public class LoggedDataMapper : ILoggedDataMapper
    {
        private readonly IOperationDataMapper _operationDataMapper;
        private readonly IUniqueIdMapper _uniqueIdMapper;
        private readonly ITimeScopeMapper _timeScopeMapper;

        public LoggedDataMapper() : this(new OperationDataMapper(), new UniqueIdMapper(), new TimeScopeMapper())
        {
            
        }

        public LoggedDataMapper(IOperationDataMapper operationDataMapper, IUniqueIdMapper uniqueIdMapper, ITimeScopeMapper timeScopeMapper)
        {
            _operationDataMapper = operationDataMapper;
            _uniqueIdMapper = uniqueIdMapper;
            _timeScopeMapper = timeScopeMapper;
        }

        public IEnumerable<LoggedData> Map(List<TSK> tsk, string dataPath, Catalog catalog)
        {
            return tsk.Select(x => Map(x, dataPath, catalog));
        }

        public LoggedData Map(TSK tsk, string dataPath, Catalog catalog)
        {
            if (tsk == null)
                return null;

            var tlgs = tsk.Items.GetItemsOfType<TLG>();

            var loggedData = new LoggedData
            {
                OperationData = _operationDataMapper.Map(tlgs, dataPath),
                GrowerId = FindMatchingId(tsk.C, catalog.Growers.Select(g => g.Id)),
                FarmId = FindMatchingId(tsk.D, catalog.Farms.Select(f => f.Id)),
                FieldId = FindMatchingId(tsk.E, catalog.Fields.Select(f => f.Id)),
            };
            loggedData.Id.UniqueIds.Add(_uniqueIdMapper.Map(tsk.A));

            ConvertTimsToTimescopes(tsk, catalog, loggedData);

            return loggedData;
        }

        private void ConvertTimsToTimescopes(TSK tsk, Catalog catalog, LoggedData loggedData)
        {
            var tims = tsk.Items.GetItemsOfType<TIM>();
            var timeScopes = _timeScopeMapper.Map(tims, catalog);
            
            if(timeScopes != null)
                loggedData.TimeScopeIds = timeScopes.Select(t => t.Id.ReferenceId).ToList();
        }

        private static int? FindMatchingId(string isoId, IEnumerable<CompoundIdentifier> adaptObjects)
        {
            var compoundIdentifier = adaptObjects.SingleOrDefault(x => x.FindIsoId() == isoId);

            if (compoundIdentifier != null)
                return compoundIdentifier.ReferenceId;
            return null;
        }
    }
}
