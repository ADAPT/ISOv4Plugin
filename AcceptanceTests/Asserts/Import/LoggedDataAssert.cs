using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class LoggedDataAssert
    {
        public static void AreEqual(XmlNodeList tskNodes, string currentPath, List<LoggedData> loggedData, Catalog catalog)
        {
            Assert.AreEqual(tskNodes.Count, loggedData.Count);
            for (int i = 0; i < tskNodes.Count; i++)
            {
                var matchingLoggedData = loggedData.SingleOrDefault(x => x.Id.FindIsoId() == tskNodes[i].Attributes["A"].Value);
                AreEqual(tskNodes[i], currentPath, matchingLoggedData, catalog);
            }
        }

        private static void AreEqual(XmlNode tskNode, string currentPath, LoggedData loggedData, Catalog catalog)
        {
            var tlgNodes = tskNode.SelectNodes("TLG");
            OperationDataAssert.AreEqual(tlgNodes, currentPath, loggedData.OperationData.ToList());
        }
    }
}