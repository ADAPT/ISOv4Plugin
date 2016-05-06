using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Export
{
    public class PtnHeaderAssert
    {
        public static void AreEqual(OperationData operationData, PTNHeader ptnHeader)
        {
            Assert.AreEqual(HeaderPropertyState.IsEmpty, ptnHeader.PositionNorth.State);
            Assert.AreEqual(HeaderPropertyState.IsEmpty, ptnHeader.PositionEast.State);
            Assert.AreEqual(HeaderPropertyState.IsEmpty, ptnHeader.PositionUp.State);
            Assert.AreEqual(HeaderPropertyState.IsNull, ptnHeader.PositionStatus.State);
            Assert.AreEqual(HeaderPropertyState.IsNull, ptnHeader.PDOP.State);
            Assert.AreEqual(HeaderPropertyState.IsNull, ptnHeader.HDOP.State);
            Assert.AreEqual(HeaderPropertyState.IsNull, ptnHeader.NumberOfSatellites.State);
            Assert.AreEqual(HeaderPropertyState.IsEmpty, ptnHeader.GpsUtcTime.State);
            Assert.AreEqual(HeaderPropertyState.IsEmpty, ptnHeader.GpsUtcDate.State);
        }
    }
}