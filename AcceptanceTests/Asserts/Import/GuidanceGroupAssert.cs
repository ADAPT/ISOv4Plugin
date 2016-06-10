using System;
using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;

namespace AcceptanceTests.Asserts.Import
{
    public class GuidanceGroupAssert
    {
        public static void AreEqual(XmlNodeList ggpNodes, IEnumerable<GuidanceGroup> guidanceGroups, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            throw new NotImplementedException();
        }
    }
}