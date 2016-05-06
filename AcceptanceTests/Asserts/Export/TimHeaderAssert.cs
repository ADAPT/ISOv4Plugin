using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Export
{
    public class TimHeaderAssert
    {
        public static void AreEqual(OperationData operationData, TIMHeader timHeader)
        {
            Assert.AreEqual(HeaderPropertyState.IsEmpty, timHeader.Start.State);
            Assert.AreEqual(HeaderPropertyState.IsNull, timHeader.Stop.State);
            Assert.AreEqual(HeaderPropertyState.IsNull, timHeader.Duration.State);
            Assert.AreEqual(HeaderPropertyState.HasValue, timHeader.Type.State);
            Assert.AreEqual(((int)TIMD.Item4).ToString(), timHeader.Type.Value);

            PtnHeaderAssert.AreEqual(operationData, timHeader.PtnHeader);

            var meters = operationData.GetSections(0).SelectMany(x => x.GetMeters()).ToList();
            DlvHeaderAssert.AreEqual(meters, timHeader.DLVs);
        }
    }
}