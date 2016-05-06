using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IDocumentMapper
    {
        Documents Map(List<TSK> tsks, string dataPath, Documents documents);
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

        public Documents Map(List<TSK> tsks, string dataPath, Documents documents)
        {           
            _loggedDataMapper.Map(tsks, dataPath, documents);

            return documents;
        }
    }
}
