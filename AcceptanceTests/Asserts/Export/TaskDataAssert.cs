using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AcceptanceTests.Asserts.Export
{
    public class TaskDataAssert
    {
        public static void AreEqual(ApplicationDataModel applicationDataModel, ISO11783_TaskData isoTaskData, string cardPath)
        {
            var loggedData = applicationDataModel.Documents.LoggedData;
            var tasks = isoTaskData.Items.Where(x => x.GetType() == typeof (TSK)).Cast<TSK>().ToList();

            TskAssert.AreEqual(loggedData, tasks, applicationDataModel.Catalog, cardPath);
        }
    }
}