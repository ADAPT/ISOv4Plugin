/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;

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
        public IEnumerable<SpatialValue> SpatialValues => SpatialValuesById.Where(x => x != null);
        public SpatialValue[] SpatialValuesById { get; set; }

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

            SpatialValue[] mine = SpatialValuesById;
            SpatialValue[] theirs = otherRow.SpatialValuesById;
            if (theirs != null)
            {
                if (mine == null)
                {
                    SpatialValuesById = theirs;
                }
                else
                {
                    if (theirs.Length > mine.Length)
                    {
                        var tmp = mine;
                        mine = SpatialValuesById = theirs;
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            mine[i] = tmp[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mine.Length && i < theirs.Length; i++)
                        {
                            if (mine[i] == null)
                            {
                                mine[i] = theirs[i];
                            }
                        }
                    }
                }
            }

            return this;
        }
    }
}
