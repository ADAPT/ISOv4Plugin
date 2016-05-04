using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface ILoggedDataMapper
    {
        List<LoggedData> Map(List<TSK> tsks, string dataPath, Documents documents);
    }

    public class LoggedDataMapper : ILoggedDataMapper
    {
        private readonly IOperationDataMapper _operationDataMapper;

        public LoggedDataMapper() : this(new OperationDataMapper())
        {
            
        }

        public LoggedDataMapper(IOperationDataMapper operationDataMapper)
        {
            _operationDataMapper = operationDataMapper;
        }

        public List<LoggedData> Map(List<TSK> tsks, string dataPath, Documents documents)
        {
            return tsks == null 
                ? null 
                : tsks.Select(tsk => Map(tsk, dataPath, documents)).ToList();
        }

        private LoggedData Map(TSK tsk, string dataPath, Documents documents)
        {
            if (tsk == null || documents.LoggedData == null)
                return null;

            var existingLoggedData = documents.LoggedData.FirstOrDefault(x => x.Id.FindIsoId() == tsk.A);
            if (existingLoggedData == null) 
                return null;

            existingLoggedData.OperationData = _operationDataMapper.Map(tsk.Items.GetItemsOfType<TLG>(), dataPath, existingLoggedData.Id.ReferenceId);
            return existingLoggedData;
        }
    }
}
