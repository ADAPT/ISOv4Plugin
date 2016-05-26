using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IOperationDataMapper
    {
        IEnumerable<OperationData> Map(List<TLG> tlgs, int? prescrptionId, string datacardPath, int loggedDataReferenceId, Dictionary<string, List<UniqueId>> linkedIds);
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

        public IEnumerable<OperationData> Map(List<TLG> tlgs, int? prescrptionId, string datacardPath, int loggedDataReferenceId, Dictionary<string, List<UniqueId>> linkedIds)
        {
            return tlgs.Select(x => Map(x, prescrptionId, datacardPath, loggedDataReferenceId, linkedIds)).ToList();
        }

        private OperationData Map(TLG tlg, int? prescrptionId, string datacardPath, int loggedDataReferenceId, Dictionary<string, List<UniqueId>> linkedIds)
        {
            var tim = _xmlReader.ReadTlgXmlData(datacardPath, tlg.A + ".xml").First();
            var isoRecords = _binaryReader.Read(datacardPath, tlg.A + ".bin", tim).ToList();
            var sections = _sectionMapper.Map(new List<TIM> {tim}, isoRecords); 
            var meters = sections != null ? sections.SelectMany(x => x.GetMeters()).ToList() : new List<Meter>();

            var operationData = new OperationData
            {
                LoggedDataId = loggedDataReferenceId,
                GetSpatialRecords = () => _spatialRecordMapper.Map(isoRecords, meters),
                MaxDepth = 0,
                GetSections = x => x == 0 ? sections : new List<Section>(),
                PrescriptionId = prescrptionId
            };
            operationData.Id.UniqueIds.Add(_uniqueIdMapper.Map(tlg.A));

            if(linkedIds.ContainsKey(tlg.A))
            {
                foreach (var linkedId in linkedIds[tlg.A])
                    operationData.Id.UniqueIds.Add(linkedId);
            }

            return operationData;
        }
    }
}
