using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Export
{
    public class TskAssert
    {
        public static void AreEqual(List<LoggedData> loggedDatas, List<TSK> tsks, Catalog catalog, string cardPath)
        {
            tsks = tsks.OrderBy(x => x.A).ToList();
            for (int i = 0; i < loggedDatas.Count; i++)
            {
                var loggedData = loggedDatas[i];
                var matchingTask = tsks[tsks.Count - loggedDatas.Count + i];
                AreEqual(loggedData, matchingTask, catalog, cardPath);
            }
        }

        private static void AreEqual(LoggedData loggedData, TSK tsk, Catalog catalog, string cardPath)
        {
            var grower = catalog.Growers.SingleOrDefault(x => x.Id.ReferenceId == loggedData.GrowerId);
            var farm = catalog.Farms.SingleOrDefault(x => x.Id.ReferenceId == loggedData.FarmId);
            var field = catalog.Fields.SingleOrDefault(x => x.Id.ReferenceId == loggedData.FieldId);

            if(grower != null)
                Assert.AreEqual(grower.Id.FindIsoId(), tsk.C);
            if (farm != null)
                Assert.AreEqual(farm.Id.FindIsoId(), tsk.D);
            if (field != null)
                Assert.AreEqual(field.Id.FindIsoId(), tsk.E);

            if(loggedData.TimeScopeIds != null && loggedData.TimeScopeIds.Any())
            {
                var timescopes = catalog.TimeScopes.Where(x => loggedData.TimeScopeIds.Contains(x.Id.ReferenceId)).ToList();
                var tims = tsk.Items.Where(x => x.GetType() == typeof(TIM)).Cast<TIM>().ToList();
                TimAssert.AreEqual(timescopes, tims);
            }

            if(loggedData.OperationData != null && loggedData.OperationData.Any())
            {
                var tlgs = tsk.Items.Where(x => x.GetType() == typeof(TLG)).Cast<TLG>().ToList();
                TlgAssert.AreEqual(loggedData.OperationData, tlgs, catalog, cardPath);
            }
        }

    }
}