using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IDocumentMapper
    {
        void Map(List<TSK> tsks, Documents documents, string dataPath, Catalog catalog, ISO11783_TaskData iso11783TaskData);
    }

    public class DocumentMapper : IDocumentMapper
    {
        private readonly ILoggedDataMapper _loggedDataMapper;

        public DocumentMapper() : this(new LoggedDataMapper())
        {
            
        }

        public DocumentMapper(ILoggedDataMapper loggedDataMapper)
        {
            _loggedDataMapper = loggedDataMapper;
        }

        public void Map(List<TSK> tsks, Documents documents, string dataPath, Catalog catalog, ISO11783_TaskData iso11783TaskData)
        {
            if(documents.LoggedData == null)
                documents.LoggedData = new List<LoggedData>();
            
            tsks.ForEach(x => documents.LoggedData.Add(_loggedDataMapper.Map(x, dataPath, catalog)));
        }
    }
}
