using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface ISectionMapper
    {
        List<DeviceElementUse> Map(List<TIM> tims, List<ISOSpatialRow> isoRecords);
        List<DeviceElementUse> ConvertToBaseTypes(List<DeviceElementUse> meters);
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

        public List<DeviceElementUse> ConvertToBaseTypes(List<DeviceElementUse> sections)
        {
            return sections.Select(x => {
                var section = new DeviceElementUse();
                var meters = x.GetWorkingDatas().Select(y => _meterMapper.ConvertToBaseType(y)).ToList();
                section.GetWorkingDatas = () => meters;
                return section;
                }).ToList();
        }

    }
}
