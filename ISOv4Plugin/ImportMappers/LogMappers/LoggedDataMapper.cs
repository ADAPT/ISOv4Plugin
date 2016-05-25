using System;
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
        List<LoggedData> Map(List<TSK> tsks, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel);
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

        public List<LoggedData> Map(List<TSK> tsks, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
        {
            return tsks == null 
                ? null 
                : tsks.Select(tsk => Map(tsk, dataPath, dataModel)).ToList();
        }

        private LoggedData Map(TSK tsk, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
        {
            if (tsk == null || dataModel.Documents.LoggedData == null)
                return null;

            var existingLoggedData = dataModel.Documents.LoggedData.FirstOrDefault(x => x.Id.FindIsoId() == tsk.A);
            if (existingLoggedData == null)
                return null;

            var taskId = null as int?;
            var grd = tsk.Items != null ? tsk.Items.FirstOrDefault(x => x.GetType() == typeof(GRD)) : null;
            if (grd != null)
            {
                taskId = dataModel.Catalog.Prescriptions.Single(x => x.Id.FindIsoId() == tsk.A).Id.ReferenceId;
            }
            existingLoggedData.OperationData = _operationDataMapper.Map(tsk.Items.GetItemsOfType<TLG>(), taskId, dataPath, existingLoggedData.Id.ReferenceId);
            return existingLoggedData;
        }
    }
}
