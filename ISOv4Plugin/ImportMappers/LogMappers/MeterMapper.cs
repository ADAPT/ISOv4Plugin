using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IMeterMapper
    {
        List<Meter> Map(TIMHeader timHeader, IEnumerable<ISOSpatialRow> isoRecords, int sectionId);
    }

    public class MeterMapper : IMeterMapper
    {
        private readonly IRepresentationMapper _representationMapper;
        private readonly IEnumeratedMeterFactory _enumeratedMeterCreatorFactory;
        private readonly IUniqueIdMapper _uniqueIdMapper;
        private readonly Dictionary<int, DdiDefinition> _ddis;

        public MeterMapper() : this(new RepresentationMapper(), new EnumeratedMeterFactory(), new UniqueIdMapper())
        {
            
        }

        public MeterMapper(IRepresentationMapper representationMapper, IEnumeratedMeterFactory enumeratedMeterCreatorFactory, IUniqueIdMapper uniqueIdMapper)
        {
            _representationMapper = representationMapper;
            _enumeratedMeterCreatorFactory = enumeratedMeterCreatorFactory;
            _uniqueIdMapper = uniqueIdMapper;
            _ddis = DdiLoader.Ddis;
        }

        public List<Meter> Map(TIMHeader timHeader, IEnumerable<ISOSpatialRow> isoSpatialRows, int sectionId)
        {
            var meters = new List<Meter>();
            for (int order = 0; order < timHeader.DLVs.Count; order++)
            {
                var dlvHeader = timHeader.DLVs[order];
                meters.AddRange(Map(dlvHeader, isoSpatialRows, sectionId, order));
            }
            return meters;
        }

        private IEnumerable<Meter> Map(DLVHeader dlv, IEnumerable<ISOSpatialRow> isoSpatialRows, int sectionId, int order)
        {
            var meters = new List<Meter>();
            if (_ddis.ContainsKey(Convert.ToInt32(dlv.ProcessDataDDI.Value)))
            {
                meters.Add(MapNumericMeter(dlv, sectionId, order));
                return meters;
            }
            var meterCreator = _enumeratedMeterCreatorFactory.GetMeterCreator(dlv.ProcessDataDDI.Value as int?);
            if(meterCreator != null)
            {
                var isoEnumeratedMeters = meterCreator.CreateMeters(isoSpatialRows);
                isoEnumeratedMeters.ForEach(x => x.Id.UniqueIds.Add(_uniqueIdMapper.Map("DLV" + order)));
                meters.AddRange(isoEnumeratedMeters);
            }
            return meters;
        }

        private NumericMeter MapNumericMeter(DLVHeader dlv, int sectionId, int order)
        {
            var meter = new NumericMeter
            {
                UnitOfMeasure = _representationMapper.GetUnitForDdi(Convert.ToInt32(dlv.ProcessDataDDI.Value)),
                SectionId = sectionId,
                Representation = _representationMapper.Map(Convert.ToInt32(dlv.ProcessDataDDI.Value))
            };
            meter.Id.UniqueIds.Add(_uniqueIdMapper.Map("DLV" + order));
            return meter;
        }
    }
}
