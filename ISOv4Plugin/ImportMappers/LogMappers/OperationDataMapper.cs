using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IOperationDataMapper
    {
        IEnumerable<OperationData> Map(List<TLG> tlgs, string datacardPath, int loggedDataReferenceId);
    }

    public class OperationDataMapper : IOperationDataMapper
    {
        private readonly ISpatialRecordMapper _spatialRecordMapper;
        private readonly ISectionMapper _sectionMapper;
        private readonly IUniqueIdMapper _uniqueIdMapper;
        private readonly IXmlReader _xmlReader;
        private readonly IBinaryReader _binaryReader;

        public OperationDataMapper() : this (new XmlReader(), new BinaryReader(), new SpatialRecordMapper(), new SectionMapper(), new UniqueIdMapper())
        {
            
        }

        public OperationDataMapper(IXmlReader xmlReader, IBinaryReader binaryReader, ISpatialRecordMapper spatialRecordMapper, ISectionMapper sectionMapper, IUniqueIdMapper uniqueIdMapper)
        {
            _spatialRecordMapper = spatialRecordMapper;
            _sectionMapper = sectionMapper;
            _uniqueIdMapper = uniqueIdMapper;
            _xmlReader = xmlReader;
            _binaryReader = binaryReader;
        }

        public IEnumerable<OperationData> Map(List<TLG> tlgs, string datacardPath, int loggedDataReferenceId)
        {
            return tlgs.Select(x => Map(x, datacardPath, loggedDataReferenceId)).ToList();
        }

        private OperationData Map(TLG tlg, string datacardPath, int loggedDataReferenceId)
        {
            var timHeader = _xmlReader.ReadTlgXmlData(datacardPath, tlg.A + ".xml");
            var isoRecords = _binaryReader.Read(datacardPath, tlg.A + ".bin", timHeader).ToList();
            var sections = _sectionMapper.Map(timHeader, isoRecords);
            var meters = sections != null ? sections.SelectMany(x => x.GetMeters()).ToList() : new List<Meter>();

            var operationData = new OperationData
            {
                LoggedDataId = loggedDataReferenceId,
                GetSpatialRecords = () => _spatialRecordMapper.Map(isoRecords, meters),
                MaxDepth = 0,
                GetSections = x => x == 0 ? sections : new List<Section>(),
            };
            operationData.Id.UniqueIds.Add(_uniqueIdMapper.Map(tlg.A));

            return operationData;
        }
    }
}
