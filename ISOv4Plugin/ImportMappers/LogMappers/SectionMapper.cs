using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface ISectionMapper
    {
        List<Section> Map(TIMHeader timHeader, List<ISOSpatialRow> isoRecords);
    }

    public class SectionMapper : ISectionMapper
    {
        private readonly IMeterMapper _meterMapper;

        public SectionMapper() : this(new MeterMapper())
        {
            
        }

        public SectionMapper(IMeterMapper meterMapper)
        {
            _meterMapper = meterMapper;
        }

        public List<Section> Map(TIMHeader timHeader, List<ISOSpatialRow> isoRecords)
        {
            var section = new Section();
            var meters = _meterMapper.Map(timHeader, isoRecords, section.Id.ReferenceId);
            section.GetMeters = () => meters;

            return new List<Section> { section };
        }
    }
}
