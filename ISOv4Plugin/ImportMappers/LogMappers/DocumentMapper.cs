using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IDocumentMapper
    {
        Documents Map(List<TSK> tsks, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel);
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

        public Documents Map(List<TSK> tsks, string dataPath, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
        {           
            _loggedDataMapper.Map(tsks, dataPath, dataModel);

            return dataModel.Documents;
        }
    }
}
