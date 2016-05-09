using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface INumericValueMapper
    {
        UInt32 Map(NumericMeter meter, SpatialRecord spatialRecord);
    }

    public class NumericValueMapper : INumericValueMapper
    {
        private readonly IRepresentationMapper _representationMapper;
        private readonly Dictionary<int, DdiDefinition> _ddis;

        public NumericValueMapper() : this(new RepresentationMapper())
        {
            
        }

        public NumericValueMapper(IRepresentationMapper representationMapper)
        {
            _representationMapper = representationMapper;
            _ddis = DdiLoader.Ddis;
        }

        public uint Map(NumericMeter meter, SpatialRecord spatialRecord)
        {
            var value = (NumericRepresentationValue)spatialRecord.GetMeterValue(meter);
            if (value == null)
                return 0;

            var resolution = GetResolution(value);
            return (UInt32)(value.Value.Value / resolution);
        }

        private double GetResolution(NumericRepresentationValue value)
        {
            var ddi = _representationMapper.Map(value.Representation);
            var resolution = 1d;
            if (_ddis.ContainsKey(ddi.GetValueOrDefault()))
            {
                resolution = _ddis[ddi.GetValueOrDefault()].Resolution;
            }
            return resolution;
        }
    }
}
