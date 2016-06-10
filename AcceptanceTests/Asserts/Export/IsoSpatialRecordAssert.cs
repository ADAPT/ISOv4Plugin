using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Export
{
    public class IsoSpatialRecordAssert
    {
        private const double CoordinateMultiplier = 0.0000001;

        public static void AreEqual(IEnumerable<SpatialRecord> adaptSpatialRecords, List<WorkingData> meters, IEnumerable<ISOSpatialRow> isoSpatialRecords)
        {
            using (var adaptSpatialRecordEnumerator = adaptSpatialRecords.GetEnumerator())
            using (var isoSpatialRecordEnumerator = isoSpatialRecords.GetEnumerator())
            {
                while (adaptSpatialRecordEnumerator.MoveNext())
                {
                    isoSpatialRecordEnumerator.MoveNext();
                    AreEqual(adaptSpatialRecordEnumerator.Current, isoSpatialRecordEnumerator.Current, meters);
                }
            }
        }

        private static void AreEqual(SpatialRecord adaptSpatialRecord, ISOSpatialRow isoSpatialRow, List<WorkingData> meters)
        {
            Assert.AreEqual(adaptSpatialRecord.Timestamp, isoSpatialRow.TimeStart);

            var point = (Point)adaptSpatialRecord.Geometry;
            Assert.AreEqual((int)(point.X / CoordinateMultiplier), isoSpatialRow.EastPosition, CoordinateMultiplier);
            Assert.AreEqual((int)(point.Y / CoordinateMultiplier), isoSpatialRow.NorthPosition, CoordinateMultiplier);
            Assert.AreEqual(point.Z, isoSpatialRow.Elevation);

            SpatialValueAssert.AreEqual(isoSpatialRow, adaptSpatialRecord, meters);
        }
    }
}