using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface ISectionMapper
    {
        List<DeviceElementUse> Map(List<TIM> tims, List<ISOSpatialRow> isoRecords);
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

        public List<DeviceElementUse> Map(List<TIM> tims, List<ISOSpatialRow> isoRecords)
        {
            var sections = new List<DeviceElementUse>();

            foreach (var tim in tims)
            {
                var section = new DeviceElementUse();
                var meters = _meterMapper.Map(tim, isoRecords, section.Id.ReferenceId);
                section.GetWorkingDatas = () => meters;

                sections.Add(section);
            }

            return sections;
        }
    }
}
