using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ISpatialRecordMapper
    {
        IEnumerable<SpatialRecord> Map(IEnumerable<ISOSpatialRow> isoSpatialRows, List<WorkingData> meters, Dictionary<string, List<ISOProductAllocation>> productAllocations);
        SpatialRecord Map(ISOSpatialRow isoSpatialRow, List<WorkingData> meters, Dictionary<string, List<ISOProductAllocation>> productAllocations);
    }

    public class SpatialRecordMapper : ISpatialRecordMapper
    {
        // ATTENTION: CoordinateMultiplier and ZMultiplier also exist in TimeLogMapper.cs!
        private const double CoordinateMultiplier = 0.0000001;
        private const double ZMultiplier = 0.001;   // In ISO the PositionUp value is specified in mm.
        private readonly IRepresentationValueInterpolator _representationValueInterpolator;
        private readonly IWorkingDataMapper _workingDataMapper;
        private readonly ISectionMapper _sectionMapper;
        private readonly TaskDataMapper _taskDataMapper;
        private double? _effectiveTimeZoneOffset;

        public SpatialRecordMapper(IRepresentationValueInterpolator representationValueInterpolator, ISectionMapper sectionMapper, IWorkingDataMapper workingDataMapper, TaskDataMapper taskDataMapper)
        {
            _representationValueInterpolator = representationValueInterpolator;
            _workingDataMapper = workingDataMapper;
            _sectionMapper = sectionMapper;
            _taskDataMapper = taskDataMapper;
        }

        public IEnumerable<SpatialRecord> Map(IEnumerable<ISOSpatialRow> isoSpatialRows, List<WorkingData> meters, Dictionary<string, List<ISOProductAllocation>> productAllocations)
        {
            //Compare the first spatial record to product allocations in case there is a mismatch between local time and UTC in comparing dates
            //This value will reflect an offset between the processing computer's timezone settings vs. the actual offset of the data itself,
            //and serves to provide a correction if PANs were reported as UTC.
            //This code will require an exact minute/second match between the first record and one of the PANs to take effect
            ISOSpatialRow firstSpatialRow = isoSpatialRows.FirstOrDefault();
            if (firstSpatialRow != null && productAllocations.Any())
            {
                foreach (ISOProductAllocation pan in productAllocations.First().Value)
                {
                    if (pan.AllocationStamp?.Start != null &&
                        pan.AllocationStamp.Start.Value.Minute == firstSpatialRow.TimeStart.Minute &&
                        pan.AllocationStamp.Start.Value.Second == firstSpatialRow.TimeStart.Second)
                    {
                        _effectiveTimeZoneOffset = (firstSpatialRow.TimeStart - pan.AllocationStamp.Start.Value).TotalHours;
                    }
                }
            }

            _representationValueInterpolator.Clear();

            return isoSpatialRows.Select(r => Map(r, meters, productAllocations));
        }

        public SpatialRecord Map(ISOSpatialRow isoSpatialRow, List<WorkingData> meters, Dictionary<string, List<ISOProductAllocation>> productAllocations)
        {
            var spatialRecord = new SpatialRecord();

            spatialRecord.Geometry = new ApplicationDataModel.Shapes.Point
            {
                X = Convert.ToDouble(isoSpatialRow.EastPosition * CoordinateMultiplier),
                Y = Convert.ToDouble(isoSpatialRow.NorthPosition * CoordinateMultiplier),
                Z = Convert.ToDouble(isoSpatialRow.Elevation.GetValueOrDefault() * ZMultiplier)
            };

            spatialRecord.Timestamp = isoSpatialRow.TimeStart;

            foreach (var meter in meters.OfType<NumericWorkingData>())
            {
                SetNumericMeterValue(isoSpatialRow, meter, spatialRecord, productAllocations);
            }

            foreach (var meter in meters.OfType<EnumeratedWorkingData>())
            {
                SetEnumeratedMeterValue(isoSpatialRow, meter, spatialRecord);
            }

            return spatialRecord;
        }

        private void SetEnumeratedMeterValue(ISOSpatialRow isoSpatialRow, EnumeratedWorkingData meter, SpatialRecord spatialRecord)
        {
            var isoDataLogValue = _workingDataMapper.DataLogValuesByWorkingDataID[meter.Id.ReferenceId];
            var isoValue = isoSpatialRow.SpatialValues.FirstOrDefault(v =>
                    v.DataLogValue.DeviceElementIdRef == isoDataLogValue.DeviceElementIdRef &&
                    v.DataLogValue.ProcessDataDDI == isoDataLogValue.ProcessDataDDI);
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

        private void SetNumericMeterValue(ISOSpatialRow isoSpatialRow, NumericWorkingData meter, SpatialRecord spatialRecord, Dictionary<string, List<ISOProductAllocation>> productAllocations)
        {
            var dataLogValue = _workingDataMapper.DataLogValuesByWorkingDataID.ContainsKey(meter.Id.ReferenceId)
                ? _workingDataMapper.DataLogValuesByWorkingDataID[meter.Id.ReferenceId]
                : null;
            var isoValue = dataLogValue != null
                           ? isoSpatialRow.SpatialValues.FirstOrDefault(v =>
                                v.DataLogValue.ProcessDataDDI != "DFFE" &&
                                v.DataLogValue.DeviceElementIdRef == dataLogValue.DeviceElementIdRef &&
                                v.DataLogValue.ProcessDataDDI == dataLogValue.ProcessDataDDI)
                           : null;


            if (isoValue != null)
            {
                ADAPT.ApplicationDataModel.Common.UnitOfMeasure userProvidedUnitOfMeasure = meter.UnitOfMeasure; //Default; no display uom provided.
                var dvp = isoValue.DeviceProcessData?.DeviceValuePresentation;
                if (dvp != null)
                {
                    //If a DVP element is present, report out the desired display unit of measure as the UserProvidedUnitOfMeasure.
                    //This will not necessarily map to the Representation.UnitSystem.   
                    userProvidedUnitOfMeasure = new ApplicationDataModel.Common.UnitOfMeasure() { Code = dvp.UnitDesignator, Scale = dvp.Scale, Offset = dvp.Offset };
                }

                var value = new NumericRepresentationValue(meter.Representation as NumericRepresentation, userProvidedUnitOfMeasure, new NumericValue(meter.UnitOfMeasure, isoValue.Value));
                spatialRecord.SetMeterValue(meter, value);

                var other = new NumericRepresentationValue(meter.Representation as NumericRepresentation, userProvidedUnitOfMeasure, new NumericValue(meter.UnitOfMeasure, isoValue.Value));
                _representationValueInterpolator.SetMostRecentMeterValue(meter, other);
            }
            else if (meter.Representation.Code == "vrProductIndex")
            {
                string detID = _workingDataMapper.ISODeviceElementIDsByWorkingDataID[meter.Id.ReferenceId];
                if (productAllocations.ContainsKey(detID)) //The DeviceElement for this meter exists in the list of allocations
                {
                    var productAllocationsForDeviceElement = productAllocations[detID];
                    double numericValue = 0d;
                    if (productAllocationsForDeviceElement.Count == 1 || TimeLogMapper.GetDistinctProductIDs(_taskDataMapper, productAllocations).Count == 1)
                    {
                        //This product is consistent throughout the task on this device element
                        int? adaptProductID = _taskDataMapper.InstanceIDMap.GetADAPTID(productAllocationsForDeviceElement.Single().ProductIdRef);
                        numericValue = adaptProductID.HasValue ? adaptProductID.Value : 0d;

                    }
                    else if (productAllocationsForDeviceElement.Count > 1)
                    {
                        //There are multiple product allocations for the device element
                        //Find the product allocation that governs this timestamp
                        ISOProductAllocation relevantPan = productAllocationsForDeviceElement.FirstOrDefault(p => Offset(p.AllocationStamp.Start) <= spatialRecord.Timestamp &&
                                                                                                         (p.AllocationStamp.Stop == null ||
                                                                                                          Offset(p.AllocationStamp.Stop) >= spatialRecord.Timestamp));
                        if (relevantPan == null)
                        {
                            //We couldn't correlate strictly based on time.  Check for a more general match on date alone before returning null.
                            var pansMatchingDate = productAllocationsForDeviceElement.Where(p => p.AllocationStamp.Start?.Date == p.AllocationStamp.Stop?.Date &&
                                                                               p.AllocationStamp.Start?.Date == spatialRecord.Timestamp.Date);
                            if (pansMatchingDate.Count() == 1)
                            {
                                //Only one PAN on this date, use it.
                                relevantPan = pansMatchingDate.Single();
                            }
                        }

                        if (relevantPan != null)
                        {
                            int? adaptProductID = _taskDataMapper.InstanceIDMap.GetADAPTID(relevantPan.ProductIdRef);
                            numericValue = adaptProductID.HasValue ? adaptProductID.Value : 0d;
                        }
                    }
                    var value = new NumericRepresentationValue(meter.Representation as NumericRepresentation, meter.UnitOfMeasure, new NumericValue(meter.UnitOfMeasure, numericValue));
                    spatialRecord.SetMeterValue(meter, value);
                }
            }
            else
            {
                var value = _representationValueInterpolator.Interpolate(meter) as NumericRepresentationValue;
                spatialRecord.SetMeterValue(meter, value);
            }
        }

        private DateTime? Offset(DateTime? input)
        {
            if (_effectiveTimeZoneOffset.HasValue && input.HasValue)
            {
                return input.Value.AddHours(_effectiveTimeZoneOffset.Value);
            }
            return input;
        }
    }
}
