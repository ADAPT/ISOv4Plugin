using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class UniqueIdAssert
    {
        public static void AreEqual(Dictionary<string, List<UniqueId>> linkList, string isoRefId, List<UniqueId> adaptIds)
        {
            if(!linkList.ContainsKey(isoRefId))
                return;

            var links = linkList[isoRefId];

            foreach (var link in links)
            {
                var matchingAdaptId = adaptIds.Single(x => x.Id == link.Id);
                Assert.AreEqual(link.Source, matchingAdaptId.Source);
                Assert.AreEqual(link.SourceType, matchingAdaptId.SourceType);
                Assert.AreEqual(link.IdType, matchingAdaptId.IdType);
            }
        }
    }
}