/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

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
        public ushort? PDOP { get; set; }
        public ushort? HDOP { get; set; }
        public byte? NumberOfSatellites { get; set; }
        public uint? GpsUtcTime { get; set;  }
        public ushort? GpsUtcDate { get; set; }
        public DateTime? GpsUtcDateTime { get; set; }
        public List<SpatialValue> SpatialValues { get; set; }

        /// <summary>
        /// Merge SpatialValues from provided SpatialRow into this one.
        /// </summary>
        /// <param name="otherRow"></param>
        /// <returns></returns>
        public ISOSpatialRow Merge(ISOSpatialRow otherRow)
        {
            if (otherRow == null)
            {
                return this;
            }

            if (SpatialValues == null)
            {
                SpatialValues = new List<SpatialValue>();
            }
            if (otherRow.SpatialValues != null)
            {
                SpatialValues.AddRange(otherRow.SpatialValues);
            }

            return this;
        }
    }
}
