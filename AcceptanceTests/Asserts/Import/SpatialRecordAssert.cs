using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class SpatialRecordAssert
    {
        private const double CoordinateMultiplier = 0.0000001;

        public static void AreEqual(IEnumerable<ISOSpatialRow> isoSpatialRows, IEnumerable<SpatialRecord> adaptSpatialRecords, IEnumerable<WorkingData> meters)
        {
            Assert.AreEqual(isoSpatialRows.Count(), adaptSpatialRecords.Count());
            for (int i = 0; i < isoSpatialRows.Count(); i++)
            {
                AreEqual(isoSpatialRows.ElementAt(i), adaptSpatialRecords.ElementAt(i), meters);
            }
        }

        private static void AreEqual(ISOSpatialRow isoSpatialRow, SpatialRecord adaptSpatialRecord, IEnumerable<WorkingData> meters)
        {
            Assert.AreEqual(isoSpatialRow.TimeStart, adaptSpatialRecord.Timestamp);

            var point = adaptSpatialRecord.Geometry as Point;
            Assert.AreEqual(isoSpatialRow.EastPosition * CoordinateMultiplier, point.X, 0.000001);
            Assert.AreEqual(isoSpatialRow.NorthPosition * CoordinateMultiplier, point.Y, 0.000001);
            Assert.AreEqual(isoSpatialRow.Elevation, point.Z);

            SpatialValueAssert.AreEqual(isoSpatialRow, adaptSpatialRecord, meters);
        }
    }
}