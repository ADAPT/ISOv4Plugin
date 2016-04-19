using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Export
{
    public class DlvHeaderAssert
    {
        public static void AreEqual(List<Meter> meters, List<DLVHeader> dlVs)
        {
            Assert.AreEqual(meters.Count(), dlVs.Count);

            foreach (var meter in meters)
            {
                //TODO: find way to get matching DLV to meter??
                //                if(meters[i].Representation != null)
                //                {
                //                    var matchingRepresentation = RepresentationManager.Instance.Representations.FirstOrDefault(x => x.DomainId == meters[i].Representation.Code);
                //                    if(matchingRepresentation != null)
                //                        Assert.AreEqual(matchingRepresentation.Ddi, timHeader.DLVs[i].ProcessDataDDI.Value);
                //                }
            }
        }
    }
}