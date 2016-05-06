using System;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class ISOSpatialRow
    {
        public DateTime TimeStart { get; set; }
        public int NorthPosition { get; set; }
        public int EastPosition { get; set; }
        public int? Elevation { get; set; }
        public byte? PositionStatus { get; set; }
        public short? PDOP { get; set; }
        public short? HDOP { get; set; }
        public byte? NumberOfSatellites { get; set; }
        public int? GpsUtcTime { get; set;  }
        public short? GpsUtcDate { get; set; }
        public DateTime? GpsUtcDateTime { get; set; }
        public List<SpatialValue> SpatialValues {get; set; }
    }
}