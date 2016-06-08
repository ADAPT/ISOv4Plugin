using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IMeterMapper
    {
        List<WorkingData> Map(TIM tim, IEnumerable<ISOSpatialRow> isoRecords, int sectionId);
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

        public List<WorkingData> Map(TIM tim, IEnumerable<ISOSpatialRow> isoSpatialRows, int sectionId)
        {
            var meters = new List<WorkingData>();
            var dlvs = tim.Items.Where(x => (x as DLV) != null).Cast<DLV>();
            for (int order = 0; order < dlvs.Count(); order++)
            {
                var dlv = dlvs.ElementAt(order);
                meters.AddRange(Map(dlv, isoSpatialRows, sectionId, order));
            }
            return meters;
        }

        private IEnumerable<WorkingData> Map(DLV dlv, IEnumerable<ISOSpatialRow> isoSpatialRows, int sectionId, int order)
        {
            var meters = new List<WorkingData>();
            if (_ddis.ContainsKey(Convert.ToInt32(dlv.A, 16)))
            {
                meters.Add(MapNumericMeter(dlv, sectionId, order));
                return meters;
            }
            var meterCreator = _enumeratedMeterCreatorFactory.GetMeterCreator(Convert.ToInt32(dlv.A, 16));
            if(meterCreator != null)
            {
                var isoEnumeratedMeters = meterCreator.CreateMeters(isoSpatialRows);
                isoEnumeratedMeters.ForEach(x => x.Id.UniqueIds.Add(_uniqueIdMapper.Map("DLV" + order)));
                meters.AddRange(isoEnumeratedMeters);
            }
            return meters;
        }

        private NumericWorkingData MapNumericMeter(DLV dlv, int sectionId, int order)
        {
            var meter = new NumericWorkingData
            {
                UnitOfMeasure = _representationMapper.GetUnitForDdi(Convert.ToInt32(dlv.A, 16)),
                DeviceElementUseId = sectionId,
                Representation = _representationMapper.Map(Convert.ToInt32(dlv.A, 16))
            };
            meter.Id.UniqueIds.Add(_uniqueIdMapper.Map("DLV" + order));
            return meter;
        }
    }
}
