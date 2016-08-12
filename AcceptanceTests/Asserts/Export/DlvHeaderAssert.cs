using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Export
{
    public class DlvHeaderAssert
    {
        public static void AreEqual(List<WorkingData> meters, List<DLV> dlVs)
        {
            Assert.AreEqual(meters.Count(), dlVs.Count);

            var sortedMeters = meters.OrderBy(x => x.Id.FindIntIsoId()).ToList();
            for (int i = 0; i < sortedMeters.Count; i++)
            {
                var meter = sortedMeters[i];
                var dlv = dlVs[i];
                var matchingRepresentation = RepresentationManager.Instance.Representations.FirstOrDefault(x => x.DomainId == meter.Representation.Code);
                if (matchingRepresentation != null)
                    Assert.AreEqual(matchingRepresentation.Ddi, dlv.A);
            }
        }
    }
}