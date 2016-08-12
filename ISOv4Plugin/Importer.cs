using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public interface IImporter
    {
        ApplicationDataModel.ADM.ApplicationDataModel Import(ISO11783_TaskData iso11783TaskData, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel, Dictionary<string, List<UniqueId>> linkedIds);
    }

    public class Importer : IImporter
    {
        private readonly IDocumentMapper _documentMapper;

        public Importer() : this(new DocumentMapper())
        {
        }

        public Importer(IDocumentMapper documentMapper)
        {
            _documentMapper = documentMapper;
        }

        public ApplicationDataModel.ADM.ApplicationDataModel Import(ISO11783_TaskData iso11783TaskData, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel, Dictionary<string, List<UniqueId>> linkedIds)
        {
            if (dataModel.Catalog == null)
                dataModel.Catalog = CreateCatalog();
            if (dataModel.Documents == null)
                dataModel.Documents = CreateDocuments();

            if(dataModel.Documents.LoggedData == null)
                dataModel.Documents.LoggedData = new List<LoggedData>();

            var isoObjects = iso11783TaskData.Items;
            if (isoObjects == null || isoObjects.Length == 0)
                return dataModel;

            var tasks = isoObjects.GetItemsOfType<TSK>();

            _documentMapper.Map(tasks, dataPath, dataModel, linkedIds);
            return dataModel;
        }

        private static Documents CreateDocuments()
        {
            return new Documents
            {
                GuidanceAllocations = new List<GuidanceAllocation>(),
                LoggedData = new List<LoggedData>(),
                Plans = new List<Plan>(),
                Recommendations = new List<Recommendation>(),
                Summaries = new List<Summary>(),
                WorkItemOperations = new List<WorkItemOperation>(),
                WorkItems = new List<WorkItem>(),
                WorkOrders = new List<WorkOrder>(),
            };
        }

        private static Catalog CreateCatalog()
        {
            return new Catalog();
        }
    }
}