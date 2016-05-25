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
            int tsksWithTlgs = 0;
            foreach (XmlNode node in tskNodes)
            {
                if (node.SelectNodes("TLG").Count > 0)
                {
                    tsksWithTlgs++;
                    
                    var matchingLoggedData = loggedData.SingleOrDefault(x => x.Id.FindIsoId() == node.Attributes["A"].Value);
                    AreEqual(node, currentPath, matchingLoggedData, catalog);
                }
            }

            Assert.AreEqual(tsksWithTlgs, loggedData.Count);
        }

        private static void AreEqual(XmlNode tskNode, string currentPath, LoggedData loggedData, Catalog catalog)
        {
            var tlgNodes = tskNode.SelectNodes("TLG");
            OperationDataAssert.AreEqual(tlgNodes, currentPath, loggedData.OperationData.ToList());
        }
    }
}