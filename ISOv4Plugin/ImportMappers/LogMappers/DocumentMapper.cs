using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IDocumentMapper
    {
        Documents Map(List<TSK> tsks, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel, Dictionary<string, List<UniqueId>> linkedIds);
    }

    public class DocumentMapper : IDocumentMapper
    {
        private readonly ILoggedDataMapper _loggedDataMapper;
        private readonly IWorkOrderMapper _workOrderMapper;

        public DocumentMapper() : this(new LoggedDataMapper(), new WorkOrderMapper())
        {
            
        }

        public DocumentMapper(ILoggedDataMapper loggedDataMapper, IWorkOrderMapper workOrderMapper)
        {
            _loggedDataMapper = loggedDataMapper;
            _workOrderMapper = workOrderMapper;
        }

        public Documents Map(List<TSK> tsks, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel, Dictionary<string, List<UniqueId>> linkedIds)
        {
            var tasksWithLoggedData = tsks.Where(task => task.Items != null && task.Items.OfType<TLG>().Any()).ToList();
            _loggedDataMapper.Map(tasksWithLoggedData, dataPath, dataModel, linkedIds);

            var tasksWithoutLogData = tsks.Where(task => task.Items == null || !task.Items.OfType<TLG>().Any()).ToList();
            _workOrderMapper.Map(tasksWithoutLogData, dataModel);

            return dataModel.Documents;
        }
    }
}
