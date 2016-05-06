using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class LoggedDataAssert
    {
        public static void AreEqual(List<TSK> tsks, string currentPath, List<LoggedData> loggedData, Catalog catalog)
        {
            Assert.AreEqual(tsks.Count, loggedData.Count);
            foreach (var tsk in tsks)
            {
                var matchingLoggedData = loggedData.SingleOrDefault(x => x.Id.FindIsoId() == tsk.A);
                AreEqual(tsk, currentPath, matchingLoggedData, catalog);
            }
        }

        public static void AreEqual(TSK tsk, string currentPath, LoggedData loggedData, Catalog catalog)
        {
            var tlgs = tsk.Items.Where(x => x.GetType() == typeof(TLG)).Cast<TLG>().ToList();
            OperationDataAssert.AreEqual(tlgs, currentPath, loggedData.OperationData.ToList());
        }
    }
}