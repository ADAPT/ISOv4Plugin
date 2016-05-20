using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface ISectionMapper
    {
        List<Section> Map(List<TIM> tims, List<ISOSpatialRow> isoRecords);
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

        public List<Section> Map(List<TIM> tims, List<ISOSpatialRow> isoRecords)
        {
            var sections = new List<Section>();

            foreach (var tim in tims)
            {
                var section = new Section();
                var meters = _meterMapper.Map(tim, isoRecords, section.Id.ReferenceId);
                section.GetMeters = () => meters;

                sections.Add(section);
            }

            return sections;
        }
    }
}
