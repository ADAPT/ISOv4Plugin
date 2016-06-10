using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface ISpatialRecordMapper
    {
        IEnumerable<SpatialRecord> Map(IEnumerable<ISOSpatialRow> isoSpatialRows, List<WorkingData> meters);
    }

    public class SpatialRecordMapper : ISpatialRecordMapper
    {
        private const double CoordinateMultiplier = 0.0000001;
        private readonly IRepresentationValueInterpolator _representationValueInterpolator;

        public SpatialRecordMapper(IRepresentationValueInterpolator representationValueInterpolator)
        {
            _representationValueInterpolator = representationValueInterpolator;
        }

        public SpatialRecordMapper() : this(new RepresentationValueInterpolator())
        {

        }

        public IEnumerable<SpatialRecord> Map(IEnumerable<ISOSpatialRow> isoSpatialRows, List<WorkingData> meters)
        {
            _representationValueInterpolator.Clear();
            return isoSpatialRows.Select(r => Map(r, meters));
        }

        public SpatialRecord Map(ISOSpatialRow isoSpatialRow, List<WorkingData> meters)
        {
            var spatialRecord = new SpatialRecord();

            foreach (var meter in meters.OfType<NumericWorkingData>())
            {
                SetNumericMeterValue(isoSpatialRow, meter, spatialRecord);
            }

            foreach (var meter in meters.OfType<EnumeratedWorkingData>())
            {
                SetEnumeratedMeterValue(isoSpatialRow, meter, spatialRecord);
            }

            spatialRecord.Geometry = new ApplicationDataModel.Shapes.Point
            {
                X = Convert.ToDouble(isoSpatialRow.EastPosition * CoordinateMultiplier),
                Y = Convert.ToDouble(isoSpatialRow.NorthPosition * CoordinateMultiplier),
                Z = isoSpatialRow.Elevation
            };

            spatialRecord.Timestamp = isoSpatialRow.TimeStart;
            
            return spatialRecord;
        }

        private void SetEnumeratedMeterValue(ISOSpatialRow isoSpatialRow, EnumeratedWorkingData meter, SpatialRecord spatialRecord)
        {
            var isoValue = isoSpatialRow.SpatialValues.SingleOrDefault(v => v.Id == meter.Id.FindIntIsoId());
            if (isoValue != null)
            {
                var isoEnumeratedMeter = meter as ISOEnumeratedMeter;
                var enumeratedValue = isoEnumeratedMeter.GetEnumeratedValue(isoValue, isoEnumeratedMeter);
                spatialRecord.SetMeterValue(meter, enumeratedValue);
                _representationValueInterpolator.SetMostRecentMeterValue(meter, enumeratedValue);
            }
            else
            {
                var value = _representationValueInterpolator.Interpolate(meter) as EnumeratedValue;
                spatialRecord.SetMeterValue(meter, value);
            }
        }

        private void SetNumericMeterValue(ISOSpatialRow isoSpatialRow, NumericWorkingData meter, SpatialRecord spatialRecord)
        {

            var isoValue = isoSpatialRow.SpatialValues.SingleOrDefault(v => v.Id == meter.Id.FindIntIsoId());

            
            if (isoValue != null)
            {
                var value = new NumericRepresentationValue(meter.Representation as NumericRepresentation, meter.UnitOfMeasure, new NumericValue(meter.UnitOfMeasure, isoValue.Value));
                spatialRecord.SetMeterValue(meter, value);

                var other = new NumericRepresentationValue(meter.Representation as NumericRepresentation, meter.UnitOfMeasure, new NumericValue(meter.UnitOfMeasure, isoValue.Value));
                _representationValueInterpolator.SetMostRecentMeterValue(meter, other);
            }
            else
            {
                var value = _representationValueInterpolator.Interpolate(meter) as NumericRepresentationValue;
                spatialRecord.SetMeterValue(meter, value);
            }
        }
    }
}
